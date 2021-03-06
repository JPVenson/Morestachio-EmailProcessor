using System.IO;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class JsonMailDataStrategyViewModel : FileBaseDataStrategyViewModel<JsonMailDataStrategyViewModel.JsonMailDataStrategyViewModelErrors>
	{
		public class JsonMailDataStrategyViewModelErrors : ErrorCollection<JsonMailDataStrategyViewModel>
		{
			public JsonMailDataStrategyViewModelErrors()
			{
				Add(new Error<JsonMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Errors.InvalidPath"),
					e => !File.Exists(e.FilePath), nameof(FilePath)));
			}
		}

		public JsonMailDataStrategyViewModel() : base(JsonFileImportStrategy.IdKey, "json file|*.json")
		{
			Title = new UiLocalizableString("DataImport.Strategy.Json.Title");
			Description = new UiLocalizableString("DataImport.Strategy.Json.Description");
			Name = new UiLocalizableString("DataImport.Strategy.Json.Name");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override IMailDataStrategy Create()
		{
			return new JsonFileImportStrategy(new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read));
		}
	}
}