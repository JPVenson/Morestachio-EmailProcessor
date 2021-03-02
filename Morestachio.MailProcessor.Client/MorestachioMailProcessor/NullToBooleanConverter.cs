using System;
using System.Globalization;
using System.Windows.Data;
using Morestachio.MailProcessor.Client.ViewModels;

namespace MorestachioMailProcessor
{
	public class NullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class TranslationNullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is UiLocalizableString uiLocalizable)
			{
				return !string.IsNullOrWhiteSpace(uiLocalizable.Key);
			}

			return value == null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}