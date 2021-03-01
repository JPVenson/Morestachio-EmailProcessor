using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;

namespace Morestachio.MailProcessor.Client.ViewModels
{
	public abstract class WizardStepBaseViewModel : WizardStepBaseViewModel<NoErrors>
	{
		
	}

	public interface IWizardStepBaseViewModel
	{
		UiLocalizableString Title { get; }
		UiLocalizableString Description { get; }
		ObservableCollection<UiDelegateCommand> Commands { get; set; }
		string GroupKey { get; set; }
		void OnAdded(DefaultGenericImportStepConfigurator configurator);
		void OnRemoved(DefaultGenericImportStepConfigurator configurator);
		Task OnEntry(IDictionary<string, object> data);
		bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator);
		bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator);
		bool CanGoNext();
		bool CanGoPrevious();
	}

	public abstract class WizardStepBaseViewModel<TErrors> : AsyncErrorProviderBase<TErrors>, IWizardStepBaseViewModel where TErrors : IErrorCollectionBase, new()
	{
		protected WizardStepBaseViewModel()
		{
			Commands = new ObservableCollection<UiDelegateCommand>();
			NextButtonText = new UiLocalizableString("Application.Navigation.Forward");
			PreviousButtonText = new UiLocalizableString("Application.Navigation.Back");
		}
		
		public abstract UiLocalizableString Title { get; }
		public abstract UiLocalizableString Description { get; }
		public ObservableCollection<UiDelegateCommand> Commands { get; set; }

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

		public virtual void OnAdded(DefaultGenericImportStepConfigurator configurator)
		{

		}

		public virtual void OnRemoved(DefaultGenericImportStepConfigurator configurator)
		{
			configurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
		}

		public virtual async Task OnEntry(IDictionary<string, object> data)
		{
			Data = data;
			await Task.CompletedTask;
		}

		public virtual bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			return true;
		}

		public virtual bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
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