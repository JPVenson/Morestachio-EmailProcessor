using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using WPFLocalizeExtension.Providers;

namespace Morestachio.MailProcessor.Ui.Resources
{
	/// <summary>
	/// Interaction logic for MultiProgressBar.xaml
	/// </summary>
	[ContentProperty("Bars")]
	public partial class MultiProgressBar
	{
		public MultiProgressBar()
		{
			Bars = new BarModels(this);
			InitializeComponent();
			Loaded += MultiProgressBar_Loaded;
			MouseDoubleClick += OnMouseDoubleClick;
		}

		private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			RefreshBars();
		}

		private void MultiProgressBar_Loaded(object sender, RoutedEventArgs e)
		{
			RefreshBars();
		}

		public static readonly DependencyProperty BarsProperty = DependencyProperty.Register(
			nameof(Bars), typeof(BarModels), typeof(MultiProgressBar), new PropertyMetadata(default(BarModels)));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
			nameof(Maximum), typeof(int), typeof(MultiProgressBar), new PropertyMetadata(default(int)));

		public int Maximum
		{
			get { return (int) GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public BarModels Bars
		{
			get { return (BarModels) GetValue(BarsProperty); }
			set { SetValue(BarsProperty, value); }
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			RefreshBars();
		}

		public void RefreshBars()
		{
			Bar previous = null;
			foreach (var barModel in Bars.Reverse())
			{
				barModel.RefreshBar(previous);
				previous = barModel;
			}

			previous = null;
			foreach (var barModel in Bars.Reverse())
			{
				barModel.RefreshLayout(previous);
				previous = barModel;
			}
		}
	}
}
