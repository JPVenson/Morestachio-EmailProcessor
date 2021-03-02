using ControlzEx.Theming;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;

namespace Morestachio.MailProcessor.Ui.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			UiWorkflow = new DefaultGenericImportMailWorkflowViewModel();
			UiWorkflow.InitSteps();
			SwitchDarkModeCommand = new DelegateCommand(SwitchDarkModeExecute, CanSwitchDarkModeExecute);
			IoC.RegisterInstance<IUiWorkflow>(UiWorkflow);
			ThemeManager.Current.ChangeTheme(App.Current, "Dark.Blue");
			EnableDarkMode = true;
		}

		private IUiWorkflow _uiWorkflow;
		private bool _enableDarkMode;

		public bool EnableDarkMode
		{
			get { return _enableDarkMode; }
			set { SetProperty(ref _enableDarkMode, value); }
		}

		public IUiWorkflow UiWorkflow
		{
			get { return _uiWorkflow; }
			set
			{
				SendPropertyChanging(() => UiWorkflow);
				_uiWorkflow = value;
				SendPropertyChanged(() => UiWorkflow);
			}
		}

		public DelegateCommand SwitchDarkModeCommand { get; private set; }

		private void SwitchDarkModeExecute(object sender)
		{
			if (EnableDarkMode)
			{
				ThemeManager.Current.ChangeTheme(App.Current, "Dark.Blue");
			}
			else
			{
				ThemeManager.Current.ChangeTheme(App.Current, "Light.Blue");
			}
		}

		private bool CanSwitchDarkModeExecute(object sender)
		{
			return true;
		}
	}
}
