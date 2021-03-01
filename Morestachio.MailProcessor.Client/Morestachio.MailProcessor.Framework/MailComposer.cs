using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Framework.Sender;

namespace Morestachio.MailProcessor.Framework
{
	public class MailComposer
	{
		public bool SendInParallel { get; set; }

		public IMailDataStrategy MailDataStrategy { get; set; }
		public IMailDistributor MailDistributor { get; set; }

		public string Template { get; set; }

		public IMorestachioExpression AddressExpression { get; set; }
		public IMorestachioExpression NameExpression { get; set; }
		public IMorestachioExpression SubjectExpression { get; set; }

		public struct SendMailProgress
		{
			public SendMailProgress(string to, int progress, int maxProgress)
			{
				To = to;
				Progress = progress;
				MaxProgress = maxProgress;
			}

			public int Progress { get; private set; }
			public int MaxProgress { get; private set; }
			public string To { get; private set; }
		}


		private async Task<DistributorData> Compose(
			MailData mailData,
			MorestachioDocumentInfo parsedTemplate,
			CompiledExpression compiledAddressExpression,
			CompiledExpression compiledSubjectExpression,
			CompiledExpression compiledNameExpression)
		{
			var context = new ContextObject(parsedTemplate.ParserOptions, ".", null, mailData.Data);

			return new DistributorData()
			{
				Address = (await compiledAddressExpression(context, new ScopeData())).Value.ToString(),
				To = (await compiledNameExpression(context, new ScopeData())).Value.ToString(),
				Subject = (await compiledSubjectExpression(context, new ScopeData())).Value.ToString(),
				Content = (await parsedTemplate.CreateAsync(mailData.Data)).Stream
			};
		}

		public async Task<ComposeMailResult> ComposeAndSend(IProgress<SendMailProgress> progress)
		{
			var compiledAddressExpression = AddressExpression.Compile();
			var compiledNameExpression = NameExpression.Compile();
			var compiledSubjectExpression = SubjectExpression.Compile();

			var parsedTemplate = await Parser.ParseWithOptionsAsync(new ParserOptions(Template));
			var sendSuccess = 0;
			var sendFailed = new ConcurrentDictionary<MailData, SendMailResult>();
			var mailDatas = await MailDataStrategy.GetMails(null, null);
			if (SendInParallel)
			{
				var sendMails = 0;

				Parallel.ForEach(mailDatas, async (mailData, state) =>
				{
					var distributorData = await Compose(mailData, parsedTemplate, compiledAddressExpression, compiledSubjectExpression, compiledNameExpression);
					var sendMailResult = await MailDistributor.SendMail(distributorData);
					if (sendMailResult.Success)
					{
						Interlocked.Add(ref sendSuccess, 1);
					}
					else
					{
						sendFailed.TryAdd(mailData, sendMailResult);
					}
					progress.Report(new SendMailProgress(distributorData.Address, Interlocked.Add(ref sendMails, 1), mailDatas.Count));
				});
			}
			else
			{
				var sendMails = 0;
				foreach (var mailData in mailDatas)
				{
					var distributorData = await Compose(mailData, parsedTemplate, compiledAddressExpression, compiledSubjectExpression, compiledNameExpression);
					var sendMailResult = await MailDistributor.SendMail(distributorData);
					sendMails++;
					progress.Report(new SendMailProgress(distributorData.Address, sendMails, mailDatas.Count));
					if (sendMailResult.Success)
					{
						Interlocked.Add(ref sendSuccess, 1);
					}
					else
					{
						sendFailed.TryAdd(mailData, sendMailResult);
					}
				}
			}

			var resultData = new ComposeMailResult();
			resultData.SendSuccessfully = sendSuccess;
			resultData.SendFailed = sendFailed.ToDictionary(e => e.Key, e => e.Value);
			return resultData;
		}
	}

	public class ComposeMailResult
	{
		public int SendSuccessfully { get; set; }
		public IDictionary<MailData, SendMailResult> SendFailed { get; set; }
	}
}
