using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.UiWorkflow
{
	public interface IUiWorkflow : INotifyPropertyChanged
	{
		IList<IWizardStepBaseViewModel> Steps { get; }
		IWizardStepBaseViewModel CurrentStep { get; }
		ICommand NextPageCommand { get; }
		ICommand PreviousPageCommand { get; }
		ICommand SaveCurrentStateCommand { get; }
		void InitSteps();
		bool HelpRequested { get; set; }
	}
}