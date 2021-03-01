using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Morestachio.MailProcessor.Client.Resources
{
	public class FieldsetGrid : Grid
	{
		public FieldsetGrid()
		{
			VerticalAlignment = VerticalAlignment.Top;
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
			ColumnDefinitions.Add(new ColumnDefinition()
			{
				SharedSizeGroup = "Fieldset_Name",
				Width = GridLength.Auto
			});
			ColumnDefinitions.Add(new ColumnDefinition()
			{
				SharedSizeGroup = "Fieldset_Value",
			});
			
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
