using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Xaml.Behaviors;
using Morestachio.MailProcessor.Ui.Services.TextService;

namespace Morestachio.MailProcessor.Ui.Behaviors
{
	public class HyperlinkOpenBehaviour : Behavior<Hyperlink>
	{
		public static readonly DependencyProperty ConfirmNavigationProperty = DependencyProperty.Register(
			nameof(ConfirmNavigation), typeof(bool), typeof(HyperlinkOpenBehaviour), new PropertyMetadata(default(bool)));

		public bool ConfirmNavigation
		{
			get { return (bool) GetValue(ConfirmNavigationProperty); }
			set { SetValue(ConfirmNavigationProperty, value); }
		}

		/// <inheritdoc />
		protected override void OnAttached()
		{
			this.AssociatedObject.RequestNavigate += NavigationRequested;
			this.AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
			base.OnAttached();
		}

		private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs e)
		{
			this.AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
			this.AssociatedObject.RequestNavigate -= NavigationRequested;
		}

		private void NavigationRequested(object sender, RequestNavigateEventArgs e)
		{
			if (!ConfirmNavigation || MessageBox.Show("Are you sure?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				OpenUrl();
			}

			e.Handled = true;
		}

		private void OpenUrl()
		{
			Process.Start(new ProcessStartInfo(AssociatedObject.NavigateUri.AbsoluteUri){ UseShellExecute = true });
		}

		/// <inheritdoc />
		protected override void OnDetaching()
		{
			this.AssociatedObject.RequestNavigate -= NavigationRequested;
			base.OnDetaching();
		}
	}

	public class UiLocalizableStringResolverConverter : IValueConverter, IMultiValueConverter
	{
		public bool ExpectFormattingArgumentsOn2ndPlace { get; set; }

		private static Lazy<ITextService> TextService = new Lazy<ITextService>(() => IoC.Resolve<ITextService>());

		public static string FormatOpt(string s, string optional, params string[] param)
		{
			StringBuilder result = new StringBuilder();
			int index = 0;
			bool opened = false;
			Stack<string> stack = new Stack<string>(param.Reverse());

			foreach (var c in s)
			{
				if (c == '{')
				{
					opened = true;
					index = result.Length;
				}
				else if (opened && c == '}')
				{
					opened = false;
					var p = stack.Count > 0 ? stack.Pop() : optional;
					var lenToRem = result.Length - index;
					result.Remove(index, lenToRem);
					result.Append(p);
					continue;
				}
				else if (opened && !Char.IsDigit(c))
				{
					opened = false;
				}

				result.Append(c);
			}

			return result.ToString();
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string strValue)
			{
				var arguments = new List<object>();
				var result = TextService.Value.Compile(strValue, CultureInfo.CurrentUICulture, out _);
				if (!(result is string resultStr))
				{
					return result;
				}
				switch (parameter)
				{
					case IEnumerable<string> strArgs:
						arguments.AddRange(strArgs);
						break;
					//case IEnumerable<ITranslatableArgument> formattableArguments:
					//	{
					//		foreach (var formattableArgument in formattableArguments)
					//		{
					//			arguments.Add(formattableArgument.ToStringWith(TextService.Value));
					//		}

					//		break;
					//	}
				}

				return FormatOpt(resultStr, "", arguments.Select(f => f.ToString()).ToArray());
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length < 1)
			{
				return values;
			}

			if (values.Length == 2 && ExpectFormattingArgumentsOn2ndPlace)
			{
				return Convert(values.FirstOrDefault(), targetType, values[1], culture);
			}

			return Convert(values.FirstOrDefault(), targetType, values.Skip(1), culture);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
