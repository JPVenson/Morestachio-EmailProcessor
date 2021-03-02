using System.Threading.Tasks;
using Morestachio.MailProcessor.Framework.Import;

namespace Morestachio.MailProcessor.Framework.Sender
{
	public interface IMailDistributor
	{
		string Id { get; }
		ParallelSupport ParallelSupport { get; }

		Task<IMailDistributorState> BeginSendMail();
		Task<IMailDistributorState> SendMail(DistributorData data, IMailDistributorState state);
		Task<IMailDistributorState> EndSendMail(IMailDistributorState state);


	}

	public enum ParallelSupport
	{
		None,
		MultiInstance,
		Full
	}

	public interface IMailDistributorState
	{
		bool Success { get; }
		string ErrorText { get; }
	}

	public class SendMailStatus : IMailDistributorState
	{
		public bool Success { get; set; }
		public string ErrorText { get; set; }

		private static readonly IMailDistributorState _ok = new SendMailStatus() { Success = true };
		public static IMailDistributorState Ok()
		{
			return _ok;
		}
	}
}