﻿using Morestachio.MailProcessor.Ui.Services.UiWorkflow;

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

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			App.Current.Shutdown();
			return false;
		}
	}
}