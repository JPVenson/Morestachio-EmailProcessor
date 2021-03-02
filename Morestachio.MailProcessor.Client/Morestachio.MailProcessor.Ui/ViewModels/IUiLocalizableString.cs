using System.Collections.Generic;
using Morestachio.MailProcessor.Ui.Services.TextService;

namespace Morestachio.MailProcessor.Ui.ViewModels
{
	public interface IUiLocalizableString
	{
		ICollection<FormattableArgument> Arguments { get; set; }
		string Key { get; set; }

		object Resolve(ITextService textService);
	}
}