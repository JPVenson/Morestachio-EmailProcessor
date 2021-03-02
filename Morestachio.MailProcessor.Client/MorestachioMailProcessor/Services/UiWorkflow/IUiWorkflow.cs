using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Morestachio.MailProcessor.Client.ViewModels;

namespace MorestachioMailProcessor.Services.UiWorkflow
{
	public interface IUiWorkflow : INotifyPropertyChanged
	{
		IList<IWizardStepBaseViewModel> Steps { get; }
		IWizardStepBaseViewModel CurrentStep { get; }
		ICommand NextPageCommand { get; }
		ICommand PreviousPageCommand { get; }
		void InitSteps();
		bool HelpRequested { get; set; }
	}
}