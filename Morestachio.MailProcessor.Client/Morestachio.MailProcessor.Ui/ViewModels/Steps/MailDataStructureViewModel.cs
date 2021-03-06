using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using JPB.DataAccess.DbInfoConfig;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.Helper;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class MailDataStructureViewModel : ViewModelBase
	{
		public MailDataStructureViewModel()
		{
			Children = new ObservableCollection<MailDataStructureViewModel>();
		}

		public ObservableCollection<MailDataStructureViewModel> Children { get; set; }
		public MailDataStructureViewModel Parent { get; set; }

		private string _fieldName;
		private Type _fieldType;
		private bool _isCollection;
		private bool _complexType;
		private string _displayType;

		public string DisplayType
		{
			get { return _displayType; }
			set { SetProperty(ref _displayType, value); }
		}

		public bool ComplexType
		{
			get { return _complexType; }
			set { SetProperty(ref _complexType, value); }
		}

		public bool IsCollection
		{
			get { return _isCollection; }
			set { SetProperty(ref _isCollection, value); }
		}

		public Type FieldType
		{
			get { return _fieldType; }
			set { SetProperty(ref _fieldType, value); }
		}

		public string FieldName
		{
			get { return _fieldName; }
			set { SetProperty(ref _fieldName, value); }
		}
	}
}