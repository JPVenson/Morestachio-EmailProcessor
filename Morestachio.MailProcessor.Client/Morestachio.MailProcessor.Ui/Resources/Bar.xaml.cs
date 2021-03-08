using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Morestachio.MailProcessor.Ui.Resources
{
	public partial class Bar
	{
		public Bar()
		{
			InitializeComponent();
		}

		private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as Bar).OnProgressChanged();
		}

		private void OnProgressChanged()
		{
			ProgressbarHost.RefreshBars();
		}

		public MultiProgressBar ProgressbarHost { get; set; }

		public static readonly DependencyProperty IsAbsoluteValueProperty = DependencyProperty.Register(
			nameof(IsAbsoluteValue), typeof(bool), typeof(Bar), new PropertyMetadata(default(bool)));

		public static readonly DependencyProperty ComputedProgressProperty = DependencyProperty.Register(
			nameof(ComputedProgress), typeof(double), typeof(Bar), new PropertyMetadata(default(double)));

		public static readonly DependencyProperty ProgressColorProperty = DependencyProperty.Register(
			nameof(ProgressColor), typeof(Brush), typeof(Bar), new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			nameof(Value), typeof(double), typeof(Bar), new PropertyMetadata(default(double), ProgressChanged));

		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
			nameof(Content), typeof(object), typeof(Bar), new PropertyMetadata(default(object)));
		
		public object Content
		{
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public Brush ProgressColor
		{
			get { return (Brush)GetValue(ProgressColorProperty); }
			set { SetValue(ProgressColorProperty, value); }
		}

		public double ComputedProgress
		{
			get { return (double)GetValue(ComputedProgressProperty); }
			set { SetValue(ComputedProgressProperty, value); }
		}

		public bool IsAbsoluteValue
		{
			get { return (bool)GetValue(IsAbsoluteValueProperty); }
			set { SetValue(IsAbsoluteValueProperty, value); }
		}

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public void RefreshBar(Bar previous)
		{
			if (IsAbsoluteValue)
			{
				ComputedProgress = Value;
			}
			else
			{
				ComputedProgress = Value + (previous?.ComputedProgress ?? 0);
			}
		}

		public void RefreshLayout(Bar previous)
		{
			Width = ProgressbarHost.ActualWidth / 100 * (Value / ProgressbarHost.Maximum * 100);
			if (Width < 1 && Value > 1)
			{
				Width = 1;
			}
			Canvas.SetLeft(this, (previous?.Width ?? 0) + (previous != null ? Canvas.GetLeft(previous) : 0));
			//LayoutTransform = new TranslateTransform(, 0);
		}
	}
}