using System;
using System.Security.Cryptography;
using JPB.WPFToolsAwesome.Error;
using JPB.WPFToolsAwesome.Extensions;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using Morestachio.Framework.Context;
using Morestachio.Framework.Tokenizing;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework.Import;

namespace Morestachio.MailProcessor.Client.Services.DataImport
{
	public interface IMailDataStrategyMetaViewModel : IWizardStepBaseViewModel
	{
		UiLocalizableString Name { get; set; }
		string Id { get; set; }
		IMailDataStrategy Create();
	}

	public abstract class MailDataStrategyMetaViewModel<Errors> : WizardStepBaseViewModel<Errors>, IMailDataStrategyMetaViewModel where Errors : IErrorCollectionBase, new()
	{
		public MailDataStrategyMetaViewModel(string id)
		{
			Id = id;
			GroupKey = "ImportData";
		}

		public UiLocalizableString Name { get; set; }
		public string Id { get; set; }

		public abstract IMailDataStrategy Create();

		public override void OnAdded(DefaultGenericImportStepConfigurator configurator)
		{
			base.OnAdded(configurator);
		}

		public override bool OnGoPrevious(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			defaultGenericImportStepConfigurator.Workflow.Steps.RemoveWhere(e => e.GroupKey == GroupKey);
			return base.OnGoPrevious(defaultGenericImportStepConfigurator);
		}
	}
}