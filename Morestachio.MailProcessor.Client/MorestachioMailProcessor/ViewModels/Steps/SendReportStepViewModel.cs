using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Microsoft.Win32;
using Morestachio;
using Morestachio.MailProcessor.Client;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Client.ViewModels.Steps;
using Morestachio.MailProcessor.Framework;

namespace MorestachioMailProcessor.ViewModels.Steps
{
	public class SendReportStepViewModel : WizardStepBaseViewModel
	{
		public SendReportStepViewModel()
		{
			GroupKey = "Report";
			SaveReportCommand = new DelegateCommand(SaveReportExecute, CanSaveReportExecute);
			RefreshReportCommand = new DelegateCommand(RefreshReportExecute, CanRefreshReportExecute);

			Commands.Add(new UiDelegateCommand(SaveReportCommand)
			{
				Content = new UiLocalizableString("SendReport.Save.Title")
			});	
			
			Commands.Add(new UiDelegateCommand(RefreshReportCommand)
			{
				Content = new UiLocalizableString("SendReport.Refresh.Title")
			});
			Title = new UiLocalizableString("SaveReport.Title");
			Description = new UiLocalizableString("SaveReport.Description");
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public SummeryStepViewModel SummeryStepViewModel { get; set; }

		private string _report;

		public string Report
		{
			get { return _report; }
			set { SetProperty(ref _report, value); }
		}

		public DelegateCommand SaveReportCommand { get; private set; }
		public MailComposer MailComposer { get; set; }

		public DelegateCommand RefreshReportCommand { get; private set; }

		private void RefreshReportExecute(object sender)
		{
			SimpleWorkAsync(async () =>
			{
				var addressCompiled = MailComposer.AddressExpression.Compile();
				var data = new Dictionary<string, object>();
				data["failedToSend"] = SummeryStepViewModel.Result.SendFailed.Select(e =>
				{
					return new Dictionary<string, object>()
					{
						{"ComposeValues", MailComposer.Compose(e.Key, null, addressCompiled, null, null)},
						{"FailReason", e.Value.Error}
					};
				});
				data["numSuccess"] = SummeryStepViewModel.Result.SendSuccessfully;
				data["startedAt"] = SummeryStepViewModel.CreatedAt;
				data["doneAt"] = SummeryStepViewModel.FinishedAt;

				var template = await File.ReadAllTextAsync("SendReportTemplate.mdoc.html");
				Report = (await (await Parser.ParseWithOptionsAsync(new ParserOptions(template)))
					.CreateAndStringifyAsync(data));
			});
		}

		private bool CanRefreshReportExecute(object sender)
		{
			return IsNotWorking;
		}

		private void SaveReportExecute(object sender)
		{
			var dialog = new SaveFileDialog();
			if (dialog.ShowDialog(App.Current.MainWindow) == true)
			{
				File.WriteAllText(dialog.FileName, Report);
				Process.Start(dialog.FileName);
			}
		}

		private bool CanSaveReportExecute(object sender)
		{
			return IsNotWorking;
		}

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == "Report");
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}

		public override Task OnEntry(IDictionary<string, object> data,
			DefaultGenericImportStepConfigurator configurator)
		{
			MailComposer = IoC.Resolve<MailComposer>();

			if (Report == null)
			{
				Report = "...";
				RefreshReportExecute(null);
			}

			return base.OnEntry(data, configurator);
		}
	}
}
