using System.Collections.Generic;

namespace Morestachio.MailProcessor.Framework.Import
{
	public class MailData
	{
		public MailData() : this(new Dictionary<string, object>())
		{
			
		}

		public MailData(IDictionary<string, object> data)
		{
			Data = data;
			MailInfo = new MailDataInfo();
		}

		public IDictionary<string, object> Data { get; }
		
		public MailDataInfo MailInfo { get; }
	}

	public class MailDataInfo
	{
		public string ToAddress { get; set; }
		public string ToName { get; set; }
		public string Subject { get; set; }
		public string FromAddress { get; set; }
		public string FromName { get; set; }
	}
}