using Morestachio.MailProcessor.Framework.Sender;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies
{
	public interface IMailDistributorBaseViewModel : IWizardStepBaseViewModel
	{
		UiLocalizableString Name { get; }
		string IdKey { get; set; }
		IMailDistributor Create();
	}
}