using System;
using System.Collections.Generic;
using System.Linq;
using MorestachioMailProcessor.Services.DataImport.Strategies;

namespace MorestachioMailProcessor.Services.DataImport
{
	public class DataImportService : IRequireInit
	{
		public DataImportService()
		{
			MailDataStrategy = new List<IMailDataStrategyMetaViewModel>();
		}

		public IList<IMailDataStrategyMetaViewModel> MailDataStrategy { get; set; }

		public void Init()
		{
			MailDataStrategy = AppDomain.CurrentDomain.GetAssemblies().SelectMany(e => e.GetTypes())
				.Where(e => e.IsClass && !e.IsAbstract)
				.Where(e => typeof(IMailDataStrategyMetaViewModel).IsAssignableFrom(e))
				.Select(e => Activator.CreateInstance(e) as IMailDataStrategyMetaViewModel)
				.ToArray();
		}
	}

	public interface IRequireInit
	{
		void Init();
	}
}
