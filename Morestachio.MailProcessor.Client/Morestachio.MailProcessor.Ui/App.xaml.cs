using System;
using System.Windows;

namespace Morestachio.MailProcessor.Ui
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			
		}

		public void Start()
		{
			var bootstrapper = new Bootstrapper();
			bootstrapper.Start(this);
		}


		/// <summary>
		/// Application Entry Point.
		/// </summary>
		[STAThread()]
		public static void Main()
		{
			var app = new App();
			app.Start();
		}
	}
}
