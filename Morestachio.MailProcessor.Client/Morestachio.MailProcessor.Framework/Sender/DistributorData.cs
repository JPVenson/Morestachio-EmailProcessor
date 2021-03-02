using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Morestachio.MailProcessor.Framework.Sender
{
	public class DistributorData
	{
		public string To { get; set; }
		public string ToAddress { get; set; }

		public string From { get; set; }
		public string FromAddress { get; set; }

		public Stream Content { get; set; }
		public string Subject { get; set; }
	}
}
