using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Framework;
using Morestachio.TemplateContainers;

namespace Morestachio.MailProcessor.Client.ViewModels.Steps
{
	public class TemplateSelectorStepViewModel : WizardStepBaseViewModel<TemplateSelectorStepViewModel.TemplateSelectorStepViewModelErrors>
	{
		public class TemplateSelectorStepViewModelErrors : ErrorCollection<TemplateSelectorStepViewModel>
		{
			public TemplateSelectorStepViewModelErrors()
			{
				Add(new AsyncError<TemplateSelectorStepViewModel>(
					"Template.Errors.InvalidTemplate",
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
