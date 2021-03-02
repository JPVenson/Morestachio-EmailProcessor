using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Extensions;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public interface IMailDataStrategyMetaViewModel : IWizardStepBaseViewModel
	{
		UiLocalizableString Name { get; set; }
		string Id { get; set; }
		IMailDataStrategy Create();
	}

	public abstract class MailDataStrategyMetaViewModel<Errors> : WizardStepBaseViewModel<Errors>, IMailDataStrategyMetaViewModel where Errors : IErrorCollectionBase, new()
	{
		public MailDataStrategyMetaViewModel(string id)
		{
			Id = id;
			GroupKey = "ImportData";
		}

		public UiLocalizableString Name { get; set; }
		public string Id { get; set; }

		public abstract IMailDataStrategy Create();

		public override void OnAdded(DefaultGenericImportStepConfigurator configurator)
		{
			base.OnAdded(configurator);
		}

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}
	}
}