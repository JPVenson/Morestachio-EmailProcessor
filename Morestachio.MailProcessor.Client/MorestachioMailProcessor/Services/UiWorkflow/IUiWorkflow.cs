using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Morestachio.MailProcessor.Client.ViewModels;

namespace Morestachio.MailProcessor.Client.Services.UiWorkflow
{
	public interface IUiWorkflow : INotifyPropertyChanged
	{
		IList<IWizardStepBaseViewModel> Steps { get; }
		IWizardStepBaseViewModel CurrentStep { get; }
		ICommand NextPageCommand { get; }
		ICommand PreviousPageCommand { get; }
		void InitSteps();
	}
}