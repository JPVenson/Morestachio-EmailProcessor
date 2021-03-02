using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using MorestachioMailProcessor.Services.TextService;

namespace MorestachioMailProcessor.ViewModels
{
	public class UiLocalizableString : ViewModelBase, IUiLocalizableString, IEquatable<UiLocalizableString>
	{
		public UiLocalizableString(string key, params FormattableArgument[] arguments)
		{
			Key = key;
			Arguments = new ThreadSaveObservableCollection<FormattableArgument>(arguments);
		}

		private string _key;
		private ICollection<FormattableArgument> _arguments;

		public ICollection<FormattableArgument> Arguments
		{
			get { return _arguments; }
			set
			{
				SendPropertyChanging(() => Arguments);
				_arguments = value;
				SendPropertyChanged(() => Arguments);
			}
		}

		public string Key
		{
			get { return _key; }
			set
			{
				SendPropertyChanging(() => Key);
				_key = value;
				SendPropertyChanged(() => Key);
			}
		}

		public object Resolve(ITextService textService)
		{
			return textService.Compile(Key, CultureInfo.CurrentUICulture, out _, Arguments.ToArray());
		}

		//  User-defined conversion from double to Digit
		public static implicit operator String(UiLocalizableString d)
		{
			return d.Key;
		}

		//  User-defined conversion from double to Digit
		public static implicit operator UiLocalizableString(string d)
		{
			return new UiLocalizableString(d);
		}

		public override string ToString()
		{
			if (Arguments.Any())
			{
				return $"Loc: {Key}: {Arguments.Select(f => f.ToString()).Aggregate((e, f) => $"{e}, {f}")}";
			}

			return "Loc:" + Key;
		}

		public bool Equals(UiLocalizableString other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			if (!string.Equals(_key, other._key))
			{
				return false;
			}

			if (_arguments.Count != other.Arguments.Count)
			{
				return false;
			}

			return _arguments.Select((item, index) => new
			{
				Left = item,
				Right = other.Arguments.ElementAt(index),
			}).Any(f => f.Left.KeyOrValue == f.Right.KeyOrValue);
		}
	}
}