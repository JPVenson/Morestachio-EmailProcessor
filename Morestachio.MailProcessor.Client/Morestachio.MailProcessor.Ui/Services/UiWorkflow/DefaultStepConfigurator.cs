using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.UiWorkflow
{
	public class DefaultStepConfigurator
	{
		public IUiWorkflow Workflow { get; }
		public IWizardStepBaseViewModel Step { get; }

		internal DefaultStepConfigurator(IWizardStepBaseViewModel step)
		{
			Workflow = IoC.Resolve<IUiWorkflow>();
			Step = step;
			PersistantSettingsService = IoC.Resolve<PersistantSettingsService>();
		}

		public PersistantSettingsService PersistantSettingsService { get; set; }

		public void AddNextToMe(IWizardStepBaseViewModel step)
		{
			var index = Workflow.Steps.IndexOf(Step);

			if (index == Workflow.Steps.Count)
			{
				Workflow.Steps.Add(step);
			}
			else
			{
				if (Workflow.Steps.ElementAt(index + 1).GetType() == step.GetType())
				{
					return;
				}

				Workflow.Steps.Insert(index + 1, step);
			}

			if (PersistantSettingsService.LoadedSettings != null)
			{
				step.ReadSettings(PersistantSettingsService.LoadedSettings.Values);
				if (step is AsyncErrorProviderBase errorProvider)
				{
					errorProvider.ForceRefreshAsync();
				}
			}

			var defaultGenericImportStepConfigurator = new DefaultStepConfigurator(step);
			step.OnAdded(defaultGenericImportStepConfigurator);
		}

		//public void AddPrecededByMe(IWizardStepBaseViewModel step)
		//{
		//	var index = Workflow.Steps.IndexOf(Step);
		//	if (index == 0)
		//	{
		//		Workflow.Steps.Insert(0, step);
		//	}
		//	else
		//	{
		//		Workflow.Steps.Insert(index - 1, step);
		//	}

		//	var defaultGenericImportStepConfigurator = new DefaultGenericImportStepConfigurator(Workflow, step);
		//	step.OnAdded(defaultGenericImportStepConfigurator);
		//}
	}
}
