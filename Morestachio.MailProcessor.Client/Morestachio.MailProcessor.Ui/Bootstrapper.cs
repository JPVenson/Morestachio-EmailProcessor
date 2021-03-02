using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Ui.Services.DataDistributor;
using Morestachio.MailProcessor.Ui.Services.DataImport;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.TextService.Translations;
using Unity;
using WPFLocalizeExtension.Engine;

namespace Morestachio.MailProcessor.Ui
{
	public class Bootstrapper
	{
		public void Start(App app)
		{
			InitIoCContainer();
			app.InitializeComponent();
			app.Run();
		}

		public void InitIoCContainer()
		{
			IoC.Init(new UnityContainer());
			IoC.RegisterInstance(new MailComposer());
			IoC.RegisterInstance(new DataImportService());
			IoC.RegisterInstance(new DataDistributorService());
			IoC.RegisterInstance<ITextService>(new TextService());

			foreach (var requireInit in IoC.ResolveMany<IRequireInit>())
			{
				requireInit.Init();
			}

			var textService = IoC.Resolve<ITextService>();
			textService.TranslationResources.Add(new ResxTranslationProvider(UiTranslations.ResourceManager));
			textService.LoadTexts().Wait();
			LocalizeDictionary.Instance.DefaultProvider = new TextServiceUsingLocalizationProvider(textService);
		}
	}
}
