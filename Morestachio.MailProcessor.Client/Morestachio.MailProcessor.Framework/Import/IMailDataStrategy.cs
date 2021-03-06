using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Morestachio.MailProcessor.Framework.Import
{
	public interface IMailDataStrategy : IAsyncDisposable
	{
		string Id { get; } 
		
		Task<IAsyncEnumerable<MailData>> GetMails();
		Task<MailData> GetPreviewData();
		Task<int> Count();
	}
}