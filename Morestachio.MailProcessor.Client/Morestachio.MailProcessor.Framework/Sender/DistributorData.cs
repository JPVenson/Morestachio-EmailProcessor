using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Morestachio.MailProcessor.Framework.Sender
{
	public class DistributorData
	{
		public string To { get; set; }
		public string Address { get; set; }

		public Stream Content { get; set; }
		public string Subject { get; set; }
	}
}
