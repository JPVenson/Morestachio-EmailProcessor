using System.Collections.Generic;
using MorestachioMailProcessor.Services.TextService;

namespace MorestachioMailProcessor.ViewModels
{
	public interface IUiLocalizableString
	{
		ICollection<FormattableArgument> Arguments { get; set; }
		string Key { get; set; }

		object Resolve(ITextService textService);
	}
}