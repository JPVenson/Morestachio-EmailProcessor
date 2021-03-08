using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
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

		public ParallelSupport ParallelSupport { get; } = ParallelSupport.MultiInstance;

		private class State : SendMailStatus
		{
			public SmtpClient SmtpClient { get; set; }
		}
		
		public async Task<IMailDistributorState> BeginSendMail()
		{
			var state = new State()
			{
				SmtpClient = new SmtpClient()
			};
			try
			{
				await state.SmtpClient.ConnectAsync(Host, HostPort);
				await state.SmtpClient.AuthenticateAsync(AuthName, AuthPassword);
			}
			catch (Exception e)
			{
				if (state.SmtpClient.IsConnected)
				{
					await state.SmtpClient.DisconnectAsync(true);
					state.SmtpClient.Dispose();
				}

				return new SendMailStatus()
				{
					Success = false,
					ErrorText = e.Message
				};
			}

			return state;
		}

		public async Task<IMailDistributorState> SendMail(DistributorData data, IMailDistributorState state)
		{
			try
			{
				var mailMessage = new MimeMessage();
			
				mailMessage.To.Add(new MailboxAddress(data.MailInfo.ToName, data.MailInfo.ToAddress));
				mailMessage.From.Add(new MailboxAddress(data.MailInfo.FromName, data.MailInfo.FromAddress));
				mailMessage.Subject = data.MailInfo.Subject;
				var mailMessageBody = new TextPart(TextFormat.Html);
				mailMessageBody.Content = new MimeContent(data.DocumentResult.Stream);
				mailMessage.Body = mailMessageBody;
				try
				{
					await (state as State).SmtpClient.SendAsync(mailMessage);
				}
				catch (Exception e)
				{
					return new SendMailStatus()
					{
						Success = false,
						ErrorText = e.Message
					};
				}
				return SendMailStatus.Ok();
			}
			finally
			{
				await data.DocumentResult.Stream.DisposeAsync();
			}
		}

		public async Task<IMailDistributorState> EndSendMail(IMailDistributorState state)
		{
			await (state as State).SmtpClient.DisconnectAsync(true);
			(state as State).SmtpClient.Dispose();
			return SendMailStatus.Ok();
		}
	}
}
