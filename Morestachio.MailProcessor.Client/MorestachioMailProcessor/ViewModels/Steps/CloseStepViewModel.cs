using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;

namespace Morestachio.MailProcessor.Client.ViewModels.Steps
{
	public class CloseStepViewModel : WizardStepBaseViewModel
	{
		public CloseStepViewModel()
		{
			Title = new UiLocalizableString("Close.Title");
			Description = new UiLocalizableString("Close.Description");
			GroupKey = "MainGroup";
			NextButtonText = new UiLocalizableString("Application.Header.Close");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override bool CanGoNext()
		{
			return true;
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			App.Current.Shutdown();
			return false;
		}
	}
}
