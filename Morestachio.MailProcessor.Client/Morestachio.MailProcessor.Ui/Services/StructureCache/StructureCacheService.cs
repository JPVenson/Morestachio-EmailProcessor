using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Util.ObjectSchema;
using Morestachio.MailProcessor.Ui.ViewModels.Steps;

namespace Morestachio.MailProcessor.Ui.Services.StructureCache
{
	public class StructureCacheService
	{
		public StructureCacheService()
		{
			DataStructure = new ThreadSaveObservableCollection<MailDataStructureViewModel>();
		}

		public MailData ExampleMailData { get; private set; }
		public ThreadSaveObservableCollection<MailDataStructureViewModel> DataStructure { get; }

		public void SetExampleData(MailData exampleData)
		{
			DataStructure.Clear();
			ExampleMailData = exampleData;
			if (exampleData != null)
			{
				DataStructure.AddEach(ObjectSchemaGenerator.GenerateStructure(exampleData));
			}
		}
	}
}
