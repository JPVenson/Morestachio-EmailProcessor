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
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

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
			ValidationState = new StepValidationViewModel();
			
			Commands.Add(new MenuBarCommand(TestConnectionCommand)
			{
				Content = ValidationState
			});
		}

		private readonly string[] _contentProperties;

		private void SmtpMailDistributorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_contentProperties.Contains(e.PropertyName))
			{
				ValidationState.IsValidated = false;
			}
		}


		public DelegateCommand TestConnectionCommand { get; private set; }
		private void TestConnectionExecute(object sender)
		{
			ValidationState.IsValidated = false;
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
				catch (Exception)
				{
					await waiter.CloseAsync();
					await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
						textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
							new FormattableArgument("ImportData.Errors.Validate.GeneralError", true)).ToString()
					);
					return;
				}

				ValidationState.IsValidated = true;
			});
		}

		private bool CanTestConnectionExecute(object sender)
		{
			return IsNotWorking && !HasError;
		}

		public UiLocalizableString Name { get; set; }
		public string IdKey { get; set; }
		private StepValidationViewModel _validationState;

		public StepValidationViewModel ValidationState
		{
			get { return _validationState; }
			set { SetProperty(ref _validationState, value); }
		}

		public abstract IMailDistributor Create();

		public override bool CanGoNext()
		{
			return base.CanGoNext() && ValidationState.IsValidated;
		}

		public override bool OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDistributor = Create();
			return base.OnGoNext(defaultStepConfigurator);
		}

		public override bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator)
		{
			defaultStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == "Distributors");
			return base.OnGoPrevious(defaultStepConfigurator);
		}
	}
}
