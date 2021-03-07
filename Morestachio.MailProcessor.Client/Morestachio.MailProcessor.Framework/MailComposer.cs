using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Linq;
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

		public IMorestachioExpression ToAddressExpression { get; set; }
		public IMorestachioExpression ToNameExpression { get; set; }
		public IMorestachioExpression SubjectExpression { get; set; }
		public IMorestachioExpression FromAddressExpression { get; set; }
		public IMorestachioExpression FromNameExpression { get; set; }

		public int ParallelReadAheadCount { get; set; }
		public int ParallelNoOfParallism { get; set; }


		public async Task<DistributorData> Compose(
			MailData mailData,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression,
			CompiledExpression compiledFromAddressExpression,
			CompiledExpression compiledFromNameExpression)
		{
			var context = new ContextObject(parsedTemplate?.ParserOptions ?? new ParserOptions(), ".", null, mailData);

			return new DistributorData
			{
				ToAddress = compiledAddressExpression != null ? (await compiledAddressExpression(context, new ScopeData())).Value.ToString() : null,
				To = compiledNameExpression != null ? (await compiledNameExpression(context, new ScopeData())).Value.ToString() : null,
				FromAddress = compiledFromAddressExpression != null ? (await compiledFromAddressExpression(context, new ScopeData())).Value.ToString() : null,
				From = compiledFromNameExpression != null ? (await compiledFromNameExpression(context, new ScopeData())).Value.ToString() : null,
				Subject = compiledSubjectExpression != null ? (await compiledSubjectExpression(context, new ScopeData())).Value.ToString() : null,
				Content = parsedTemplate != null ? (await parsedTemplate.CreateAsync(mailData)).Stream : null,
			};
		}

		public async Task<ComposeMailResult> ComposeAndSend(IProgress<SendMailProgress> progress)
		{
			var compiledAddressExpression = ToAddressExpression.Compile();
			var compiledNameExpression = ToNameExpression.Compile();
			var compiledSubjectExpression = SubjectExpression.Compile();
			var compiledFromAddressExpression = FromAddressExpression.Compile();
			var compiledFromNameExpression = FromNameExpression.Compile();

			var parsingOptions = new ParserOptions(Template);
			parsingOptions.Timeout = TimeSpan.FromMinutes(1);
			parsingOptions.Formatters.AddFromType(typeof(LinqFormatter));
			parsingOptions.Formatters.AddFromType(typeof(DynamicLinq));
			var parsedTemplate = await Parser.ParseWithOptionsAsync(parsingOptions);
			var sendData = 0;
			var sendFailed = new ConcurrentDictionary<MailData, SendMailStatus>();
			var maxSendData = await MailDataStrategy.Count();
			var mailDatas = await MailDataStrategy.GetMails();

			void Progress(SendMailTaskProgress taskProgress, MailData mailData)
			{
				Interlocked.Add(ref sendData, 1);
				if (!taskProgress.Success)
				{
					sendFailed.TryAdd(mailData, new SendMailStatus() { ErrorText = taskProgress.Error });
				}

				progress.Report(new SendMailProgress(taskProgress.To, sendData, maxSendData, true));
			}

			if (SendInParallel)
			{
				await SendParallel(mailDatas,
					Progress,
					parsedTemplate,
					compiledAddressExpression,
					compiledSubjectExpression,
					compiledNameExpression,
					compiledFromAddressExpression,
					compiledFromNameExpression);
			}
			else
			{
				var beginSend = await MailDistributor.BeginSendMail();
				await foreach (var mailData in mailDatas)
				{
					await SendSingleItem(taskProgress => Progress(taskProgress, mailData),
						mailData,
						parsedTemplate,
						compiledAddressExpression,
						compiledSubjectExpression,
						compiledNameExpression,
						compiledFromAddressExpression,
						compiledFromNameExpression,
						beginSend);
				}

				await MailDistributor.EndSendMail(beginSend);
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
			CompiledExpression compiledNameExpression,
			CompiledExpression compiledFromAddressExpression,
			CompiledExpression compiledFromNameExpression)
		{
			var consumerItems = new BlockingCollection<MailData>();
			IMailDistributorState globalState = null;

			if (MailDistributor.ParallelSupport == ParallelSupport.Full)
			{
				globalState = await MailDistributor.BeginSendMail();
			}

			var threads = new Thread[ParallelNoOfParallism];
			for (int i = 0; i < threads.Length; i++)
			{
				async void Start()
				{
					IMailDistributorState threadState = globalState;
					if (MailDistributor.ParallelSupport == ParallelSupport.MultiInstance)
					{
						threadState = await MailDistributor.BeginSendMail();
					}

					foreach (var mailData in consumerItems.GetConsumingEnumerable())
					{
						await SendSingleItem(taskProgress => progress(taskProgress, mailData),
							mailData,
							parsedTemplate,
							compiledAddressExpression,
							compiledSubjectExpression,
							compiledNameExpression,
							compiledFromAddressExpression,
							compiledFromNameExpression,
							threadState);
					}

					if (MailDistributor.ParallelSupport == ParallelSupport.MultiInstance)
					{
						threadState = await MailDistributor.EndSendMail(threadState);
					}
				}

				var thread = new Thread(Start);
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


			if (MailDistributor.ParallelSupport == ParallelSupport.Full)
			{
				globalState = await MailDistributor.EndSendMail(globalState);
			}
		}

		private async Task SendSingleItem(Action<SendMailTaskProgress> progress,
			MailData mailData,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression,
			CompiledExpression compiledFromAddressExpression,
			CompiledExpression compiledFromNameExpression,
			IMailDistributorState state)
		{
			var distributorData = await Compose(mailData, parsedTemplate, compiledAddressExpression,
				compiledSubjectExpression,
				compiledNameExpression,
				compiledFromAddressExpression,
				compiledFromNameExpression);
			var sendMailResult = await MailDistributor.SendMail(distributorData, state);
			if (!sendMailResult.Success)
			{
				progress(new SendMailTaskProgress(distributorData.ToAddress, sendMailResult.ErrorText));
			}
			else
			{
				progress(new SendMailTaskProgress(distributorData.ToAddress));
			}
		}
	}

	public class ComposeMailResult
	{
		public int SendSuccessfully { get; set; }
		public IDictionary<MailData, SendMailStatus> SendFailed { get; set; }
	}
}