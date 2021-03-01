﻿using System.IO;
using System.Linq;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Microsoft.Win32;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Framework.Import.Strategies;

namespace Morestachio.MailProcessor.Client.Services.DataImport
{
	public class CSVMailDataStrategyMetaViewModel : MailDataStrategyMetaViewModel<CSVMailDataStrategyMetaViewModel.CSVMailDataStrategyMetaViewModelErrors>
	{
		public class CSVMailDataStrategyMetaViewModelErrors : ErrorCollection<CSVMailDataStrategyMetaViewModel>
		{
			public CSVMailDataStrategyMetaViewModelErrors()
			{
				Add(new Error<CSVMailDataStrategyMetaViewModel>("DataImport.Strategy.CSV.Errors.InvalidPath",
					e => !File.Exists(e.FilePath), nameof(FilePath)));
				Add(new AsyncError<CSVMailDataStrategyMetaViewModel>(
					"DataImport.Strategy.CSV.Errors.InvalidFile",
					async e => (await e.Create().GetMails(0, 1)).FirstOrDefault() == null, nameof(FilePath)));
			}
		}

		public CSVMailDataStrategyMetaViewModel() : base(CsvImportStrategy.IdKey)
		{
			Title = new UiLocalizableString("DataImport.Strategy.CSV.Title");
			Description = new UiLocalizableString("DataImport.Strategy.CSV.Description");
			Name = new UiLocalizableString("DataImport.Strategy.CSV.Name");
			PickFileCommand = new DelegateCommand(PickFileCommandExecute, CanPickFileCommandExecute);

#if DEBUG
			FilePath =
				"H:\\Code\\Morestachio-EmailProcessor\\Morestachio.MailProcessor.Client\\Morestachio.MailProcessor.Client\\TestData.csv";
#endif
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

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
	}
}