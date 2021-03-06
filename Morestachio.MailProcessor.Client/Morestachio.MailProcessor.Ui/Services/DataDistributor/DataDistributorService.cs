﻿using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies;
using Morestachio.MailProcessor.Ui.Services.DataImport;

namespace Morestachio.MailProcessor.Ui.Services.DataDistributor
{
	public class DataDistributorService : IRequireInit
	{
		public DataDistributorService()
		{
			MailDistributors = new List<IMailDistributorBaseViewModel>();
		}

		public IList<IMailDistributorBaseViewModel> MailDistributors { get; private set; }
		public void Init()
		{
			MailDistributors = AppDomain.CurrentDomain.GetAssemblies().SelectMany(e => e.GetTypes())
				.Where(e => e.IsClass && !e.IsAbstract)
				.Where(e => typeof(IMailDistributorBaseViewModel).IsAssignableFrom(e))
				.Select(e => Activator.CreateInstance(e) as IMailDistributorBaseViewModel)
				.ToArray();
		}
	}
}
