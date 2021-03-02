using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Sender;

namespace Morestachio.MailProcessor.Framework
{
	public readonly struct SendMailProgress
	{
		public SendMailProgress(string to, int progress, int maxProgress, bool success)
		{
			To = to;
			Progress = progress;
			MaxProgress = maxProgress;
			Success = success;
		}

		public int Progress { get; }
		public int MaxProgress { get; }
		public string To { get; }
		public bool Success { get; }
	}

	public readonly struct SendMailTaskProgress
	{
		public SendMailTaskProgress(string to)
		{
			To = to;
			Success = true;
			Error = null;
		}

		public SendMailTaskProgress(string to, string error)
		{
			To = to;
			Success = false;
			Error = error;
		}

		public string To { get; }
		public bool Success { get; }
		public string Error { get; }
	}

	public class MailComposer
	{
		public MailComposer()
		{
			ParallelNoOfParallism = Environment.ProcessorCount;
			ParallelReadAheadCount = ParallelNoOfParallism * 2;
		}

		public bool SendInParallel { get; set; }

		public IMailDataStrategy MailDataStrategy { get; set; }
		public IMailDistributor MailDistributor { get; set; }

		public string Template { get; set; }

		public IMorestachioExpression AddressExpression { get; set; }
		public IMorestachioExpression NameExpression { get; set; }
		public IMorestachioExpression SubjectExpression { get; set; }

		public int ParallelReadAheadCount { get; set; }
		public int ParallelNoOfParallism { get; set; }

		public async Task<DistributorData> Compose(
			MailData mailData,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression)
		{
			var context = new ContextObject(parsedTemplate?.ParserOptions ?? new ParserOptions(), ".", null, mailData.Data);

			return new DistributorData
			{
				Address = compiledAddressExpression != null ? (await compiledAddressExpression(context, new ScopeData())).Value.ToString() : null,
				To = compiledNameExpression != null ? (await compiledNameExpression(context, new ScopeData())).Value.ToString() : null,
				Subject = compiledSubjectExpression != null ? (await compiledSubjectExpression(context, new ScopeData())).Value.ToString() : null,
				Content = parsedTemplate != null ? (await parsedTemplate.CreateAsync(mailData.Data)).Stream : null,
			};
		}

		public async Task<ComposeMailResult> ComposeAndSend(IProgress<SendMailProgress> progress)
		{
			var compiledAddressExpression = AddressExpression.Compile();
			var compiledNameExpression = NameExpression.Compile();
			var compiledSubjectExpression = SubjectExpression.Compile();

			var parsedTemplate = await Parser.ParseWithOptionsAsync(new ParserOptions(Template));
			var sendData = 0;
			var sendFailed = new ConcurrentDictionary<MailData, SendMailResult>();
			var mailDatas = await MailDataStrategy.GetMails();
			var maxSendData = await MailDataStrategy.Count();


			void Progress(SendMailTaskProgress taskProgress, MailData mailData)
			{
				Interlocked.Add(ref sendData, 1);
				if (!taskProgress.Success)
				{
					sendFailed.TryAdd(mailData, new SendMailResult() { Error = taskProgress.Error });
				}

				progress.Report(new SendMailProgress(taskProgress.To, sendData, maxSendData, true));
			}

			if (SendInParallel)
			{
				await SendParallel(mailDatas,
					Progress,
					parsedTemplate,
					compiledAddressExpression,
					compiledSubjectExpression, compiledNameExpression);
			}
			else
			{
				await foreach (var mailData in mailDatas)
				{
					await SendSingleItem(taskProgress => Progress(taskProgress, mailData),
						mailData,
						parsedTemplate,
						compiledAddressExpression,
						compiledSubjectExpression,
						compiledNameExpression);
				}
			}

			var resultData = new ComposeMailResult();
			resultData.SendSuccessfully = sendData;
			resultData.SendFailed = sendFailed.ToDictionary(e => e.Key, e => e.Value);
			return resultData;
		}

		private async Task SendParallel(IAsyncEnumerable<MailData> producer,
			Action<SendMailTaskProgress, MailData> progress,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression)
		{
			var consumerItems = new BlockingCollection<MailData>();

			var threads = new Thread[ParallelNoOfParallism];
			for (int i = 0; i < threads.Length; i++)
			{
				var thread = new Thread(async () =>
				{
					foreach (var mailData in consumerItems.GetConsumingEnumerable())
					{
						await SendSingleItem(taskProgress => progress(taskProgress, mailData), mailData, parsedTemplate, compiledAddressExpression,
							compiledSubjectExpression, compiledNameExpression);
					}
				});
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.Normal;
				thread.TrySetApartmentState(ApartmentState.MTA);
				threads[i] = thread;
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			var readAheadCount = ParallelReadAheadCount;
			await foreach (var item in producer)
			{
				consumerItems.Add(item);
				while (consumerItems.Count >= readAheadCount)
				{
					await Task.Delay(250);
				}
			}

			consumerItems.CompleteAdding();

			foreach (var thread in threads)
			{
				thread.Join();
			}
		}

		private async Task SendSingleItem(
			Action<SendMailTaskProgress> progress,
			MailData mailData,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression)
		{
			var distributorData = await Compose(mailData, parsedTemplate, compiledAddressExpression,
				compiledSubjectExpression,
				compiledNameExpression);
			var sendMailResult = await MailDistributor.SendMail(distributorData);
			if (!sendMailResult.Success)
			{
				progress(new SendMailTaskProgress(distributorData.Address, sendMailResult.Error));
			}
			else
			{
				progress(new SendMailTaskProgress(distributorData.Address));
			}
		}
	}

	public class ComposeMailResult
	{
		public int SendSuccessfully { get; set; }
		public IDictionary<MailData, SendMailResult> SendFailed { get; set; }
	}
}