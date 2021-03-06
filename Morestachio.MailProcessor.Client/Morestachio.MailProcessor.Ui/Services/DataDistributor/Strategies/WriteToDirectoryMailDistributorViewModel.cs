using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Sender;
using Morestachio.MailProcessor.Framework.Sender.Strategies;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies
{
	public class WriteToDirectoryMailDistributorViewModel : MailDistributorBaseViewModel<WriteToDirectoryMailDistributorViewModel.WriteToDirectoryMailDistributorViewModelErrors>
	{
		public class WriteToDirectoryMailDistributorViewModelErrors : ErrorCollection<WriteToDirectoryMailDistributorViewModel>
		{
			public WriteToDirectoryMailDistributorViewModelErrors()
			{
				Add(new Error<WriteToDirectoryMailDistributorViewModel>(new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Errors.InvalidDirectory"),
					e => !System.IO.Directory.Exists(e.Directory),
					nameof(Directory)));
			}
		}

		public WriteToDirectoryMailDistributorViewModel() : base(nameof(Directory))
		{
			Title = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Title");
			Description = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Description");
			IdKey = WriteToDirectoryMailDistributor.IdKey;
			Name = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Name");
			
			Directory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Output");
			if (!System.IO.Directory.Exists(Directory))
			{
				System.IO.Directory.CreateDirectory(Directory);
			}

			PickDirectoryCommand = new DelegateCommand(PickDirectoryExecute, CanPickDirectoryExecute);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(Directory), Directory}
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			Directory = settings.GetOrNull(nameof(Directory))?.ToString();
			await base.ReadSettings(settings);
		}

		public DelegateCommand PickDirectoryCommand { get; private set; }

		private void PickDirectoryExecute(object sender)
		{
			var directoryDialog = new FolderBrowserDialog();
			directoryDialog.ShowNewFolderButton = true;
			directoryDialog.ShowDialog();
			Directory = directoryDialog.SelectedPath;
		}

		private bool CanPickDirectoryExecute(object sender)
		{
			return true;
		}

		private string _directory;

		public string Directory
		{
			get { return _directory; }
			set { SetProperty(ref _directory, value); }
		}

		public override IMailDistributor Create()
		{
			return new WriteToDirectoryMailDistributor(Directory);
		}
	}
}
