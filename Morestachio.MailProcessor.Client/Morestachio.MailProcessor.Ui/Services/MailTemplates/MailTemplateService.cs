using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Morestachio.MailProcessor.Ui.Services.DataImport;

namespace Morestachio.MailProcessor.Ui.Services.MailTemplates
{
	public class MailTemplateService : IRequireInit
	{
		public MailTemplateService()
		{
			OnlineSources = new List<MailTemplate>()
			{
				new MailTemplate("leemunroe's responsive-html-email-template",
					@"https://raw.githubusercontent.com/leemunroe/responsive-html-email-template/master/email.html",
					MailTemplateSource.Url,
					false),
				new MailTemplate("leemunroe's responsive-html-email-template Inline",
					@"https://raw.githubusercontent.com/leemunroe/responsive-html-email-template/master/email-inlined.html",
					MailTemplateSource.Url,
					false),
				new MailTemplate("konsav's General template",
					@"https://raw.githubusercontent.com/konsav/email-templates/master/general.html",
					MailTemplateSource.Url,
					false),
				new MailTemplate("konsav's Explorational template",
					@"https://raw.githubusercontent.com/konsav/email-templates/master/explorational.html",
					MailTemplateSource.Url,
					false),
				new MailTemplate("ColorlibHQ's Template Collection",
					@"https://github.com/ColorlibHQ/email-templates",
					MailTemplateSource.Url,
					false),
			};
			LocalSources = new List<MailTemplate>();
			Sources = new CollectionView(LocalSources.Concat(OnlineSources));
		}

		public List<MailTemplate> OnlineSources { get; set; }
		public List<MailTemplate> LocalSources { get; set; }

		public ICollectionView Sources { get; private set; }
		
		public void Init()
		{
			var templateFolder = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
			if (!Directory.Exists(templateFolder))
			{
				return;
			}

			foreach (var enumerateFile in Directory.EnumerateFiles(templateFolder))
			{
				LocalSources.Add(new MailTemplate(Path.GetFileName(enumerateFile), enumerateFile, MailTemplateSource.LocalFile, false));
			}
		}

		public async Task<string> ObtainTemplate(MailTemplate template)
		{
			switch (template.SourceType)
			{
				case MailTemplateSource.Url:
					if (!template.SourceValue.EndsWith(".html"))
					{
						Process.Start(new ProcessStartInfo(template.SourceValue){ UseShellExecute = true });
						return null;
					}

					using (var httpClient = new HttpClient())
					{
						return await (await httpClient.GetAsync(template.SourceValue)).Content.ReadAsStringAsync();
					}
				case MailTemplateSource.LocalFile:
					return await File.ReadAllTextAsync(template.SourceValue);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	public class MailTemplate
	{
		public MailTemplate(string header, string sourceValue, MailTemplateSource sourceType, bool needsLicenseAgreement)
		{
			Header = header;
			SourceValue = sourceValue;
			SourceType = sourceType;
			NeedsLicenseAgreement = needsLicenseAgreement;
		}

		public bool NeedsLicenseAgreement { get; }
		public string Header { get; }
		public string SourceValue { get; }
		public MailTemplateSource SourceType { get; }
	}

	public enum MailTemplateSource
	{
		Url,
		LocalFile
	}
}
