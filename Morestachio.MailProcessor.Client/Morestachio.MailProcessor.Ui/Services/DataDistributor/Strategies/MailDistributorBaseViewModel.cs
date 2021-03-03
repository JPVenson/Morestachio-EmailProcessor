using System;
using System.Globalization;
using System.Linq;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Sender;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies
{
	public abstract class MailDistributorBaseViewModel<TErrors> : WizardStepBaseViewModel<TErrors>, IMailDistributorBaseViewModel where TErrors : IErrorCollectionBase, new()
	{
		protected MailDistributorBaseViewModel(params string[] contentProperties)
		{
			_contentProperties = contentProperties;
			GroupKey = "Distributors";
			PropertyChanged += SmtpMailDistributorViewModel_PropertyChanged;

			TestConnectionCommand = new DelegateCommand(TestConnectionExecute, CanTestConnectionExecute);
			IsValidated = false;
			
			Commands.Add(new UiDelegateCommand(TestConnectionCommand)
			{
				Content = new UiLocalizableString("MailDistributor.Strategy.Smtp.Validate.Title")
			});
		}

		private readonly string[] _contentProperties;

		private void SmtpMailDistributorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_contentProperties.Contains(e.PropertyName))
			{
				IsValidated = false;
			}
		}


		public DelegateCommand TestConnectionCommand { get; private set; }
		private void TestConnectionExecute(object sender)
		{
			IsValidated = false;
			SimpleWorkAsync(async () =>
			{
				var uiWorkflow = IoC.Resolve<IUiWorkflow>();
				var textService = IoC.Resolve<ITextService>();
				
				var waiter = await DialogCoordinator.Instance.ShowProgressAsync(uiWorkflow,
					textService.Compile("Validate.Progress.Title", CultureInfo.CurrentUICulture, out _).ToString(),
					textService.Compile("Validate.Progress.Message", CultureInfo.CurrentUICulture, out _).ToString()
				);
				waiter.SetIndeterminate();
				try
				{
					var mailDistributor = Create();
					var beginResult = await mailDistributor.BeginSendMail();
					if (!beginResult.Success)
					{
						await waiter.CloseAsync();
						await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
							textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
							textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
								new FormattableArgument(beginResult.ErrorText, false)).ToString()
						);
						return;
					}

					var endResult = await mailDistributor.EndSendMail(beginResult);
					if (!endResult.Success)
					{
						await waiter.CloseAsync();
						await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
							textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
							textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
								new FormattableArgument(endResult.ErrorText, false)).ToString()
						);

						return;
					}
					await waiter.CloseAsync();
				}
				catch (Exception e)
				{
					await waiter.CloseAsync();
					await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
						textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
							new FormattableArgument("ImportData.Errors.Validate.GeneralError", true)).ToString()
					);
					return;
				}

				IsValidated = true;
			});
		}

		private bool CanTestConnectionExecute(object sender)
		{
			return IsNotWorking && !HasError;
		}

		public UiLocalizableString Name { get; set; }
		public string IdKey { get; set; }
		private bool _isValidated;

		public bool IsValidated
		{
			get { return _isValidated; }
			set { SetProperty(ref _isValidated, value); }
		}

		public abstract IMailDistributor Create();

		public override bool CanGoNext()
		{
			return base.CanGoNext() && IsValidated;
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDistributor = Create();
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == "Distributors");
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}
	}
}
