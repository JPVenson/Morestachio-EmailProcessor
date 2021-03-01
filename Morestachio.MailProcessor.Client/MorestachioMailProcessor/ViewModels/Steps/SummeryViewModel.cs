using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.MailProcessor.Client.Services.DataDistributor;
using Morestachio.MailProcessor.Client.Services.DataImport;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Framework;

namespace Morestachio.MailProcessor.Client.ViewModels.Steps
{
	public class SummeryStepViewModel : WizardStepBaseViewModel, IProgress<MailComposer.SendMailProgress>
	{
		public SummeryStepViewModel()
		{
			Title = new UiLocalizableString("Summery.Title");
			Description = new UiLocalizableString("Summery.Description");
			Progress = new MailComposer.SendMailProgress("", 0, 100);
			NextButtonText = new UiLocalizableString("Application.Header.StartSend");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public MailComposer MailComposer { get; set; }

		public UiLocalizableString DataStrategyName { get; set; }
		public UiLocalizableString DistributorName { get; set; }

		private MailComposer.SendMailProgress _progress;

		public MailComposer.SendMailProgress Progress
		{
			get { return _progress; }
			set { _progress = value; }
		}

		public override Task OnEntry(IDictionary<string, object> data)
		{
			IsProcessed = false;
			MailComposer = IoC.Resolve<MailComposer>();
			DataStrategyName = IoC.Resolve<DataImportService>().MailDataStrategy
				.First(e => e.Id == MailComposer.MailDataStrategy.Id).Name;

			DistributorName = IoC.Resolve<DataDistributorService>().MailDistributors
				.First(e => e.IdKey == MailComposer.MailDistributor.Id).Name;

			return base.OnEntry(data);
		}

		public bool IsProcessed { get; set; }

		private async Task SendMails()
		{
			var done = new CancellationTokenSource();
			var task = Task.Run(async () =>
			{
				while (!done.IsCancellationRequested)
				{
					await Task.Delay(1000, done.Token);
					SendPropertyChanged(() => Progress);
				}
			}, done.Token);
			await MailComposer.ComposeAndSend(this);
			done.Cancel();
			ViewModelAction(() =>
			{
				SendPropertyChanged(() => Progress);
				IsProcessed = true;
				NextButtonText = new UiLocalizableString("Application.Navigation.Forward");
			});
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			if (IsProcessed == false)
			{
				SimpleWorkAsync(SendMails);
				return false;
			}

			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}

		public void Report(MailComposer.SendMailProgress value)
		{
			Progress = value;
		}
	}
}
