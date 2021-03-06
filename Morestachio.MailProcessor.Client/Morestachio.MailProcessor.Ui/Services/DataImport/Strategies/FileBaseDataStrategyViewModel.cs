using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Microsoft.Win32;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public interface IFileBaseDataStrategyViewModel
	{
		string FilePath { get; set; }
		DelegateCommand PickFileCommand { get; }
	}

	public abstract class FileBaseDataStrategyViewModel<TErrors> : MailDataStrategyBaseViewModel<TErrors>, IFileBaseDataStrategyViewModel where TErrors : IErrorCollectionBase, new()
	{
		private readonly string _filter;

		protected FileBaseDataStrategyViewModel(string id,
			string filter,
			params string[] contentProperties) : base(id, contentProperties.Concat(new[] { nameof(FilePath) }).ToArray())
		{
			_filter = filter;
			PickFileCommand = new DelegateCommand(PickFileCommandExecute, CanPickFileCommandExecute);
		}

		private string _filePath;

		public string FilePath
		{
			get { return _filePath; }
			set { SetProperty(ref _filePath, value); }
		}
		public DelegateCommand PickFileCommand { get; private set; }

		private void PickFileCommandExecute(object sender)
		{
			var filePicker = new OpenFileDialog();
			filePicker.Filter = _filter;
			if (filePicker.ShowDialog(App.Current.MainWindow) == true)
			{
				FilePath = filePicker.FileName;
			}
		}

		private bool CanPickFileCommandExecute(object sender)
		{
			return IsNotWorking;
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(FilePath), FilePath}
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			FilePath = settings.GetOrNull(nameof(FilePath))?.ToString();
			await base.ReadSettings(settings);
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext()
				   && !string.IsNullOrWhiteSpace(FilePath)
				   && File.Exists(FilePath);
		}
	}
}