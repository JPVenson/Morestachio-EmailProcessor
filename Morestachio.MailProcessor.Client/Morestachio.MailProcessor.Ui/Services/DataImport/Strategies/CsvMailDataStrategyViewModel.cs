using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.Services.Settings;
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

				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("Application.Errors.RequiredField", 
						new FormattableArgument("DataImport.Strategy.CSV.Field.Delimiter", true)),
					e => string.IsNullOrWhiteSpace(e.Delimiter),
					nameof(Delimiter)));
				
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("Application.Errors.RequiredField", 
						new FormattableArgument("DataImport.Strategy.CSV.Field.QuoteChar", true)),
					e => e.IgnoreQuotes && string.IsNullOrWhiteSpace(e.QuoteCharacter.ToString()),
					nameof(QuoteCharacter), nameof(IgnoreQuotes)));
				
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Errors.CharRequired", 
						new FormattableArgument("DataImport.Strategy.CSV.Field.QuoteChar", true)),
					e => e.IgnoreQuotes && e.QuoteCharacter.Length != 1,
					nameof(QuoteCharacter), nameof(IgnoreQuotes)));
				
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("Application.Errors.RequiredField", 
						new FormattableArgument("DataImport.Strategy.CSV.Field.CommentChar", true)),
					e => e.AllowComments && string.IsNullOrWhiteSpace(e.CommentDelimiter.ToString()),
					nameof(CommentDelimiter), nameof(AllowComments)));
				
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Errors.CharRequired", 
						new FormattableArgument("DataImport.Strategy.CSV.Field.CommentChar", true)),
					e => e.AllowComments && e.CommentDelimiter.Length != 1,
					nameof(CommentDelimiter), nameof(AllowComments)));
			}
		}

		public CsvMailDataStrategyViewModel() : base(CsvImportStrategy.IdKey,
			"comma seperated file|*.csv",
			nameof(FilePath),
			nameof(SelectedRegionData),
			nameof(QuoteCharacter),
			nameof(IgnoreQuotes),
			nameof(IgnoreEmptyLines),
			nameof(EscapeCharacter),
			nameof(Delimiter),
			nameof(CommentDelimiter),
			nameof(AllowComments)
			)
		{
			Title = new UiLocalizableString("DataImport.Strategy.CSV.Title");
			Description = new UiLocalizableString("DataImport.Strategy.CSV.Description");
			Name = new UiLocalizableString("DataImport.Strategy.CSV.Name");
			var config = new Configuration(CultureInfo.CurrentUICulture);
			SelectedRegionData = new RegionInfo(config.CultureInfo.LCID);
			QuoteCharacter = config.Quote.ToString();
			IgnoreQuotes = config.IgnoreQuotes;
			IgnoreEmptyLines = config.IgnoreBlankLines;
			EscapeCharacter = config.Escape.ToString();
			Delimiter = config.Delimiter;
			CommentDelimiter = config.Comment.ToString();
			AllowComments = config.AllowComments;
		}

		public override IMailDataStrategy Create()
		{
			var config = new Configuration();
			config.CultureInfo = CultureInfo.GetCultureInfo(SelectedRegionData.Name);
			config.Quote = QuoteCharacter[0];
			config.IgnoreQuotes = IgnoreQuotes;
			config.IgnoreBlankLines = IgnoreEmptyLines;
			config.Escape = EscapeCharacter[0];
			config.Delimiter = Delimiter;
			config.Comment = CommentDelimiter[0];
			config.AllowComments = AllowComments;
			return new CsvImportStrategy(new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
				config);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		private string _delimiter;
		private string _commentDelimiter;
		private bool _allowComments;
		private string _escapeCharacter;
		private bool _ignoreEmptyLines;
		private bool _ignoreQuotes;
		private string _quoteCharacter;
		private RegionInfo _selectedRegionData;

		public RegionInfo SelectedRegionData
		{
			get { return _selectedRegionData; }
			set { SetProperty(ref _selectedRegionData, value); }
		}

		public string QuoteCharacter
		{
			get { return _quoteCharacter; }
			set { SetProperty(ref _quoteCharacter, value); }
		}

		public bool IgnoreQuotes
		{
			get { return _ignoreQuotes; }
			set { SetProperty(ref _ignoreQuotes, value); }
		}

		public bool IgnoreEmptyLines
		{
			get { return _ignoreEmptyLines; }
			set { SetProperty(ref _ignoreEmptyLines, value); }
		}

		public string EscapeCharacter
		{
			get { return _escapeCharacter; }
			set { SetProperty(ref _escapeCharacter, value); }
		}

		public bool AllowComments
		{
			get { return _allowComments; }
			set { SetProperty(ref _allowComments, value); }
		}

		public string CommentDelimiter
		{
			get { return _commentDelimiter; }
			set { SetProperty(ref _commentDelimiter, value); }
		}

		public string Delimiter
		{
			get { return _delimiter; }
			set { SetProperty(ref _delimiter, value); }
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			var settings = await base.SaveSetting();
			settings[nameof(QuoteCharacter)] = QuoteCharacter;
			settings[nameof(IgnoreQuotes)] = IgnoreQuotes.ToString();
			settings[nameof(IgnoreEmptyLines)] = IgnoreEmptyLines.ToString();
			settings[nameof(EscapeCharacter)] = EscapeCharacter;
			settings[nameof(AllowComments)] = AllowComments.ToString();
			settings[nameof(Delimiter)] = Delimiter;
			settings[nameof(CommentDelimiter)] = CommentDelimiter;
			return settings;
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			QuoteCharacter = settings.GetOrNull(nameof(QuoteCharacter)) ?? QuoteCharacter;
			EscapeCharacter = settings.GetOrNull(nameof(EscapeCharacter)) ?? EscapeCharacter;
			IgnoreQuotes = settings.GetOrNull(nameof(IgnoreQuotes)) == bool.TrueString;
			IgnoreEmptyLines = settings.GetOrNull(nameof(IgnoreEmptyLines)) == bool.TrueString;
			AllowComments = settings.GetOrNull(nameof(AllowComments)) == bool.TrueString;
			Delimiter = settings.GetOrNull(nameof(Delimiter));
			CommentDelimiter = settings.GetOrNull(nameof(CommentDelimiter)) ?? CommentDelimiter;

			await base.ReadSettings(settings);
		}
	}
}