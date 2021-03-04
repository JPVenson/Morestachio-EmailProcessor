using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.Services.WebView;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class WelcomeStepViewModel : WizardStepBaseViewModel
	{
		public WelcomeStepViewModel()
		{
			Title = new UiLocalizableString("WelcomeStep.Title");
			Description = new UiLocalizableString("WelcomeStep.Description");
			GroupKey = "MainGroup";
			WebViewService = IoC.Resolve<WebViewService>();
			PersistantSettingsService = IoC.Resolve<PersistantSettingsService>();

			LoadSettingCommand = new DelegateCommand<SettingsMetaEntry>(LoadSettingExecute, CanLoadSettingExecute);
			InstallWebRuntimeCommand = new DelegateCommand(InstallWebRuntimeExecute, CanInstallWebRuntimeExecute);
		}

		public DelegateCommand InstallWebRuntimeCommand { get; private set; }
		public DelegateCommand LoadSettingCommand { get; private set; }

		private async void LoadSettingExecute(SettingsMetaEntry sender)
		{
			var uiWorkflow = IoC.Resolve<IUiWorkflow>();
			var textService = IoC.Resolve<ITextService>();
			
			if (!PersistantSettingsService.LoadSettings(sender))
			{
				await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
					textService.Compile("Application.LoadSettings.Error.Title", CultureInfo.CurrentUICulture, out _)
						.ToString(),
					textService.Compile("Application.LoadSettings.Error.Message", CultureInfo.CurrentUICulture,
						out _).ToString()
				);
				return;
			}

			foreach (var wizardStepBaseViewModel in uiWorkflow.Steps)
			{
				wizardStepBaseViewModel.ReadSettings(PersistantSettingsService.LoadedSettings.Values);
			}

			uiWorkflow.NextPageCommand.Execute(null);

		}

		private bool CanLoadSettingExecute(SettingsMetaEntry sender)
		{
			return sender != null;
		}

		private async void InstallWebRuntimeExecute(object sender)
		{
			var uiWorkflow = IoC.Resolve<IUiWorkflow>();
			var textService = IoC.Resolve<ITextService>();

			await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
				textService.Compile("Application.FeatureUnsupported.Title", CultureInfo.CurrentUICulture, out _).ToString(),
				textService.Compile("Application.FeatureUnsupported.Message", CultureInfo.CurrentUICulture, out _).ToString()
			);
		}

		private bool CanInstallWebRuntimeExecute(object sender)
		{
			return true;
		}

		public WebViewService WebViewService { get; set; }
		public PersistantSettingsService PersistantSettingsService { get; set; }

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>();
		}

		public override void ReadSettings(IDictionary<string, string> settings)
		{
		}

		public override bool CanGoPrevious()
		{
			return false;
		}
	}
}
