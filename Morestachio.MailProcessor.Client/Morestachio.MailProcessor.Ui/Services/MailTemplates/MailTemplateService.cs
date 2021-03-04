using System;
using System.Collections.Generic;
using System.ComponentModel;
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
				new MailTemplate("leemunroe's responsive-html-email-template", @"https://raw.githubusercontent.com/leemunroe/responsive-html-email-template/master/email.html", MailTemplateSource.Url),
				new MailTemplate("leemunroe's responsive-html-email-template Inline", @"https://raw.githubusercontent.com/leemunroe/responsive-html-email-template/master/email-inlined.html", MailTemplateSource.Url),
			};
			LocalSources = new List<MailTemplate>();
			Sources = new CollectionView(OnlineSources.Concat(LocalSources));
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
				LocalSources.Add(new MailTemplate(Path.GetFileName(enumerateFile), enumerateFile, MailTemplateSource.LocalFile));
			}
		}

		public async Task<string> ObtainTemplate(MailTemplate template)
		{
			switch (template.SourceType)
			{
				case MailTemplateSource.Url:
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
		public MailTemplate(string header, string sourceValue, MailTemplateSource sourceType)
		{
			Header = header;
			SourceValue = sourceValue;
			SourceType = sourceType;
		}

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
