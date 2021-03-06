using System.IO;
using System.Linq;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class CsvMailDataStrategyViewModel : FileBaseDataStrategyViewModel<CsvMailDataStrategyViewModel.CSVMailDataStrategyMetaViewModelErrors>
	{
		public class CSVMailDataStrategyMetaViewModelErrors : ErrorCollection<CsvMailDataStrategyViewModel>
		{
			public CSVMailDataStrategyMetaViewModelErrors()
			{
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Errors.InvalidPath"),
					e => !File.Exists(e.FilePath), nameof(FilePath)));
			}
		}

		public CsvMailDataStrategyViewModel() : base(CsvImportStrategy.IdKey, "comma seperated file|*.csv", nameof(FilePath))
		{
			Title = new UiLocalizableString("DataImport.Strategy.CSV.Title");
			Description = new UiLocalizableString("DataImport.Strategy.CSV.Description");
			Name = new UiLocalizableString("DataImport.Strategy.CSV.Name");
		}

		public override IMailDataStrategy Create()
		{
			return new CsvImportStrategy(new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
	}
}