using System.Collections.Generic;
using Morestachio.MailProcessor.Client.Services.TextService;

namespace Morestachio.MailProcessor.Client.ViewModels
{
	public interface IUiLocalizableString
	{
		ICollection<FormattableArgument> Arguments { get; set; }
		string Key { get; set; }

		object Resolve(ITextService textService);
	}
}