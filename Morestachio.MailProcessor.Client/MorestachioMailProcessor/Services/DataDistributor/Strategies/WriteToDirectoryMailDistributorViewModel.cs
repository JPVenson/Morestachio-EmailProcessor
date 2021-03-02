using System.IO;
using System.Windows.Forms;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework.Sender;
using Morestachio.MailProcessor.Framework.Sender.Strategies;
using MorestachioMailProcessor.Services.DataDistributor.Strategies;

namespace Morestachio.MailProcessor.Client.Services.DataDistributor.Strategies
{
	public class WriteToDirectoryMailDistributorViewModel : MailDistributorBaseViewModel<WriteToDirectoryMailDistributorViewModel.WriteToDirectoryMailDistributorViewModelErrors>
	{
		public class WriteToDirectoryMailDistributorViewModelErrors : ErrorCollection<WriteToDirectoryMailDistributorViewModel>
		{
			public WriteToDirectoryMailDistributorViewModelErrors()
			{
				Add(new Error<WriteToDirectoryMailDistributorViewModel>("MailDistributor.Strategy.ToDirectory.Errors.InvalidDirectory",
					e => !System.IO.Directory.Exists(e.Directory),
					nameof(Directory)));
			}
		}

		public WriteToDirectoryMailDistributorViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Title");
			Description = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Description");
			IdKey = WriteToDirectoryMailDistributor.IdKey;
			Name = new UiLocalizableString("MailDistributor.Strategy.ToDirectory.Name");

#if DEBUG
			Directory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Output");
#endif
			PickDirectoryCommand = new DelegateCommand(PickDirectoryExecute, CanPickDirectoryExecute);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

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
