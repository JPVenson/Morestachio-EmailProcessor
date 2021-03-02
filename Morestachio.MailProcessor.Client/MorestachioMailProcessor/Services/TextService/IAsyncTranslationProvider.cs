using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MorestachioMailProcessor.Services.TextService
{
	public interface IAsyncTranslationProvider : ITranslationProvider
	{
		Task<Dictionary<string, object>> ProvideTranslationsAsync(CultureInfo culture);
	}
}