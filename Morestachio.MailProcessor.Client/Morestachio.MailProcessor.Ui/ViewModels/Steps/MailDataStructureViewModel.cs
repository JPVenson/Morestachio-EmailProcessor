using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using JPB.DataAccess.DbInfoConfig;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.Helper;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class MailDataStructureViewModel : ViewModelBase
	{
		static MailDataStructureViewModel()
		{
			InitCoreTypes();
		}

		public MailDataStructureViewModel()
		{
			Children = new ObservableCollection<MailDataStructureViewModel>();
		}

		public ObservableCollection<MailDataStructureViewModel> Children { get; set; }
		public MailDataStructureViewModel Parent { get; set; }

		private string _fieldName;
		private Type _fieldType;

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

		private static void InitCoreTypes()
		{
			_coreTypes = new Dictionary<Type, string>()
			{
				{typeof(string), "string"},
				{typeof(bool), "bool"},
				{typeof(DateTime), "DateTime"},
				{typeof(TimeSpan), "TimeSpan"},
				{typeof(DateTimeOffset), "DateTimeOffset"},
			};

			foreach (var csFrameworkFloatingPointNumberType in Number.CsFrameworkFloatingPointNumberTypes)
			{
				_coreTypes[csFrameworkFloatingPointNumberType] = csFrameworkFloatingPointNumberType.Name;
			}
			foreach (var csFrameworkFloatingPointNumberType in Number.CsFrameworkIntegralTypes)
			{
				_coreTypes[csFrameworkFloatingPointNumberType] = csFrameworkFloatingPointNumberType.Name;
			}
		}

		private static IDictionary<Type, string> _coreTypes;

		public static IEnumerable<MailDataStructureViewModel> GenerateStructure(IDictionary<string, object> values)
		{
			foreach (var value in values)
			{
				yield return GenerateSubStructure(value.Key, value.Value);
			}
		}

		public static MailDataStructureViewModel GenerateSubStructure(string name, object value, MailDataStructureViewModel parent = null)
		{
			var field = new MailDataStructureViewModel();
			field.FieldName = name;
			field.FieldType = value?.GetType() ?? typeof(object);
			field.Parent = parent;
			if (!_coreTypes.ContainsKey(field.FieldType))
			{
				if (value is IDictionary<string, object> duc)
				{
					foreach (var prop in duc)
					{
						field.Children.Add(GenerateSubStructure(prop.Key, prop.Value, field));
					}
				}

				if (value is IDataRecord rec)
				{
					for (int i = 0; i < rec.FieldCount; i++)
					{
						field.Children.Add(GenerateSubStructure(rec.GetName(i), rec.GetValue(i), field));
					}
				}
				else
				{
					foreach (var propertyInfo in field.FieldType.GetProperties())
					{
						field.Children.Add(GenerateSubStructure(propertyInfo.Name, propertyInfo.GetValue(value), field));
					}	
				}
			}

			return field;
		}
	}
}