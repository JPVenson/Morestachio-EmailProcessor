using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Client.ViewModels.Steps;

namespace Morestachio.MailProcessor.Client.Services.UiWorkflow
{
	public class DefaultGenericImportStepConfigurator
	{
		public IUiWorkflow Workflow { get; }
		public IWizardStepBaseViewModel Step { get; }

		internal DefaultGenericImportStepConfigurator(IUiWorkflow workflow, IWizardStepBaseViewModel step)
		{
			Workflow = workflow;
			Step = step;
		}

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

			var defaultGenericImportStepConfigurator = new DefaultGenericImportStepConfigurator(Workflow, step);
			step.OnAdded(defaultGenericImportStepConfigurator);
		}

		public void AddPrecededByMe(IWizardStepBaseViewModel step)
		{
			var index = Workflow.Steps.IndexOf(Step);
			if (index == 0)
			{
				Workflow.Steps.Insert(0, step);
			}
			else
			{
				Workflow.Steps.Insert(index - 1, step);
			}

			var defaultGenericImportStepConfigurator = new DefaultGenericImportStepConfigurator(Workflow, step);
			step.OnAdded(defaultGenericImportStepConfigurator);
		}
	}
}
