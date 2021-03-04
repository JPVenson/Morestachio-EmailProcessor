using ControlzEx.Theming;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.Services.WebView;

namespace Morestachio.MailProcessor.Ui.ViewModels
{
	public class MainWindowViewModel : AsyncViewModelBase
	{
		public MainWindowViewModel()
		{
			UiWorkflow = new DefaultWorkflow();
			UiWorkflow.InitSteps();
			SwitchDarkModeCommand = new DelegateCommand(SwitchDarkModeExecute, CanSwitchDarkModeExecute);
			InstallWebEnvCommand = new DelegateCommand(InstallWebEnvExecute, CanInstallWebEnvExecute);

			IoC.RegisterInstance<IUiWorkflow>(UiWorkflow);
			ThemeManager.Current.ChangeTheme(App.Current, "Dark.Blue");
			EnableDarkMode = true;
			WebViewService = IoC.Resolve<WebViewService>();
		}

		private IUiWorkflow _uiWorkflow;
		private bool _enableDarkMode;
		public WebViewService WebViewService { get; set; }

		public DelegateCommand InstallWebEnvCommand { get; private set; }

		private void InstallWebEnvExecute(object sender)
		{
			SimpleWorkAsync(async () =>
			{
				await WebViewService.Install();
			});
		}

		private bool CanInstallWebEnvExecute(object sender)
		{
			return WebViewService != null && WebViewService.IsSupported && !WebViewService.IsInstalled;
		}

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
