using System.IO;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Microsoft.Win32;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public class CSVMailDataStrategyMetaViewModel : MailDataStrategyMetaViewModel<CSVMailDataStrategyMetaViewModel.CSVMailDataStrategyMetaViewModelErrors>
	{
		public class CSVMailDataStrategyMetaViewModelErrors : ErrorCollection<CSVMailDataStrategyMetaViewModel>
		{
			public CSVMailDataStrategyMetaViewModelErrors()
			{
				Add(new Error<CSVMailDataStrategyMetaViewModel>(new UiLocalizableString("DataImport.Strategy.CSV.Errors.InvalidPath"),
					e => !File.Exists(e.FilePath), nameof(FilePath)));
				Add(new AsyncError<CSVMailDataStrategyMetaViewModel>(
					new UiLocalizableString("DataImport.Strategy.CSV.Errors.InvalidFile"),
					async e => await e.Create().GetPreviewData() == null, nameof(FilePath)));
			}
		}

		public CSVMailDataStrategyMetaViewModel() : base(CsvImportStrategy.IdKey)
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
			filePicker.ShowDialog(App.Current.MainWindow);
			FilePath = filePicker.FileName;
		}

		private bool CanPickFileCommandExecute(object sender)
		{
			return true;
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

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDataStrategy = Create();
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
	}
}