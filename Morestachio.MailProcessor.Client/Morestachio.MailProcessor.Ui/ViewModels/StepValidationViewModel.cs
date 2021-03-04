using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace Morestachio.MailProcessor.Ui.ViewModels
{
	public class MenuBarCommand : UiDelegateCommand
	{
		public MenuBarCommand(ICommand command) : base(command)
		{
		}

		public MenuBarCommand(Dispatcher dispatcher, ICommand command) : base(dispatcher, command)
		{
		}

		private Dock _dock = Dock.Right;

		public Dock Dock
		{
			get { return _dock; }
			set { SetProperty(ref _dock, value); }
		}
	}

	public class StepValidationViewModel : ViewModelBase
	{
		private bool _isValidated;

		public bool IsValidated
		{
			get { return _isValidated; }
			set { SetProperty(ref _isValidated, value); }
		}
	}
}
