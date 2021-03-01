using System.Threading.Tasks;
using Morestachio.MailProcessor.Framework.Import;

namespace Morestachio.MailProcessor.Framework.Sender
{
	public interface IMailDistributor
	{
		string Id { get; }

		Task<string> BeginSendMail();
		Task<SendMailResult> SendMail(DistributorData data);
		Task<string> EndSendMail();
	}

	public class SendMailResult
	{
		public bool Success { get; set; }
		public string Error { get; set; }
	}
}