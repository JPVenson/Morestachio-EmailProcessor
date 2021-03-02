using System.Collections.Generic;
using System.Globalization;

namespace MorestachioMailProcessor.Services.TextService
{
	public interface ITranslationProvider
	{
		Dictionary<string, object> ProvideTranslations(CultureInfo culture);
	}
}