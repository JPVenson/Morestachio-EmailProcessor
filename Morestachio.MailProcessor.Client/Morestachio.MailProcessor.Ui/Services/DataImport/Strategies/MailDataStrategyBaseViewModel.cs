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
			Commands.Add(new UiDelegateCommand(ValidateCommand)
			{
				Content = new UiLocalizableString("MailDistributor.Strategy.Smtp.Validate.Title")
			});
		}		
		
		public DelegateCommand ValidateCommand { get; private set; }
		
		private readonly string[] _contentProperties;

		private void SmtpMailDistributorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_contentProperties.Contains(e.PropertyName))
			{
				IsValidated = false;
			}
		}

		private void ValidateExecute(object sender)
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
					var strategy = Create();
					var previewData = await strategy.GetPreviewData();
					IsValidated = previewData != null;
					await waiter.CloseAsync();
					if (!IsValidated)
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
		private bool _isValidated;

		public bool IsValidated
		{
			get { return _isValidated; }
			set { SetProperty(ref _isValidated, value); }
		}

		public abstract IMailDataStrategy Create();

		public override bool CanGoNext()
		{
			return base.CanGoNext() && IsValidated;
		}

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			IoC.Resolve<MailComposer>().MailDataStrategy = Create();
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}