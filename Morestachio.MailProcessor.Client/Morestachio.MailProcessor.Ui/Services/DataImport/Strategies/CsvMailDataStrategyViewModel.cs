using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Microsoft.Win32;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class CsvMailDataStrategyViewModel : MailDataStrategyBaseViewModel<CsvMailDataStrategyViewModel.CSVMailDataStrategyMetaViewModelErrors>
	{
		public class CSVMailDataStrategyMetaViewModelErrors : ErrorCollection<CsvMailDataStrategyViewModel>
		{
			public CSVMailDataStrategyMetaViewModelErrors()
			{
				Add(new Error<CsvMailDataStrategyViewModel>(new UiLocalizableString("DataImport.Strategy.CSV.Errors.InvalidPath"),
					e => !File.Exists(e.FilePath), nameof(FilePath)));
			}
		}

		public CsvMailDataStrategyViewModel() : base(CsvImportStrategy.IdKey, nameof(FilePath))
		{
			Title = new UiLocalizableString("DataImport.Strategy.CSV.Title");
			Description = new UiLocalizableString("DataImport.Strategy.CSV.Description");
			Name = new UiLocalizableString("DataImport.Strategy.CSV.Name");
			PickFileCommand = new DelegateCommand(PickFileCommandExecute, CanPickFileCommandExecute);
		}

		public DelegateCommand PickFileCommand { get; private set; }

		private void PickFileCommandExecute(object sender)
		{
			var filePicker = new OpenFileDialog();
			filePicker.Filter = "comma seperated file|*.csv";
			if (filePicker.ShowDialog(App.Current.MainWindow) == true)
			{
				FilePath = filePicker.FileName;
			}
		}

		private bool CanPickFileCommandExecute(object sender)
		{
			return IsNotWorking;
		}

		private string _filePath;

		public string FilePath
		{
			get { return _filePath; }
			set
			{
				SendPropertyChanging(() => FilePath);
				_filePath = value;
				SendPropertyChanged(() => FilePath);
			}
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(FilePath), FilePath}
			};
		}

		public override void ReadSettings(IDictionary<string, string> settings)
		{
			FilePath = settings.GetOrNull(nameof(FilePath))?.ToString();
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() 
			       && !string.IsNullOrWhiteSpace(FilePath)
			       && File.Exists(FilePath);
		}

		public override IMailDataStrategy Create()
		{
			return new CsvImportStrategy(FilePath);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
	}
}