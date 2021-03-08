using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Morestachio.MailProcessor.Framework.Import;

namespace Morestachio.MailProcessor.Framework.Sender
{
	public class DistributorData
	{
		public DistributorData(MailDataInfo mailInfo, MorestachioDocumentResult documentResult)
		{
			MailInfo = mailInfo;
			DocumentResult = documentResult;
		}

		public MailDataInfo MailInfo { get; }
		public MorestachioDocumentResult DocumentResult { get; }
	}
}
