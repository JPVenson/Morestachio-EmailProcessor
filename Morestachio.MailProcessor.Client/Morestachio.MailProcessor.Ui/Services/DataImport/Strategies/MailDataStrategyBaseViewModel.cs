using System;
using System.Globalization;
using System.Linq;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.Services.DataImport.Strategies
{
	public abstract class MailDataStrategyBaseViewModel<TErrors> : 
		WizardStepBaseViewModel<TErrors>, 
		IMailDataStrategyMetaViewModel 
		where TErrors : IErrorCollectionBase, new()
	{
		protected MailDataStrategyBaseViewModel(string id, params string[] contentProperties)
		{
			Id = id;
			GroupKey = "ImportData";
			ValidateCommand = new DelegateCommand(ValidateExecute, CanValidateExecute);
			PropertyChanged += SmtpMailDistributorViewModel_PropertyChanged;
			_contentProperties = contentProperties;
			ValidateionState = new StepValidationViewModel();

			Commands.Add(new MenuBarCommand(ValidateCommand)
			{
				Content = ValidateionState
			});
		}		
		
		public DelegateCommand ValidateCommand { get; private set; }
		
		private readonly string[] _contentProperties;

		private void SmtpMailDistributorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_contentProperties.Contains(e.PropertyName))
			{
				ValidateionState.IsValidated = false;
			}
		}

		private void ValidateExecute(object sender)
		{
			ValidateionState.IsValidated = false;
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
					var strategy = Create();
					var previewData = await strategy.GetPreviewData();
					ValidateionState.IsValidated = previewData != null;
					await waiter.CloseAsync();
					if (!ValidateionState.IsValidated)
					{
						await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
							textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
							textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
								new FormattableArgument("ImportData.Errors.Validate.GeneralError", true)).ToString()
						);
					}
				}
				catch (Exception e)
				{
					await waiter.CloseAsync();
					await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
						textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
							new FormattableArgument(e.Message, false)).ToString()
					);
				}
			});
		}

		private bool CanValidateExecute(object sender)
		{
			return !HasError && IsNotWorking;
		}

		public UiLocalizableString Name { get; set; }
		public string Id { get; set; }
		private StepValidationViewModel _validateionState;

		public StepValidationViewModel ValidateionState
		{
			get { return _validateionState; }
			set { SetProperty(ref _validateionState, value); }
		}

		public abstract IMailDataStrategy Create();

		public override bool CanGoNext()
		{
			return base.CanGoNext() && ValidateionState.IsValidated;
		}

		public override bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator)
		{
			defaultStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
			return base.OnGoPrevious(defaultStepConfigurator);
		}

		public override bool OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDataStrategy = Create();
			return base.OnGoNext(defaultStepConfigurator);
		}
	}
}