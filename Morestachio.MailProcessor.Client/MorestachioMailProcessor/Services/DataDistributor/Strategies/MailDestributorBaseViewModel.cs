using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Extensions;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework.Sender;

namespace MorestachioMailProcessor.Services.DataDistributor.Strategies
{
	public interface IMailDistributorBaseViewModel : IWizardStepBaseViewModel
	{
		UiLocalizableString Name { get; }
		string IdKey { get; set; }
		IMailDistributor Create();
	}

	public abstract class MailDistributorBaseViewModel<TErrors> : WizardStepBaseViewModel<TErrors>, IMailDistributorBaseViewModel where TErrors : IErrorCollectionBase, new()
	{
		protected MailDistributorBaseViewModel()
		{
			GroupKey = "Distributors";
		}

		public UiLocalizableString Name { get; set; }
		public string IdKey { get; set; }
		public abstract IMailDistributor Create();

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == "Distributors");
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}
	}
}
