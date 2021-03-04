using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Morestachio.MailProcessor.Ui.Resources.Steps
{
	/// <summary>
	/// Interaction logic for PreviewTemplateWindow.xaml
	/// </summary>
	public partial class PreviewTemplateWindow
	{
		public PreviewTemplateWindow()
		{
			InitializeComponent();
			
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			WebView2.Dispose();
			base.OnClosing(e);
		}
	}
}
