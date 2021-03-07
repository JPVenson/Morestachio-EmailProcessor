using System.Windows;
using System.Windows.Controls;

namespace Morestachio.MailProcessor.Ui.Resources
{
	public partial class FieldsetGrid : Grid
	{
		public FieldsetGrid()
		{
			VerticalAlignment = VerticalAlignment.Top;
			HorizontalAlignment = HorizontalAlignment.Left;
			InitializeComponent();
			Margin = new Thickness(7,15,15,7);
		}

		public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
			nameof(Columns), typeof(int), typeof(FieldsetGrid), new PropertyMetadata(default));

		public int Columns
		{
			get { return (int) GetValue(ColumnsProperty); }
			set { SetValue(ColumnsProperty, value); }
		}

		public override void EndInit()
		{
			for (int i = 0; i < Columns - 2; i++)
			{
				ColumnDefinitions.Add(new ColumnDefinition()
				{
					SharedSizeGroup = "Fieldset_Value_" + i
				});
			}
			RowDefinitions.Add(new RowDefinition()
			{
				
			});
			base.EndInit();
		}

		public override void BeginInit()
		{
			

			base.BeginInit();
		}
	}
}
