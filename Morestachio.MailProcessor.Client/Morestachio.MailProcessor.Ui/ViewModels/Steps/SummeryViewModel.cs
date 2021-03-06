using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.DataDistributor;
using Morestachio.MailProcessor.Ui.Services.DataImport;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
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
			MailComposer = IoC.Resolve<MailComposer>();
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

		private async void SaveSendReportExecute(object sender)
		{
			var uiWorkflow = IoC.Resolve<IUiWorkflow>();
			var defaultGenericImportStepConfigurator = new DefaultStepConfigurator(this);
			await defaultGenericImportStepConfigurator.AddNextToMe(new SendReportStepViewModel()
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

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(MailComposer.SendInParallel), MailComposer.SendInParallel.ToString()},
				{nameof(MailComposer.ParallelNoOfParallism), MailComposer.ParallelNoOfParallism.ToString()},
				{nameof(MailComposer.ParallelReadAheadCount), MailComposer.ParallelReadAheadCount.ToString()},
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			if (settings.TryGetValue(nameof(MailComposer.SendInParallel), out var sendInParallel))
			{
				MailComposer.SendInParallel = sendInParallel == bool.TrueString;
			}
			if (settings.TryGetValue(nameof(MailComposer.ParallelNoOfParallism), out var noOfParallism))
			{
				MailComposer.ParallelNoOfParallism = int.Parse(noOfParallism);
			}
			if (settings.TryGetValue(nameof(MailComposer.ParallelReadAheadCount), out var readAhead))
			{
				MailComposer.ParallelReadAheadCount = int.Parse(readAhead);
			}
			await base.ReadSettings(settings);
		}

		public override Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			DataStrategyName = IoC.Resolve<DataImportService>().MailDataStrategy
				.First(e => e.Id == MailComposer.MailDataStrategy.Id).Name;

			DistributorName = IoC.Resolve<DataDistributorService>().MailDistributors
				.First(e => e.IdKey == MailComposer.MailDistributor.Id).Name;

			var stringVisitor = new ToParsableStringExpressionVisitor();

			MailComposer.ToAddressExpression.Accept(stringVisitor);
			AddressExpressionString = stringVisitor.StringBuilder.ToString();
			stringVisitor.StringBuilder.Clear();

			MailComposer.SubjectExpression.Accept(stringVisitor);
			SubjectExpressionString = stringVisitor.StringBuilder.ToString();
			stringVisitor.StringBuilder.Clear();

			MailComposer.ToNameExpression.Accept(stringVisitor);
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
				var uiWorkflow = IoC.Resolve<IUiWorkflow>();
				var textService = IoC.Resolve<ITextService>();

				await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
					textService.Compile("Application.Error.Title", CultureInfo.CurrentUICulture, out _).ToString(),
					textService.Compile("Summery.Error.Message", CultureInfo.CurrentUICulture, out _,
						new FormattableArgument(e.Message, false)).ToString()
				);
				return;
			}
			finally
			{
				done.Cancel();
			}

			FinishedAt = DateTime.Now;
			ViewModelAction(() =>
			{
				SendPropertyChanged(() => Progress);
				IsProcessed = true;
				NextButtonText = new UiLocalizableString("Application.Navigation.Forward");
				Commands.Add(new MenuBarCommand(ResetCommand)
				{
					Content = new UiLocalizableString("Summery.Commands.Reset"),
					Tag = "AfterButton.ResetCommand",
					Dock = Dock.Left
				});
				Commands.Add(new MenuBarCommand(SaveSendReportCommand)
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

		public override Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			if (IsProcessed == false)
			{
				SimpleWorkAsync(SendMails);
				return Task.FromResult(false);
			}

			return base.OnGoNext(defaultStepConfigurator);
		}

		public void Report(SendMailProgress value)
		{
			Progress = value;
		}
	}
}
