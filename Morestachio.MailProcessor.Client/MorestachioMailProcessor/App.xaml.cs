using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Morestachio.MailProcessor.Client
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
		[System.STAThreadAttribute()]
		public static void Main()
		{
			var app = new App();
			app.Start();
		}

	}
}
