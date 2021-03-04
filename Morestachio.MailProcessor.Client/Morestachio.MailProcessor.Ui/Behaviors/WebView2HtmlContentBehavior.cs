using System;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using Morestachio.MailProcessor.Ui.Services.WebView;

namespace Morestachio.MailProcessor.Ui.Behaviors
{
	public class WebView2HtmlContentBehavior : Behavior<WebView2>
	{
		public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
			nameof(Html), typeof(string), typeof(WebView2HtmlContentBehavior), new PropertyMetadata(default(string), HtmlChanged));

		private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as WebView2HtmlContentBehavior).HtmlValueChanged(e.NewValue as string);
		}

		private async void HtmlValueChanged(string eNewValue)
		{
			if (eNewValue == null)
			{
				return;
			}

			try
			{
				await AssociatedObject.EnsureCoreWebView2Async(IoC.Resolve<WebViewService>().CoreWebView2Environment);
				AssociatedObject.NavigateToString(eNewValue);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		public string Html
		{
			get { return (string) GetValue(HtmlProperty); }
			set { SetValue(HtmlProperty, value); }
		}

		protected override void OnDetaching()
		{
			AssociatedObject.Dispose();
			base.OnDetaching();
		}

		protected override void OnAttached()
		{
			//try
			//{
			//	var coreWebView2Environment = IoC.Resolve<WebViewService>().CoreWebView2Environment;
			//	AssociatedObject.EnsureCoreWebView2Async().GetAwaiter().GetResult();
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e);
			//	throw;
			//}
		}
	}
}