using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework;

namespace Morestachio.MailProcessor.Client.Services.DataDistributor.Strategies
{
	public class MailDistributorSelectorViewModel : WizardStepBaseViewModel
	{
		public MailDistributorSelectorViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Selector.Title");
			Description = new UiLocalizableString("MailDistributor.Selector.Description");
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

		public override Task OnEntry(IDictionary<string, object> data)
		{
			var mailDistributor = IoC.Resolve<MailComposer>().MailDistributor;
			MailDistributors = new ObservableCollection<IMailDistributorBaseViewModel>(IoC.Resolve<DataDistributorService>().MailDistributors);
			SelectedDistributor =
				MailDistributors.FirstOrDefault(e => e.IdKey == mailDistributor?.Id);
			return base.OnEntry(data);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.AddNextToMe(SelectedDistributor);
			IoC.Resolve<MailComposer>().MailDistributor = SelectedDistributor.Create();
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}
