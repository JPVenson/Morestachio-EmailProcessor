using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.DataDistributor;
using Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class MailDistributorSelectorViewModel : WizardStepBaseViewModel
	{
		public MailDistributorSelectorViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Selector.Title");
			Description = new UiLocalizableString("MailDistributor.Selector.Description");
			Description = new UiLocalizableString("");
			MailComposer = IoC.Resolve<MailComposer>();
			DataDistributorService = IoC.Resolve<DataDistributorService>();
			MailDistributors = new ObservableCollection<IMailDistributorBaseViewModel>(DataDistributorService.MailDistributors);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
		public MailComposer MailComposer { get; set; }
		public DataDistributorService DataDistributorService { get; set; }

		private ObservableCollection<IMailDistributorBaseViewModel> _mailDistributors;
		private IMailDistributorBaseViewModel _selectedDistributor;

		public IMailDistributorBaseViewModel SelectedDistributor
		{
			get { return _selectedDistributor; }
			set { SetProperty(ref _selectedDistributor, value); }
		}

		public ObservableCollection<IMailDistributorBaseViewModel> MailDistributors
		{
			get { return _mailDistributors; }
			set { SetProperty(ref _mailDistributors, value); }
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() && SelectedDistributor != null;
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{ "MailDistributor.SelectedStrategy", SelectedDistributor?.IdKey }
			};
		}

		public override void ReadSettings(IDictionary<string, string> settings)
		{
			if (settings.TryGetValue("MailDistributor.SelectedStrategy", out var selectedDistributorId))
			{
				SelectedDistributor =
					MailDistributors.FirstOrDefault(e => e.IdKey == selectedDistributorId);
				MailComposer.MailDistributor = SelectedDistributor?.Create();
			}
		}

		public override Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			SelectedDistributor =
				MailDistributors.FirstOrDefault(e => e.IdKey == MailComposer.MailDistributor?.Id);
			return base.OnEntry(data, configurator);
		}

		public override bool OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			defaultStepConfigurator.AddNextToMe(SelectedDistributor);
			return base.OnGoNext(defaultStepConfigurator);
		}
	}
}
