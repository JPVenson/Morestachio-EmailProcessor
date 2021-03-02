using System.Collections.Generic;
using System.Globalization;

namespace Morestachio.MailProcessor.Ui.Services.TextService
{
	public interface ITranslationProvider
	{
		Dictionary<string, object> ProvideTranslations(CultureInfo culture);
	}
}