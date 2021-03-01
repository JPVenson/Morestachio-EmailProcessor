using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Morestachio.MailProcessor.Client.Services.TextService
{
	public interface IAsyncTranslationProvider : ITranslationProvider
	{
		Task<Dictionary<string, object>> ProvideTranslationsAsync(CultureInfo culture);
	}
}