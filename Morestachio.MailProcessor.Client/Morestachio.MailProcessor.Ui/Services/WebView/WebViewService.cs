using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Morestachio.MailProcessor.Ui.Services.DataImport;

namespace Morestachio.MailProcessor.Ui.Services.WebView
{
	public class WebViewService : IRequireInit
	{
		public bool IsSupported { get; set; }
		public bool IsInstalled { get; set; }
		public CoreWebView2Environment CoreWebView2Environment { get; set; }

		public void Init()
		{
			IsInstalled = CoreWebView2Environment.GetAvailableBrowserVersionString() != null;
			IsSupported = true;
			if (IsInstalled)
			{
				Install().GetAwaiter().GetResult();
			}
		}

		public async Task Install()
		{
			CoreWebView2Environment = await CoreWebView2Environment.CreateAsync();
		}
	}
}
