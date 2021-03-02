using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel();
		}
	}
}
