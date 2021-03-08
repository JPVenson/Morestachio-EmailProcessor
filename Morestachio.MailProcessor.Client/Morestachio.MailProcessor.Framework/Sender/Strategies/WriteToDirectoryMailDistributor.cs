using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using Morestachio.Helper;

namespace Morestachio.MailProcessor.Framework.Sender.Strategies
{
	public class WriteToDirectoryMailDistributor : IMailDistributor
	{
		private readonly string _workingDirectory;

		public WriteToDirectoryMailDistributor(string workingDirectory)
		{
			_workingDirectory = workingDirectory;
		}

		public const string IdKey = "WriteToDirectory";
		public string Id
		{
			get { return IdKey; }
		}

		public ParallelSupport ParallelSupport { get; } = ParallelSupport.Full;

		public async Task<IMailDistributorState> BeginSendMail()
		{
			await Task.CompletedTask;
			return SendMailStatus.Ok();
		}

		public async Task<IMailDistributorState> SendMail(DistributorData data, IMailDistributorState state)
		{
			var mailMessage = new MimeMessage();
			
			mailMessage.To.Add(new MailboxAddress(data.MailInfo.ToName, data.MailInfo.ToAddress));
			mailMessage.From.Add(new MailboxAddress(data.MailInfo.FromName, data.MailInfo.FromAddress));
			mailMessage.Subject = data.MailInfo.Subject;
			var mailMessageBody = new TextPart(TextFormat.Html);
			mailMessageBody.Content = new MimeContent(data.DocumentResult.Stream);
			mailMessage.Body = mailMessageBody;

			await using (var targetStream = new FileStream(Path.Combine(_workingDirectory, $"mail-to-{data.MailInfo.ToAddress}.mime"),
				FileMode.Create))
			{
				await mailMessage.WriteToAsync(targetStream);
			}

			return SendMailStatus.Ok();
		}

		public async Task<IMailDistributorState> EndSendMail(IMailDistributorState state)
		{
			await Task.CompletedTask;
			return SendMailStatus.Ok();
		}
	}
}
