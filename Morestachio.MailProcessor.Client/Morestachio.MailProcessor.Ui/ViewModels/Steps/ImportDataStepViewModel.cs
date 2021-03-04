using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.DataImport;
using Morestachio.MailProcessor.Ui.Services.DataImport.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class ImportDataSelectorStepViewModel : WizardStepBaseViewModel
	{
		public ImportDataSelectorStepViewModel()
		{
			Title = new UiLocalizableString("DataImport.Selector.Title");
			Description = new UiLocalizableString("DataImport.Selector.Description");
			GroupKey = "MainGroup";
			DataImportService = IoC.Resolve<DataImportService>();
		}

		public DataImportService DataImportService { get; set; }
		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		private IMailDataStrategyMetaViewModel _selectedStrategy;
		private IList<IMailDataStrategyMetaViewModel> _mailDataStrategies;

		public IList<IMailDataStrategyMetaViewModel> MailDataStrategies
		{
			get { return _mailDataStrategies; }
			set
			{
				SendPropertyChanging(() => MailDataStrategies);
				_mailDataStrategies = value;
				SendPropertyChanged(() => MailDataStrategies);
			}
		}

		public IMailDataStrategyMetaViewModel SelectedStrategy
		{
			get { return _selectedStrategy; }
			set
			{
				SendPropertyChanging(() => SelectedStrategy);
				_selectedStrategy = value;
				SendPropertyChanged(() => SelectedStrategy);
			}
		}

		public override bool OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			defaultStepConfigurator.AddNextToMe(SelectedStrategy);
			return base.OnGoNext(defaultStepConfigurator);
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{"ImportDataStep.SelectedImporterStrategy", SelectedStrategy?.Id}
			};
		}

		public override void ReadSettings(IDictionary<string, string> settings)
		{
			if (settings.TryGetValue("ImportDataStep.SelectedImporterStrategy", out var strategyId))
			{
				IoC.Resolve<MailComposer>().MailDataStrategy = DataImportService.MailDataStrategy
					.FirstOrDefault(e => e.Id == strategyId)?.Create();
			}
		}

		public override async Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			await base.OnEntry(data, configurator);

			var mailComposer = IoC.Resolve<MailComposer>();
			MailDataStrategies = DataImportService.MailDataStrategy;
			SelectedStrategy = DataImportService.MailDataStrategy
				.FirstOrDefault(e => e.Id == mailComposer.MailDataStrategy?.Id);
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() && SelectedStrategy != null;
		}
	}
}
