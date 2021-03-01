using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Morestachio.Helper;

namespace Morestachio.MailProcessor.Framework.Sender.Strategies
{
	public class SmtpMailDistributor : IMailDistributor
	{
		public string Host { get; set; }
		public int HostPort { get; set; } = 587;

		public string AuthName { get; set; }
		public string AuthPassword { get; set; }
		

		public const string IdKey = "SmtpDirect";
		public string Id
		{
			get { return IdKey; }
		}


		private SmtpClient _client;
		public async Task<string> BeginSendMail()
		{
			_client = new SmtpClient();
			try
			{
				await _client.ConnectAsync(Host, HostPort);
				await _client.AuthenticateAsync(AuthName, AuthPassword);
			}
			catch (Exception e)
			{
				return e.Message;
			}

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
			try
			{
				await _client.SendAsync(mailMessage);
			}
			catch (Exception e)
			{
				return new SendMailResult()
				{
					Success = false,
					Error = e.Message
				};
			}
			return new SendMailResult()
			{
				Success = true
			};
		}

		public async Task<string> EndSendMail()
		{
			await _client.DisconnectAsync(true);
			_client?.Dispose();
			return null;
		}
	}
}
