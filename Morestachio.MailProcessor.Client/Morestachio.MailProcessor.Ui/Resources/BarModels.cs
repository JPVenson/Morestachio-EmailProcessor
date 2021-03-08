using System.Collections.ObjectModel;

namespace Morestachio.MailProcessor.Ui.Resources
{
	public class BarModels : Collection<Bar>
	{
		public Collection<Bar> Collection { get; set; }
		private readonly MultiProgressBar _multiProgressBar;

		public BarModels(MultiProgressBar multiProgressBar)
		{
			_multiProgressBar = multiProgressBar;
		}

		protected override void InsertItem(int index, Bar item)
		{
			item.ProgressbarHost = _multiProgressBar;
			base.InsertItem(index, item);
		}
	}
}