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
			var mailMessage = new MimeMessage();
			
			mailMessage.To.Add(new MailboxAddress(data.To, data.ToAddress));
			mailMessage.From.Add(new MailboxAddress(data.From, data.FromAddress));
			mailMessage.Subject = data.Subject;
			mailMessage.Body = new TextPart(TextFormat.Html)
			{
				Text = data.Content.Stringify(true, Encoding.UTF8),
			};
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

		public async Task<IMailDistributorState> EndSendMail(IMailDistributorState state)
		{
			await (state as State).SmtpClient.DisconnectAsync(true);
			(state as State).SmtpClient.Dispose();
			return SendMailStatus.Ok();
		}
	}
}
