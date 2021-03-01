using System.Collections.Generic;
using System.Threading.Tasks;

namespace Morestachio.MailProcessor.Framework.Import
{
	public interface IMailDataStrategy
	{
		string Id { get; } 
		
		Task<ICollection<MailData>> GetMails(int? skip, int? take);
	}
}