using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public interface IMailDataStrategyMetaViewModel : IWizardStepBaseViewModel
	{
		UiLocalizableString Name { get; set; }
		string Id { get; set; }
		IMailDataStrategy Create();
	}
}