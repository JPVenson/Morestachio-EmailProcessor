using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Client.Services.DataDistributor.Strategies;
using Morestachio.MailProcessor.Client.Services.DataImport;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Client.ViewModels.Steps;
using Morestachio.MailProcessor.Client.ViewModels.Steps.Import;

namespace Morestachio.MailProcessor.Client.Services.UiWorkflow
{
	public class DefaultGenericImportMailWorkflowViewModel : AsyncViewModelBase, IUiWorkflow
	{
		private IWizardStepBaseViewModel _currentStep;

		public DefaultGenericImportMailWorkflowViewModel()
		{
			Data = new Dictionary<string, object>();
			InitCommands();
		}

		protected void InitCommands()
		{
			NextPageCommand = new DelegateCommand(NextPageExecute, CanNextPageExecute);
			PreviousPageCommand = new DelegateCommand(PreviousPageExecute, CanPreviousPageExecute);
		}
		
		private void PreviousPageExecute(object sender)
		{
			var indexOf = Steps.IndexOf(CurrentStep) - 1;
			if (!CurrentStep.OnGoPrevious(new DefaultGenericImportStepConfigurator(this, CurrentStep)))
			{
				return;
			}
			CurrentStep = Steps.ElementAt(indexOf);
			SimpleWorkAsync(async () =>
			{
				await CurrentStep.OnEntry(Data);
			});
		}

		private bool CanPreviousPageExecute(object sender)
		{
			return IsNotWorking && CurrentStep.CanGoPrevious();
		}

		private void NextPageExecute(object sender)
		{
			if (!CurrentStep.OnGoNext(new DefaultGenericImportStepConfigurator(this, CurrentStep)))
			{
				return;
			}
			CurrentStep = Steps.ElementAt(Steps.IndexOf(CurrentStep) + 1);
			SimpleWorkAsync(async () =>
			{
				await CurrentStep.OnEntry(Data);
			});
		}

		private bool CanNextPageExecute(object sender)
		{
			return IsNotWorking && CurrentStep.CanGoNext();
		}

		public virtual void InitSteps()
		{
			Steps = new ThreadSaveObservableCollection<IWizardStepBaseViewModel>();
			Steps.Add(new WelcomeStepViewModel());
			Steps.Add(new ImportDataStepViewModel());

			Steps.Add(new PrepareMailDataStepViewModel());
			Steps.Add(new TemplateSelectorStepViewModel());
			Steps.Add(new MailDistributorSelectorViewModel());
			Steps.Add(new SummeryStepViewModel());

			Steps.Add(new CloseStepViewModel());

			foreach (var wizardStepBaseViewModel in Steps)
			{
				wizardStepBaseViewModel.OnAdded(new DefaultGenericImportStepConfigurator(this, wizardStepBaseViewModel));
			}


			CurrentStep = Steps.First();
		}

		public IList<IWizardStepBaseViewModel> Steps { get; set; }

		public IWizardStepBaseViewModel CurrentStep
		{
			get { return _currentStep; }
			set
			{
				SetProperty(ref _currentStep, value);
			}
		}		

		public ICommand NextPageCommand { get; set; }
		public ICommand PreviousPageCommand { get; set; }

		public IDictionary<string, object> Data { get; set; }
	}
}
