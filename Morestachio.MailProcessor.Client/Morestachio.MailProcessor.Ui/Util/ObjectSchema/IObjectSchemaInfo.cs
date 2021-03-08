using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using Morestachio.Helper;
using Morestachio.MailProcessor.Ui.ViewModels.Steps;

namespace Morestachio.MailProcessor.Ui.Util.ObjectSchema
{
	public static class ObjectSchemaGenerator
	{
		public static Dictionary<Type, string> CoreTypes { get; }
		public static Dictionary<string, string> TypeCsToTsMapping { get; }

		static ObjectSchemaGenerator()
		{
			CoreTypes = new Dictionary<Type, string>()
			{
				{typeof(string), "string"},
				{typeof(bool), "bool"},
				{typeof(object), "object"},
				{typeof(DateTime), "DateTime"},
				{typeof(TimeSpan), "TimeSpan"},
				{typeof(DateTimeOffset), "DateTimeOffset"},
				{typeof(IDictionary<string, object>), "DynamicObject"},
			};

			foreach (var csFrameworkFloatingPointNumberType in Number.CsFrameworkFloatingPointNumberTypes)
			{
				CoreTypes[csFrameworkFloatingPointNumberType] = csFrameworkFloatingPointNumberType.Name;
			}
			foreach (var csFrameworkFloatingPointNumberType in Number.CsFrameworkIntegralTypes)
			{
				CoreTypes[csFrameworkFloatingPointNumberType] = csFrameworkFloatingPointNumberType.Name;
			}

			TypeCsToTsMapping = new Dictionary<string, string>();
			TypeCsToTsMapping.Add("String", "string");
			TypeCsToTsMapping.Add("SecureString", "string | password");
			TypeCsToTsMapping.Add(nameof(DateTime), "string | Date");
			TypeCsToTsMapping.Add(nameof(DateTimeOffset), "string | Date");
			TypeCsToTsMapping.Add("Int64", "number");
			TypeCsToTsMapping.Add("Int32", "number");
			TypeCsToTsMapping.Add("Int16", "number");
			TypeCsToTsMapping.Add("Single", "number");
			TypeCsToTsMapping.Add("Decimal", "number");
			TypeCsToTsMapping.Add("Byte", "number");
			TypeCsToTsMapping.Add("Double", "number");
			TypeCsToTsMapping.Add("Boolean", "boolean");
			TypeCsToTsMapping.Add("Void", "void");
			TypeCsToTsMapping.Add("Object", "any");
		}
		
		public static IEnumerable<MailDataStructureViewModel> GenerateStructure(object value)
		{
			if (value is IDictionary<string, object> duc)
			{
				foreach (var prop in duc)
				{
					yield return GenerateSubStructure(prop.Key, prop.Value);
				}
			}
			else if (value is IEnumerable list && !(value is string))
			{
				var first = list.ToDynamicArray().FirstOrDefault() as object;
				if (!CoreTypes.ContainsKey(first?.GetType() ?? typeof(object)))
				{
					foreach (var item in GenerateStructure(first))
					{
						yield return item;
					}
				}
			}
			else if (value is IDataRecord rec)
			{
				for (int i = 0; i < rec.FieldCount; i++)
				{
					yield return GenerateSubStructure(rec.GetName(i), rec.GetValue(i), rec.GetFieldType(i));
				}
			}
			else
			{
				foreach (var propertyInfo in value.GetType().GetProperties())
				{
					yield return GenerateSubStructure(propertyInfo.Name, propertyInfo.GetValue(value), propertyInfo.PropertyType);
				}
			}
		}

		public static MailDataStructureViewModel GenerateSubStructure(string name, object value, Type supposedType = null)
		{
			var field = new MailDataStructureViewModel();
			field.FieldName = name;
			var fieldType = value?.GetType() ?? supposedType ?? typeof(object);
			field.FieldType = fieldType;
			field.IsCollection = typeof(IEnumerable).IsAssignableFrom(field.FieldType) 
			                     && !CoreTypes.ContainsKey(fieldType);
			field.ComplexType = !field.IsCollection && !CoreTypes.ContainsKey(fieldType) || (value is IDictionary<string, object>);

			if (field.ComplexType || field.IsCollection)
			{
				foreach (var mailDataStructureViewModel in GenerateStructure(value))
				{
					mailDataStructureViewModel.Parent = field;
					field.Children.Add(mailDataStructureViewModel);
				}
			}
			field.DisplayType = TypeToDisplayName(field, value);

			return field;
		}

		private static string TypeToDisplayName(MailDataStructureViewModel field, object value)
		{
			if (field.IsCollection && !field.ComplexType)
			{
				if (field.Children.Count == 0)
				{
					Type fieldType = value.GetType().GetGenericArguments().FirstOrDefault();
					if (fieldType == null)
					{
						fieldType = value.GetType().GetElementType();
					}

					if (fieldType == typeof(object))
					{
						var first = (value as IEnumerable).ToDynamicArray().FirstOrDefault() as object;
						fieldType = first?.GetType();
					}

					return $"Array of '{TranslateCsType(fieldType)}'";
				}

				return $"Array";
			}
			
			return TranslateCsType(field.FieldType);
		}

		private static string TranslateCsType(Type type)
		{
			if (type.IsEnum)
			{
				return Enum.GetNames(type).Aggregate((e, f) => e + " | " + f);
			}

			if (TypeCsToTsMapping.ContainsKey(type.Name))
			{
				return TypeCsToTsMapping[type.Name];
			}

			return null;
		}
	}
}