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
		public SendMailProgress(string to,
			int progress,
			int maxProgress,
			bool success,
			int successfullySend,
			int failedToSend,
			int buffered)
		{
			To = to;
			Progress = progress;
			MaxProgress = maxProgress;
			Success = success;
			SuccessfullySend = successfullySend;
			FailedToSend = failedToSend;
			Buffered = buffered;
		}

		public int Progress { get; }
		public int SuccessfullySend { get; }
		public int FailedToSend { get; }
		public int Buffered { get; }

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
			mailData.MailInfo.ToName = compiledNameExpression != null
				? (await compiledNameExpression(context, new ScopeData())).Value.ToString()
				: null;
			mailData.MailInfo.ToAddress = compiledAddressExpression != null
				? (await compiledAddressExpression(context, new ScopeData())).Value.ToString()
				: null;
			mailData.MailInfo.FromAddress = compiledFromAddressExpression != null
				? (await compiledFromAddressExpression(context, new ScopeData())).Value.ToString()
				: null;
			mailData.MailInfo.FromName = compiledFromNameExpression != null
				? (await compiledFromNameExpression(context, new ScopeData())).Value.ToString()
				: null;
			mailData.MailInfo.Subject = compiledSubjectExpression != null
				? (await compiledSubjectExpression(context, new ScopeData())).Value.ToString()
				: null;

			return new DistributorData(mailData.MailInfo,
				parsedTemplate != null ? (await parsedTemplate.CreateAsync(mailData)) : null);
		}

		public async Task<ComposeMailResult> ComposeAndSend(IProgress<SendMailProgress> progress,
			CancellationToken stopRequestedToken)
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
			var sendSuccess = 0;
			var buffered = 0;

			var sendFailed = new ConcurrentDictionary<MailData, SendMailStatus>();
			var maxSendData = await MailDataStrategy.Count();
			var mailDatas = await MailDataStrategy.GetMails();

#if DEBUG
			var rand = new Random(1337);
#endif

			void Progress(SendMailTaskProgress taskProgress, MailData mailData)
			{
				Interlocked.Add(ref sendData, 1);
				if (!taskProgress.Success)
				{
					sendFailed.TryAdd(mailData, new SendMailStatus() { ErrorText = taskProgress.Error });
				}
				else
				{
					Interlocked.Add(ref sendSuccess, 1);
				}

				progress.Report(new SendMailProgress(taskProgress.To, sendData, maxSendData, true, sendSuccess, sendFailed.Count, buffered));
			}

			if (SendInParallel)
			{
				var consumerItems = new BlockingCollection<MailData>(new ConcurrentBag<MailData>());
				IMailDistributorState globalState = null;

				if (MailDistributor.ParallelSupport == ParallelSupport.Full)
				{
					globalState = await MailDistributor.BeginSendMail();
				}

				var threads = new Thread[ParallelNoOfParallism];
				for (int i = 0; i < threads.Length; i++)
				{
					void Start()
					{
						try
						{
							IMailDistributorState threadState = globalState;
							if (MailDistributor.ParallelSupport == ParallelSupport.MultiInstance)
							{
								threadState = MailDistributor.BeginSendMail().ConfigureAwait(true).GetAwaiter().GetResult();
							}

							while (consumerItems.TryTake(out var mailData))
							{
								Interlocked.Exchange(ref buffered, consumerItems.Count);
								if (stopRequestedToken.IsCancellationRequested)
								{
									break;
								}
#if DEBUG
								Task.Delay(rand.Next(1050, 2000), stopRequestedToken)
									.ConfigureAwait(true).GetAwaiter().GetResult();
#endif
								SendSingleItem(taskProgress =>
									{
#if DEBUG
										if (rand.Next(0, 2) == 1)
										{
											taskProgress = new SendMailTaskProgress(taskProgress.To, "");
										}
#endif
										
										Progress(taskProgress, mailData);
									},
									mailData,
									parsedTemplate,
									compiledAddressExpression,
									compiledSubjectExpression,
									compiledNameExpression,
									compiledFromAddressExpression,
									compiledFromNameExpression,
									threadState).ConfigureAwait(true).GetAwaiter().GetResult();
							}

							if (MailDistributor.ParallelSupport == ParallelSupport.MultiInstance)
							{
								threadState = MailDistributor.EndSendMail(threadState).ConfigureAwait(true).GetAwaiter().GetResult();
							}
						}
						catch (Exception e)
						{
							
						}
					}

					var thread = new Thread(Start);
					thread.IsBackground = true;
					thread.Name = "MailComposerConsumer_" + i;
					thread.Priority = ThreadPriority.Normal;
					thread.TrySetApartmentState(ApartmentState.MTA);
					threads[i] = thread;
				}

				foreach (var thread in threads)
				{
					thread.Start();
				}

				var readAheadCount = ParallelReadAheadCount;
				await foreach (var item in mailDatas.WithCancellation(stopRequestedToken))
				{
					consumerItems.Add(item, stopRequestedToken);
					while (consumerItems.Count >= readAheadCount)
					{
						await Task.Delay(250, stopRequestedToken);
					}
				}

				consumerItems.CompleteAdding();
				try
				{
					foreach (var thread in threads)
					{
						thread.Join();
					}
				}
				catch (Exception e)
				{
					
				}

				if (MailDistributor.ParallelSupport == ParallelSupport.Full)
				{
					globalState = await MailDistributor.EndSendMail(globalState);
				}
			}
			else
			{
				var beginSend = await MailDistributor.BeginSendMail();
				await foreach (var mailData in mailDatas.WithCancellation(stopRequestedToken))
				{
					buffered = 1;
					if (stopRequestedToken.IsCancellationRequested)
					{
						break;
					}

#if DEBUG
					await Task.Delay(rand.Next(150, 200), stopRequestedToken);
#endif
					await SendSingleItem(taskProgress =>
						{
#if DEBUG
							if (rand.Next(0, 2) == 1)
							{
								taskProgress = new SendMailTaskProgress(taskProgress.To, "");
							}
#endif
							Progress(taskProgress, mailData);
						},
						mailData,
						parsedTemplate,
						compiledAddressExpression,
						compiledSubjectExpression,
						compiledNameExpression,
						compiledFromAddressExpression,
						compiledFromNameExpression,
						beginSend);
				}
				buffered = 0;

				await MailDistributor.EndSendMail(beginSend);
			}

			var resultData = new ComposeMailResult();
			resultData.SendSuccessfully = sendData;
			resultData.SendFailed = sendFailed.ToDictionary(e => e.Key, e => e.Value);

			progress.Report(new SendMailProgress("", sendData, maxSendData, true, sendSuccess, sendFailed.Count, buffered));
			return resultData;
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
			DistributorData distributorData = null;
			try
			{
				distributorData = await Compose(mailData, parsedTemplate, compiledAddressExpression,
					compiledSubjectExpression,
					compiledNameExpression,
					compiledFromAddressExpression,
					compiledFromNameExpression);
				var sendMailResult = await MailDistributor.SendMail(distributorData, state);
				if (!sendMailResult.Success)
				{
					progress(new SendMailTaskProgress(distributorData.MailInfo.ToAddress, sendMailResult.ErrorText));
				}
				else
				{
					progress(new SendMailTaskProgress(distributorData.MailInfo.ToAddress));
				}
			}
			catch (Exception e)
			{
				progress(new SendMailTaskProgress(distributorData?.MailInfo.ToAddress, e.Message));
			}
		}
	}

	public class ComposeMailResult
	{
		public int SendSuccessfully { get; set; }
		public IDictionary<MailData, SendMailStatus> SendFailed { get; set; }
	}
}