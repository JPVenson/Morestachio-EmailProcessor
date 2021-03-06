using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
			ValidationState = new StepValidationViewModel();

			Commands.Add(new MenuBarCommand(ValidateCommand)
			{
				Content = ValidationState
			});
			MailComposer = IoC.Resolve<MailComposer>();
		}

		public DelegateCommand ValidateCommand { get; private set; }
		public MailComposer MailComposer { get; set; }

		private readonly string[] _contentProperties;

		private void SmtpMailDistributorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_contentProperties.Contains(e.PropertyName))
			{
				ValidationState.IsValidated = false;
			}
		}

		private void ValidateExecute(object sender)
		{
			ValidationState.IsValidated = false;
			SimpleWorkAsync(async () => { await ValidateData(false); });
		}

		private async Task ValidateData(bool silent)
		{
			var uiWorkflow = IoC.Resolve<IUiWorkflow>();
			var textService = IoC.Resolve<ITextService>();
			ProgressDialogController waiter = null;

			if (!silent)
			{
				waiter = await DialogCoordinator.Instance.ShowProgressAsync(uiWorkflow,
					textService.Compile("Validate.Progress.Title", CultureInfo.CurrentUICulture, out _).ToString(),
					textService.Compile("Validate.Progress.Message", CultureInfo.CurrentUICulture, out _).ToString()
				);
				waiter.SetIndeterminate();
			}

			try
			{
				MailData previewData;
				await using (var strategy = Create())
				{
					previewData = await strategy.GetPreviewData();
				}

				ValidationState.IsValidated = previewData != null;

				if (!silent)
				{
					await waiter.CloseAsync();
					if (!ValidationState.IsValidated)
					{
						await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
							textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _)
								.ToString(),
							textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture,
								out _,
								new FormattableArgument("ImportData.Errors.Validate.GeneralError", true)).ToString()
						);
					}
				}
			}
			catch (Exception e)
			{
				if (!silent)
				{
					await waiter.CloseAsync();
					await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
						textService.Compile("ImportData.Errors.Validate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("ImportData.Errors.Validate.Description", CultureInfo.CurrentUICulture, out _,
							new FormattableArgument(e.Message, false)).ToString()
					);
				}
			}
		}

		private bool CanValidateExecute(object sender)
		{
			return !HasError && IsNotWorking;
		}

		public UiLocalizableString Name { get; set; }
		public string Id { get; set; }
		private StepValidationViewModel _validationState;

		public StepValidationViewModel ValidationState
		{
			get { return _validationState; }
			set { SetProperty(ref _validationState, value); }
		}

		public abstract IMailDataStrategy Create();

		public override Task ReadSettings(IDictionary<string, string> settings)
		{
			SimpleWorkAsync(async () => await ValidateData(true));
			return base.ReadSettings(settings);
		}

		public override bool CanGoNext()
		{
			return base.CanGoNext() && ValidationState.IsValidated;
		}

		public override bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator)
		{
			defaultStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
			return base.OnGoPrevious(defaultStepConfigurator);
		}

		public override async Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			if (MailComposer.MailDataStrategy != null)
			{
				await MailComposer.MailDataStrategy.DisposeAsync();
			}

			MailComposer.MailDataStrategy = Create();
			return await base.OnGoNext(defaultStepConfigurator);
		}
	}
}