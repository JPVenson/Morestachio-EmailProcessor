using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies;
using Morestachio.MailProcessor.Ui.Services.DataImport.Strategies;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Steps;

namespace Morestachio.MailProcessor.Ui.Services.UiWorkflow
{
	public class DefaultGenericImportMailWorkflowViewModel : AsyncViewModelBase, IUiWorkflow
	{
		private IWizardStepBaseViewModel _currentStep;

		public DefaultGenericImportMailWorkflowViewModel()
		{
			Data = new Dictionary<string, object>();
			InitCommands();
			App.Current.MainWindow.Closing += MainWindow_Closing;
		}

		private static bool _forceClose = false;
		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (_forceClose || CurrentStep is CloseStepViewModel || CurrentStep is WelcomeStepViewModel)
			{
				return;
			}

			var textService = IoC.Resolve<ITextService>();

			DialogCoordinator.Instance.ShowMessageAsync(this,
					textService.Compile("Application.Navigation.Close.Title", CultureInfo.CurrentUICulture, out _).ToString(),
					textService.Compile("Application.Navigation.Close.Message", CultureInfo.CurrentUICulture, out _).ToString(),
				MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
				{

				})
				.ContinueWith(e =>
				{
					if (e.Result == MessageDialogResult.Affirmative)
					{
						_forceClose = true;
						ViewModelAction(() =>
						{
							App.Current.MainWindow.Close();
						});
					}
				});
			e.Cancel = true;
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

			TransitionType = TransitionType.Left;
			CurrentStep = Steps.ElementAt(indexOf);
			SimpleWorkAsync(async () =>
			{
				await CurrentStep.OnEntry(Data, new DefaultGenericImportStepConfigurator(this, CurrentStep));
			});
		}

		private bool CanPreviousPageExecute(object sender)
		{
			return IsNotWorking && !HelpRequested && CurrentStep.CanGoPrevious();
		}

		private void NextPageExecute(object sender)
		{
			if (!CurrentStep.OnGoNext(new DefaultGenericImportStepConfigurator(this, CurrentStep)))
			{
				return;
			}
			TransitionType = TransitionType.Right;
			CurrentStep = Steps.ElementAt(Steps.IndexOf(CurrentStep) + 1);
			SimpleWorkAsync(async () =>
			{
				await CurrentStep.OnEntry(Data, new DefaultGenericImportStepConfigurator(this, CurrentStep));
			});
		}

		private bool CanNextPageExecute(object sender)
		{
			return IsNotWorking && !HelpRequested && CurrentStep.CanGoNext();
		}

		public virtual void InitSteps()
		{
			Steps = new ThreadSaveObservableCollection<IWizardStepBaseViewModel>();
			Steps.Add(new WelcomeStepViewModel());
			Steps.Add(new ImportDataSelectorStepViewModel());

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

		private TransitionType _transitionType;

		public TransitionType TransitionType
		{
			get { return _transitionType; }
			set { SetProperty(ref _transitionType, value); }
		}

		public IWizardStepBaseViewModel CurrentStep
		{
			get { return _currentStep; }
			set { SetProperty(ref _currentStep, value); }
		}		

		public ICommand NextPageCommand { get; set; }
		public ICommand PreviousPageCommand { get; set; }

		public IDictionary<string, object> Data { get; set; }
		private bool _helpRequested;

		public bool HelpRequested
		{
			get { return _helpRequested; }
			set { SetProperty(ref _helpRequested, value); }
		}
	}
}
