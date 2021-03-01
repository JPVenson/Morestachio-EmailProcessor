using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Morestachio.MailProcessor.Client.ViewModels.Steps
{
	public class WelcomeStepViewModel : WizardStepBaseViewModel
	{
		public WelcomeStepViewModel()
		{
			Title = new UiLocalizableString("WelcomeStep.Title");
			Description = new UiLocalizableString("WelcomeStep.Description");
			GroupKey = "MainGroup";
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override bool CanGoPrevious()
		{
			return false;
		}
	}
}
