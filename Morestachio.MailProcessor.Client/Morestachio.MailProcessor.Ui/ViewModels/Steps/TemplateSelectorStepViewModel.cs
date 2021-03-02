using System.Linq;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.TemplateContainers;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class TemplateSelectorStepViewModel : WizardStepBaseViewModel<TemplateSelectorStepViewModel.TemplateSelectorStepViewModelErrors>
	{
		public class TemplateSelectorStepViewModelErrors : ErrorCollection<TemplateSelectorStepViewModel>
		{
			public TemplateSelectorStepViewModelErrors()
			{
				Add(new AsyncError<TemplateSelectorStepViewModel>(
					new UiLocalizableString("Template.Errors.InvalidTemplate"),
					async e => (await Parser.Validate(new StringTemplateContainer(e.Template))).Any(),
					nameof(Template)));
			}
		}

		public TemplateSelectorStepViewModel()
		{
			Title = new UiLocalizableString("Template.Title");
			Description = new UiLocalizableString("Template.Description");

#if DEBUG
			Template = "{{Addresse}}";
#endif
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		private string _template;

		public string Template
		{
			get { return _template; }
			set { SetProperty(ref _template, value); }
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().Template = Template;
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}
