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

		public async Task<string> BeginSendMail()
		{
			await Task.CompletedTask;
			return null;
		}

		public async Task<SendMailResult> SendMail(DistributorData data)
		{
			var mailMessage = new MimeMessage();
			mailMessage.To.Add(new MailboxAddress(data.To, data.Address));
			mailMessage.Subject = data.Subject;
			mailMessage.Body = new TextPart(TextFormat.Html)
			{
				Text = data.Content.Stringify(true, Encoding.UTF8),
			};
			using (var targetStream = new FileStream(Path.Combine(_workingDirectory, $"mail-to-{data.Address}.mime"),
				FileMode.Create))
			{
				await mailMessage.WriteToAsync(targetStream);
			}
			return new SendMailResult()
			{
				Success = true
			};
		}

		public async Task<string> EndSendMail()
		{
			await Task.CompletedTask;
			return null;
		}
	}
}
