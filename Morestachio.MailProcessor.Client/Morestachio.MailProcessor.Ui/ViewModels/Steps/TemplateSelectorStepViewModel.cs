using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Services.MailTemplates;
using Morestachio.MailProcessor.Ui.Services.StructureCache;
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
			Structure = new ThreadSaveObservableCollection<MailDataStructureViewModel>();
			SetTemplateCommand = new DelegateCommand(SetTemplateExecute, CanSetTemplateExecute);
			MailTemplateService = IoC.Resolve<MailTemplateService>();
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
		public ThreadSaveObservableCollection<MailDataStructureViewModel> Structure { get; set; }
		public MailData ExampleMailData { get; set; }
		public DelegateCommand SetTemplateCommand { get; private set; }

		public MailTemplateService MailTemplateService { get; set; }
		private MailTemplate _selectedTemplate;
		private string _template;

		public MailTemplate SelectedTemplate
		{
			get { return _selectedTemplate; }
			set { SetProperty(ref _selectedTemplate, value); }
		}

		public string Template
		{
			get { return _template; }
			set { SetProperty(ref _template, value); }
		}


		private void SetTemplateExecute(object sender)
		{
			SimpleWorkAsync(async () =>
			{
				Template = (await MailTemplateService.ObtainTemplate(SelectedTemplate)) ?? Template;
			});
		}

		private bool CanSetTemplateExecute(object sender)
		{
			return IsNotWorking && SelectedTemplate != null;
		}

		public override Task OnEntry(IDictionary<string, object> data, DefaultGenericImportStepConfigurator configurator)
		{
			ExampleMailData = IoC.Resolve<StructureCacheService>().ExampleMailData;
			Structure.Clear();
			Structure.AddEach(MailDataStructureViewModel.GenerateStructure(ExampleMailData.Data));

			return base.OnEntry(data, configurator);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().Template = Template;
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}
