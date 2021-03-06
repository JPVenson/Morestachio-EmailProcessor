using System;
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
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;
using Morestachio.MailProcessor.Ui.ViewModels.Steps;

namespace Morestachio.MailProcessor.Ui.Services.UiWorkflow
{
	public class DefaultWorkflow : AsyncViewModelBase, IUiWorkflow
	{
		private IWizardStepBaseViewModel _currentStep;

		public DefaultWorkflow()
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
			SaveCurrentStateCommand = new DelegateCommand(SaveCurrentStateExecute, CanSaveCurrentStateExecute);
		}

		public ICommand SaveCurrentStateCommand { get; private set; }

		private void SaveCurrentStateExecute(object sender)
		{
			SimpleWorkAsync(async () =>
			{
				var textService = IoC.Resolve<ITextService>();
				var persistantSettingsService = IoC.Resolve<PersistantSettingsService>();
				var settingsToSave = persistantSettingsService.LoadedSettings;
				if (persistantSettingsService.LoadedSettings != null)
				{
					if ((await DialogCoordinator.Instance.ShowMessageAsync(this,
						textService.Compile("Application.Storage.OverwriteExisting.Title",
							CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("Application.Storage.OverwriteExisting.Message",
							CultureInfo.CurrentUICulture, out _,
							new FormattableArgument(persistantSettingsService.LoadedSettings.Name, false)).ToString(),
						MessageDialogStyle.AffirmativeAndNegative
					)) == MessageDialogResult.Negative)
					{
						settingsToSave = new SettingsEntry();
					}
				}
				else
				{
					settingsToSave = new SettingsEntry();	
				}

				if (settingsToSave.Name == null)
				{
					var name = (await DialogCoordinator.Instance.ShowInputAsync(this,
						textService.Compile("Application.Storage.Filename.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("Application.Storage.Filename.Message", CultureInfo.CurrentUICulture, out _).ToString()));
					if (string.IsNullOrWhiteSpace(name))
					{
						return;
					}

					settingsToSave.Name = name;
				}

				foreach (var wizardStepBaseViewModel in Steps)
				{
					var saveSetting = await wizardStepBaseViewModel.SaveSetting();
					foreach (var setting in saveSetting)
					{
						settingsToSave.Values[setting.Key] = setting.Value;
					}
				}

				persistantSettingsService.SaveSetting(settingsToSave);

			});


		}

		private bool CanSaveCurrentStateExecute(object sender)
		{
			return IsNotWorking;
		}
		
		private void PreviousPageExecute(object sender)
		{
			var indexOf = Steps.IndexOf(CurrentStep) - 1;
			if (!CurrentStep.OnGoPrevious(new DefaultStepConfigurator(CurrentStep)))
			{
				return;
			}

			TransitionType = TransitionType.Left;
			SimpleWorkAsync(async () =>
			{
				var previousStep = Steps.ElementAt(indexOf); 
				await previousStep.OnEntry(Data, new DefaultStepConfigurator(previousStep));
				CurrentStep = previousStep;
			});
		}

		private bool CanPreviousPageExecute(object sender)
		{
			return IsNotWorking && !HelpRequested && CurrentStep.CanGoPrevious();
		}

		private async void NextPageExecute(object sender)
		{
			if ((await CurrentStep.OnGoNext(new DefaultStepConfigurator(CurrentStep))) == false)
			{
				return;
			}
			TransitionType = TransitionType.Right;
#pragma warning disable 4014
			SimpleWorkAsync(async () =>
#pragma warning restore 4014
			{
				var nextStep = Steps.ElementAt(Steps.IndexOf(CurrentStep) + 1);
				await nextStep.OnEntry(Data, new DefaultStepConfigurator(nextStep));
				CurrentStep = nextStep;
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
				wizardStepBaseViewModel.OnAdded(new DefaultStepConfigurator(wizardStepBaseViewModel));
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
