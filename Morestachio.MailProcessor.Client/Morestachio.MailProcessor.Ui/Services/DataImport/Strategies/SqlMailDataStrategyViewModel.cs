using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class SqlMailDataStrategyViewModel : MailDataStrategyBaseViewModel<SqlMailDataStrategyViewModel.SqlMailDataStrategyViewModelErrors>
	{
		public class SqlMailDataStrategyViewModelErrors : ErrorCollection<SqlMailDataStrategyViewModel>
		{
			public SqlMailDataStrategyViewModelErrors()
			{
				Add(new Error<SqlMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Sql.Errors.NoConnectionString"),
					e => string.IsNullOrWhiteSpace(e.ConnectionString),
					nameof(ConnectionString)));
				Add(new Error<SqlMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.Sql.Errors.NoQuery"),
					e => string.IsNullOrWhiteSpace(e.Query),
					nameof(Query)));
			}
		}

		public SqlMailDataStrategyViewModel() : base(SqlImportStrategy.IdKey,
			nameof(Query),
			nameof(ConnectionString))
		{
			Title = new UiLocalizableString("DataImport.Strategy.Sql.Title");
			Description = new UiLocalizableString("DataImport.Strategy.Sql.Description");
			Name = new UiLocalizableString("DataImport.Strategy.Sql.Name");
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
		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(Query), Query},
				{nameof(ConnectionString), ConnectionString}
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			Query = settings.GetOrNull(nameof(Query))?.ToString();
			ConnectionString = settings.GetOrNull(nameof(ConnectionString))?.ToString();
			await base.ReadSettings(settings);
		}

		public override IMailDataStrategy Create()
		{
			return new SqlImportStrategy(Query, ConnectionString);
		}
	}
}