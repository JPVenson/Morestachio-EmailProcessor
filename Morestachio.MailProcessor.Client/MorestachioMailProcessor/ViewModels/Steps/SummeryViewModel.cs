using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.MailProcessor.Client;
using Morestachio.MailProcessor.Client.Services.DataDistributor;
using Morestachio.MailProcessor.Client.Services.DataImport;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework;
using MorestachioMailProcessor.Services.UiWorkflow;

namespace MorestachioMailProcessor.ViewModels.Steps
{
	public class SummeryStepViewModel : WizardStepBaseViewModel, IProgress<SendMailProgress>
	{
		public SummeryStepViewModel()
		{
			Title = new UiLocalizableString("Summery.Title");
			Description = new UiLocalizableString("Summery.Description");
			Progress = new SendMailProgress("", 0, 0, true);
			NextButtonText = new UiLocalizableString("Application.Header.StartSend");
			ResetCommand = new DelegateCommand(ResetExecute, CanResetExecute);
			SaveSendReportCommand = new DelegateCommand(SaveSendReportExecute, CanSaveSendReportExecute);
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public MailComposer MailComposer { get; set; }

		public string AddressExpressionString { get; set; }
		public string SubjectExpressionString { get; set; }
		public string NameExpressionString { get; set; }

		public UiLocalizableString DataStrategyName { get; set; }
		public UiLocalizableString DistributorName { get; set; }


		private DateTime _createdAt;
		private DateTime _finishedAt;
		private SendMailProgress _progress;
		private bool _isProcessed;
		private ComposeMailResult _result;

		public DateTime FinishedAt
		{
			get { return _finishedAt; }
			set { SetProperty(ref _finishedAt, value); }
		}

		public DateTime CreatedAt
		{
			get { return _createdAt; }
			set { SetProperty(ref _createdAt, value); }
		}

		public ComposeMailResult Result
		{
			get { return _result; }
			set { SetProperty(ref _result, value); }
		}

		public bool IsProcessed
		{
			get { return _isProcessed; }
			set { SetProperty(ref _isProcessed, value); }
		}

		public SendMailProgress Progress
		{
			get { return _progress; }
			set { _progress = value; }
		}

		public DelegateCommand ResetCommand { get; private set; }
		public DelegateCommand SaveSendReportCommand { get; private set; }

		private void SaveSendReportExecute(object sender)
		{
			var uiWorkflow = IoC.Resolve<IUiWorkflow>();
			var defaultGenericImportStepConfigurator = new DefaultGenericImportStepConfigurator(uiWorkflow, this);
			defaultGenericImportStepConfigurator.AddNextToMe(new SendReportStepViewModel()
			{
				SummeryStepViewModel = this
			});
			uiWorkflow.NextPageCommand.Execute(null);
		}

		private bool CanSaveSendReportExecute(object sender)
		{
			return true;
		}

		private void ResetExecute(object sender)
		{
			Progress = new SendMailProgress("", 0, 100, true);
			Result = null;
			IsProcessed = false;
			Commands.RemoveWhere(e => ((string)e.Tag).StartsWith("AfterButton."));
			SendPropertyChanged(() => Progress);
			NextButtonText = new UiLocalizableString("Application.Header.StartSend");
		}

		private bool CanResetExecute(object sender)
		{
			return IsProcessed;
		}

		public override Task OnEntry(IDictionary<string, object> data,
			DefaultGenericImportStepConfigurator configurator)
		{
			MailComposer = IoC.Resolve<MailComposer>();
			DataStrategyName = IoC.Resolve<DataImportService>().MailDataStrategy
				.First(e => e.Id == MailComposer.MailDataStrategy.Id).Name;

			DistributorName = IoC.Resolve<DataDistributorService>().MailDistributors
				.First(e => e.IdKey == MailComposer.MailDistributor.Id).Name;

			var stringVisitor = new ToParsableStringExpressionVisitor();

			MailComposer.AddressExpression.Accept(stringVisitor);
			AddressExpressionString = stringVisitor.StringBuilder.ToString();
			stringVisitor.StringBuilder.Clear();

			MailComposer.SubjectExpression.Accept(stringVisitor);
			SubjectExpressionString = stringVisitor.StringBuilder.ToString();
			stringVisitor.StringBuilder.Clear();

			MailComposer.NameExpression.Accept(stringVisitor);
			NameExpressionString = stringVisitor.StringBuilder.ToString();
			stringVisitor.StringBuilder.Clear();
			return base.OnEntry(data, configurator);
		}

		private async Task SendMails()
		{
			CreatedAt = DateTime.Now;

			var done = new CancellationTokenSource();
			var task = Task.Run(async () =>
			{
				while (!done.IsCancellationRequested)
				{
					await Task.Delay(1000, done.Token);
					SendPropertyChanged(() => Progress);
				}
			}, done.Token);
			try
			{
				Result = await MailComposer.ComposeAndSend(this);
			}
			catch (Exception e)
			{
				//todo add exception handling
				Console.WriteLine(e);
				throw;
			}

			FinishedAt = DateTime.Now;
			done.Cancel();
			ViewModelAction(() =>
			{
				SendPropertyChanged(() => Progress);
				IsProcessed = true;
				NextButtonText = new UiLocalizableString("Application.Navigation.Forward");
				Commands.Add(new UiDelegateCommand(ResetCommand)
				{
					Content = new UiLocalizableString("Summery.Commands.Reset"),
					Tag = "AfterButton.ResetCommand",
				});
				Commands.Add(new UiDelegateCommand(SaveSendReportCommand)
				{
					Content = new UiLocalizableString("Summery.Commands.Report"),
					Tag = "AfterButton.ReportCommand",
				});
			});
		}

		public override bool CanGoPrevious()
		{
			return base.CanGoPrevious() && !IsProcessed;
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

		public void Report(SendMailProgress value)
		{
			Progress = value;
		}
	}
}
