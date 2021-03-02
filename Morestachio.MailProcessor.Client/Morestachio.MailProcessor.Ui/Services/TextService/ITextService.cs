using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.TextService
{
	public interface ITextService
	{
		IDictionary<string, CultureInfo> SupportedCultures { get; }
		ICollection<ITranslationProvider> TranslationResources { get; }
		object Compile(string key, CultureInfo culture, out bool found);
		object Compile(string key, CultureInfo culture, out bool found, params FormattableArgument[] arguments);
		string RunTransformation(CultureInfo culture, string transformationKey, string text);
		IEnumerable<TextResourceEntity> GetByGroupName(string groupName, string locale);
		Task LoadTexts();
		event EventHandler<string> TranslationLoaded;
	}
}
