namespace MorestachioMailProcessor.ViewModels.Steps
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
