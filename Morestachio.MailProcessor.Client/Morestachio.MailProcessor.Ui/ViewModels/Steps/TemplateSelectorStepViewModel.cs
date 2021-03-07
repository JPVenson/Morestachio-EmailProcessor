using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using ControlzEx.Theming;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using MahApps.Metro.Controls.Dialogs;
using Morestachio.Helper;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Behaviors.Avalon;
using Morestachio.MailProcessor.Ui.Resources.Steps;
using Morestachio.MailProcessor.Ui.Services.MailTemplates;
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.StructureCache;
using Morestachio.MailProcessor.Ui.Services.TextService;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.Services.WebView;
using Morestachio.MailProcessor.Ui.Util.ObjectSchema;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;
using Morestachio.Parsing.ParserErrors;
using Morestachio.TemplateContainers;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class TemplateSelectorStepViewModel : WizardStepBaseViewModel<TemplateSelectorStepViewModel.TemplateSelectorStepViewModelErrors>
	{
		public class TemplateSelectorStepViewModelErrors : ErrorCollection<TemplateSelectorStepViewModel>
		{
			public TemplateSelectorStepViewModelErrors()
			{
				Add(new AsyncError<TemplateSelectorStepViewModel>(
					new UiLocalizableString("Template.Errors.InvalidTemplate"),
					async e =>
					{
						if (string.IsNullOrWhiteSpace(e.Template))
						{
							return true;
						}

						await Task.Delay(250);
						e.SyncErrors(await Parser.Validate(new StringTemplateContainer(e.Template)));
						return e.MorestachioErrors.Count > 0;
					},
					nameof(Template)));
			}
		}

		public TemplateSelectorStepViewModel()
		{
			Title = new UiLocalizableString("Template.Title");
			Description = new UiLocalizableString("Template.Description");
			SetTemplateCommand = new DelegateCommand(SetTemplateExecute, CanSetTemplateExecute);
			MailTemplateService = IoC.Resolve<MailTemplateService>();
			WebViewService = IoC.Resolve<WebViewService>();
			StructureCacheService = IoC.Resolve<StructureCacheService>();

			GeneratePreviewCommand = new DelegateCommand(GeneratePreviewExecute, CanGeneratePreviewExecute);
			MorestachioErrors = new ThreadSaveObservableCollection<IMorestachioError>();
			ShowPreviewWindowCommand = new DelegateCommand(ShowPreviewWindowExecute, CanShowPreviewWindowExecute);
			PropertyChanged += TemplateSelectorStepViewModel_PropertyChanged;
			Commands.Add(new MenuBarCommand(ShowPreviewWindowCommand)
			{
				Content = new UiLocalizableString("Template.Preview.Title")
			});

			if (File.Exists("preview_error_template.mdoc.html"))
			{
				var errorTemplateOptions = new ParserOptions(File.ReadAllText("preview_error_template.mdoc.html"));
				ErrorDisplayTemplate = Parser.ParseWithOptions(errorTemplateOptions).Compile();
			}

			SetSyntax();
			ThemeManager.Current.ThemeChanged += Current_ThemeChanged;
		}

		private void Current_ThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			SetSyntax();
		}

		private void SetSyntax()
		{
			var syntaxDefinition = "";
			using (var manifestResourceStream = GetType().Assembly
				.GetManifestResourceStream("Morestachio.MailProcessor.Ui.MorestachioHightlight.xml"))
			{
				using (var reader = new StreamReader(manifestResourceStream, null, true, -1, true))
				{
					syntaxDefinition = reader.ReadToEnd();
				}
			}

			string themeName;
			if (ThemeManager.Current.DetectTheme().Name == "Dark.Blue")
			{
				themeName = "darkThemeColors";
			}
			else
			{
				themeName = "lightThemeColors";
			}

			var themeData = "";
			using (var manifestResourceStream = GetType().Assembly
				.GetManifestResourceStream($"Morestachio.MailProcessor.Ui.MorestachioHightlight-{themeName}.xml"))
			{
				using (var reader = new StreamReader(manifestResourceStream, null, true, -1, true))
				{
					themeData = reader.ReadToEnd();
				}
			}

			var endOfRoot = themeData.IndexOf(">") + 1;
			themeData = themeData.Substring(endOfRoot, themeData.LastIndexOf("<") - endOfRoot);

			syntaxDefinition = syntaxDefinition.Replace("<ThemeData/>", themeData);
			var xshdSyntaxDefinition = HighlightingLoader.Load(new XmlTextReader(new StringReader(syntaxDefinition)), null);

			//HighlightingManager.Instance.RegisterHighlighting("Morestachio", new string[0], xshdSyntaxDefinition);
			MorestachioHtmlMixDefinition = xshdSyntaxDefinition;
		}

		private void TemplateSelectorStepViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Template) && PreviewTemplateWindow != null)
			{
				GeneratePreviewExecute(sender);
			}
		}

		private string _template;
		private string _preview;
		private MailTemplate _selectedTemplate;
		private IHighlightingDefinition _morestachioHtmlMixDefinition;
		private ITextMarkerService _textMarkerService;
		

		public ITextMarkerService TextMarkerService
		{
			get { return _textMarkerService; }
			set { SetProperty(ref _textMarkerService, value); }
		}

		public IHighlightingDefinition MorestachioHtmlMixDefinition
		{
			get { return _morestachioHtmlMixDefinition; }
			set { SetProperty(ref _morestachioHtmlMixDefinition, value); }
		}

		public MailTemplate SelectedTemplate
		{
			get { return _selectedTemplate; }
			set { SetProperty(ref _selectedTemplate, value); }
		}

		public string Preview
		{
			get { return _preview; }
			set { SetProperty(ref _preview, value); }
		}

		public string Template
		{
			get { return _template; }
			set { SetProperty(ref _template, value); }
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
		public MailData ExampleMailData { get; set; }
		public DelegateCommand SetTemplateCommand { get; private set; }
		public DelegateCommand GeneratePreviewCommand { get; private set; }
		public DelegateCommand ShowPreviewWindowCommand { get; private set; }

		public ThreadSaveObservableCollection<IMorestachioError> MorestachioErrors { get; set; }
		public PreviewTemplateWindow PreviewTemplateWindow { get; set; }
		public bool PreviewGenerationRequested { get; set; }

		public WebViewService WebViewService { get; set; }
		public StructureCacheService StructureCacheService { get; set; }
		public MailTemplateService MailTemplateService { get; set; }

		public CompilationResult ErrorDisplayTemplate { get; set; }

		private void RefreshErrorMarkers()
		{
			if (TextMarkerService == null)
			{
				return;
			}

			ViewModelAction(() =>
			{
				TextMarkerService.RemoveAll(f => true);
				foreach (var morestachioError in MorestachioErrors)
				{
					var textMarker = TextMarkerService.Create(
						morestachioError.Location.Line,
						morestachioError.Location.Character,
						morestachioError.Location.Snipped.Snipped.Length);
					textMarker.MarkerColor = Colors.Red;
					textMarker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
					textMarker.ToolTip = morestachioError;
				}

				//if (e.InferredTemplateModel != null)
				//{
				//	MorestachioFoldingStrategy.UpdateFolding(FoldingManager, Template, e.InferredTemplateModel);
				//}
			});
		}

		private void ShowPreviewWindowExecute(object sender)
		{
			if (PreviewTemplateWindow == null)
			{
				PreviewTemplateWindow = new PreviewTemplateWindow()
				{
					DataContext = this
				};
				GeneratePreviewExecute(sender);
				PreviewTemplateWindow.Show();
				PreviewTemplateWindow.Closed += (o, args) => PreviewTemplateWindow = null;
				App.Current.MainWindow.Closing += (o, args) => PreviewTemplateWindow?.Close();
			}
			else
			{
				PreviewTemplateWindow.BringIntoView();
			}
		}

		private bool CanShowPreviewWindowExecute(object sender)
		{
			return IsNotWorking;
		}

		private void GeneratePreviewExecute(object sender)
		{
			if (PreviewGenerationRequested)
			{
				return;
			}

			PreviewGenerationRequested = true;
			SimpleWorkAsync(async () =>
			{
				await Task.Delay(250);
				await LoopPreview();
			}, null, false);
		}

		private async Task LoopPreview()
		{
			string preview = null;
			MorestachioDocumentInfo morestachioDocumentInfo;
			do
			{
				PreviewGenerationRequested = false;
				var parser = new ParserOptions(Template);
				morestachioDocumentInfo = await Parser.ParseWithOptionsAsync(parser);
				if (morestachioDocumentInfo.Errors.Any())
				{
					continue;
				}

				preview = await morestachioDocumentInfo.CreateAndStringifyAsync(IoC.Resolve<StructureCacheService>()
					.ExampleMailData);

			} while (PreviewGenerationRequested);

			if (morestachioDocumentInfo.Errors.Any())
			{
				SyncErrors(morestachioDocumentInfo.Errors);

				if (ErrorDisplayTemplate != null)
				{
					Preview = (await ErrorDisplayTemplate(MorestachioErrors, CancellationToken.None)).Stream.Stringify(false, Encoding.UTF8);
				}
			}
			else
			{
				Preview = preview;
			}
		}

		private void SyncErrors(IEnumerable<IMorestachioError> errors)
		{
			foreach (var morestachioError in errors)
			{
				if (MorestachioErrors.Any(e =>
					ErrorComparer.Instance.Equals(e, morestachioError)))
				{
					continue;
				}

				MorestachioErrors.Add(morestachioError);
			}

			var obsoleteErrors = MorestachioErrors.Except(errors, ErrorComparer.Instance).ToArray();
			foreach (var morestachioError in obsoleteErrors)
			{
				MorestachioErrors.Remove(morestachioError);
			}

			if (MorestachioErrors.Any())
			{
				RefreshErrorMarkers();
			}
			else
			{
				TextMarkerService.RemoveAll(f => true);
			}
		}

		private bool CanGeneratePreviewExecute(object sender)
		{
			return IsNotWorking && !HasErrors;
		}


		private void SetTemplateExecute(object sender)
		{
			SimpleWorkAsync(async () =>
			{
				var uiWorkflow = IoC.Resolve<IUiWorkflow>();
				var textService = IoC.Resolve<ITextService>();
				var loadingScreen = await DialogCoordinator.Instance.ShowProgressAsync(uiWorkflow,
					textService.Compile("Summery.Progress.ObtainTemplate.Title", CultureInfo.CurrentUICulture, out _).ToString(),
					textService.Compile("Summery.Progress.ObtainTemplate.Message", CultureInfo.CurrentUICulture, out _).ToString()
				);
				loadingScreen.SetIndeterminate();

				string newTemplate;
				try
				{
					newTemplate = (await MailTemplateService.ObtainTemplate(SelectedTemplate));
				}
				catch (Exception e)
				{
					await loadingScreen.CloseAsync();
					await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
						textService.Compile("Application.Error.Title", CultureInfo.CurrentUICulture, out _).ToString(),
						textService.Compile("Summery.Error.Message", CultureInfo.CurrentUICulture, out _,
							new FormattableArgument(SelectedTemplate.Header, false),
							new FormattableArgument(e.Message, false)
							).ToString()
					);

					return;
				}

				await loadingScreen.CloseAsync();
				if (newTemplate != null)
				{
					if (((string.IsNullOrWhiteSpace(Template) == false && Template != newTemplate)
						 &&
						 (await DialogCoordinator.Instance.ShowMessageAsync(uiWorkflow,
							 textService.Compile("Application.ConfirmQuestion.Title", CultureInfo.CurrentUICulture,
								 out _).ToString(),
							 textService.Compile("Summery.Template.Confirm.Message", CultureInfo.CurrentUICulture,
								 out _).ToString(),
							 MessageDialogStyle.AffirmativeAndNegative
						 )) == MessageDialogResult.Negative))
					{
						return;
					}
					Template = newTemplate;
				}
			});
		}

		private bool CanSetTemplateExecute(object sender)
		{
			return IsNotWorking && SelectedTemplate != null;
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(Template), Template}
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			Template = settings.GetOrNull(nameof(Template))?.ToString();
			await base.ReadSettings(settings);
		}

		public override Task OnEntry(IDictionary<string, object> data, DefaultStepConfigurator configurator)
		{
			ExampleMailData = StructureCacheService.ExampleMailData;
			return base.OnEntry(data, configurator);
		}

		public override bool OnGoPrevious(DefaultStepConfigurator defaultStepConfigurator)
		{
			PreviewTemplateWindow?.Close();
			return base.OnGoPrevious(defaultStepConfigurator);
		}

		public override Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			PreviewTemplateWindow?.Close();
			IoC.Resolve<MailComposer>().Template = Template;
			return base.OnGoNext(defaultStepConfigurator);
		}
	}

	internal class ErrorComparer : IEqualityComparer<IMorestachioError>
	{
		static ErrorComparer()
		{
			Instance = new ErrorComparer();
		}

		public static ErrorComparer Instance { get; private set; }

		public bool Equals(IMorestachioError x, IMorestachioError y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (ReferenceEquals(x, null))
			{
				return false;
			}

			if (ReferenceEquals(y, null))
			{
				return false;
			}

			if (x.GetType() != y.GetType())
			{
				return false;
			}

			return x.Location.Equals(y.Location) && x.HelpText == y.HelpText;
		}

		public int GetHashCode(IMorestachioError obj)
		{
			return HashCode.Combine(obj.Location, obj.HelpText);
		}
	}
}
