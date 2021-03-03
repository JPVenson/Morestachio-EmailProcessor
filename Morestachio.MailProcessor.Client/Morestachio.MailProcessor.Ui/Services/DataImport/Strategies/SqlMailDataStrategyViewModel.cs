using JPB.WPFToolsAwesome.Error.ValidationRules;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class SqlMailDataStrategyViewModel : MailDataStrategyBaseViewModel<NoErrors>
	{
		public SqlMailDataStrategyViewModel() : base(SqlImportStrategy.IdKey,
			nameof(Query),
			nameof(ConnectionString))
		{
			Title = new UiLocalizableString("DataImport.Strategy.Sql.Title");
			Description = new UiLocalizableString("DataImport.Strategy.Sql.Description");
			Name = new UiLocalizableString("DataImport.Strategy.Sql.Name");
			IsValidated = false;
			//Query = "SELECT * FROM Address WHERE EMailAddress IS NOT NULL";
			//ConnectionString = @"Server=.\V17;Database=JPB.MyWorksheet.Database;Trusted_Connection=True;";
		}

		private string _connectionString;
		private string _query;

		public string Query
		{
			get { return _query; }
			set { SetProperty(ref _query, value); }
		}

		public string ConnectionString
		{
			get { return _connectionString; }
			set { SetProperty(ref _connectionString, value); }
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public override IMailDataStrategy Create()
		{
			return new SqlImportStrategy(Query, ConnectionString);
		}
	}
}