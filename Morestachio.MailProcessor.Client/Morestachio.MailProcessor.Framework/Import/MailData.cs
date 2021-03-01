using System.Collections.Generic;

namespace Morestachio.MailProcessor.Framework.Import
{
	public class MailData
	{
		public MailData()
		{
			Data = new Dictionary<string, object>();
		}
		
		public IDictionary<string, object> Data { get; }
	}
}