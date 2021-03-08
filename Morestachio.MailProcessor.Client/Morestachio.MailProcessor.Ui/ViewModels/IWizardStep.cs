using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

namespace Morestachio.MailProcessor.Ui.ViewModels
{
	public abstract class WizardStepBaseViewModel : WizardStepBaseViewModel<NoErrors>
	{
		
	}

	public interface IWizardStepBaseViewModel
	{
		UiLocalizableString Title { get; }
		UiLocalizableString Description { get; }
		ObservableCollection<MenuBarCommand> Commands { get; set; }
		string GroupKey { get; set; }
		Task<IDictionary<string, string>> SaveSetting();
		Task ReadSettings(IDictionary<string, string> settings);

		void OnAdded(DefaultStepConfigurator configurator);
		void OnRemoved(DefaultStepConfigurator configurator);
		Task OnEntry(IDictionary<string, object> data, DefaultStepConfigurator configurator);
		Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator);
		bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator);
		bool CanGoNext();
		bool CanGoPrevious();
	}

	public abstract class WizardStepBaseViewModel<TErrors> : AsyncErrorProviderBase<TErrors>,
			IWizardStepBaseViewModel
		where TErrors : IErrorCollectionBase, new()
	{
		protected WizardStepBaseViewModel()
		{
			Commands = new ObservableCollection<MenuBarCommand>();
			NextButtonText = new UiLocalizableString("Application.Navigation.Forward");
			PreviousButtonText = new UiLocalizableString("Application.Navigation.Back");
			//ActiveValidationCases.First();
		}

		public abstract UiLocalizableString Title { get; }
		public abstract UiLocalizableString Description { get; }
		public ObservableCollection<MenuBarCommand> Commands { get; set; }

		private UiLocalizableString _nextButtonText;
		private UiLocalizableString _previousButtonText;

		public UiLocalizableString PreviousButtonText
		{
			get { return _previousButtonText; }
			set { SetProperty(ref _previousButtonText, value); }
		}

		public UiLocalizableString NextButtonText
		{
			get { return _nextButtonText; }
			set { SetProperty(ref _nextButtonText, value); }
		}

		public string GroupKey { get; set; }

		private IDictionary<string, object> _data;

		public IDictionary<string, object> Data
		{
			get { return _data; }
			set
			{
				SendPropertyChanging(() => Data);
				_data = value;
				SendPropertyChanged(() => Data);
			}
		}

		public virtual Task<IDictionary<string, string>> SaveSetting()
		{
			return Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>());
		}

		public virtual Task ReadSettings(IDictionary<string, string> settings)
		{
			return Task.CompletedTask;
		}

		public virtual void OnAdded(DefaultStepConfigurator configurator)
		{

		}

		public virtual void OnRemoved(DefaultStepConfigurator configurator)
		{
			configurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
		}

		public virtual async Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			Data = data;
			await Task.CompletedTask;
		}

		public virtual Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			return Task.FromResult(true);
		}

		public virtual bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator)
		{
			return true;
		}

		public virtual bool CanGoNext()
		{
			return IsNotWorking && !HasError;
		}

		public virtual bool CanGoPrevious()
		{
			return IsNotWorking;
		}
	}
}