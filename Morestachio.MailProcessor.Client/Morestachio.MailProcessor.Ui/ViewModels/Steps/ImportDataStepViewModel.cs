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
			MailComposer = IoC.Resolve<MailComposer>();
		}

		public DataImportService DataImportService { get; set; }
		public MailComposer MailComposer { get; set; }
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

		public override async Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			await defaultStepConfigurator.AddNextToMe(SelectedStrategy);
			return await base.OnGoNext(defaultStepConfigurator);
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{"ImportDataStep.SelectedImporterStrategy", SelectedStrategy?.Id}
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			if (settings.TryGetValue("ImportDataStep.SelectedImporterStrategy", out var strategyId))
			{
				if (MailComposer.MailDataStrategy != null)
				{
					await MailComposer.MailDataStrategy.DisposeAsync();
				}

				SelectedStrategy = DataImportService.MailDataStrategy
					.FirstOrDefault(e => e.Id == strategyId);
			}
		}

		public override async Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			await base.OnEntry(data, configurator);
			
			MailDataStrategies = DataImportService.MailDataStrategy;
			SelectedStrategy = SelectedStrategy ?? DataImportService.MailDataStrategy
				.FirstOrDefault(e => e.Id == MailComposer.MailDataStrategy?.Id);
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() && SelectedStrategy != null;
		}
	}
}
