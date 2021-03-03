using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.DataDistributor;
using Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class MailDistributorSelectorViewModel : WizardStepBaseViewModel
	{
		public MailDistributorSelectorViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Selector.Title");
			Description = new UiLocalizableString("MailDistributor.Selector.Description");
			Description = new UiLocalizableString("");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

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

		public override Task OnEntry(IDictionary<string, object> data,
			DefaultGenericImportStepConfigurator configurator)
		{
			var mailDistributor = IoC.Resolve<MailComposer>().MailDistributor;
			MailDistributors = new ObservableCollection<IMailDistributorBaseViewModel>(IoC.Resolve<DataDistributorService>().MailDistributors);
			SelectedDistributor =
				MailDistributors.FirstOrDefault(e => e.IdKey == mailDistributor?.Id);
			return base.OnEntry(data, configurator);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.AddNextToMe(SelectedDistributor);
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}
