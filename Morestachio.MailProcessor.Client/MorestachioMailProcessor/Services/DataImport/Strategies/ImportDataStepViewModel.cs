using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Client.Services.DataImport;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;

namespace Morestachio.MailProcessor.Client.ViewModels.Steps.Import
{
	public class ImportDataStepViewModel : WizardStepBaseViewModel
	{
		public ImportDataStepViewModel()
		{
			Title = new UiLocalizableString("DataImport.Selector.Title");
			Description = new UiLocalizableString("DataImport.Selector.Description");
			GroupKey = "MainGroup";
		}

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

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDataStrategy = SelectedStrategy.Create();
			defaultGenericImportStepConfigurator.AddNextToMe(SelectedStrategy);
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}

		public override async Task OnEntry(IDictionary<string, object> data,
			DefaultGenericImportStepConfigurator configurator)
		{
			await base.OnEntry(data, configurator);
			var dataImportService = IoC.Resolve<DataImportService>();

			var mailComposer = IoC.Resolve<MailComposer>();
			MailDataStrategies = dataImportService.MailDataStrategy;
			SelectedStrategy = dataImportService.MailDataStrategy
				.FirstOrDefault(e => e.Id == mailComposer.MailDataStrategy.Id);
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() && SelectedStrategy != null;
		}
	}
}
