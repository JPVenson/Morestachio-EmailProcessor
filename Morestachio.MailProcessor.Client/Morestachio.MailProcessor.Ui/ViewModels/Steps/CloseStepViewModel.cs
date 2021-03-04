using System.Collections.Generic;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class CloseStepViewModel : WizardStepBaseViewModel
	{
		public CloseStepViewModel()
		{
			Title = new UiLocalizableString("Close.Title");
			Description = new UiLocalizableString("Close.Description");
			Description = new UiLocalizableString("");
			GroupKey = "MainGroup";
			NextButtonText = new UiLocalizableString("Application.Header.Close");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override bool CanGoNext()
		{
			return true;
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>();
		}

		public override void ReadSettings(IDictionary<string, string> settings)
		{
		}

		public override bool OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			App.Current.Shutdown();
			return false;
		}
	}
}
