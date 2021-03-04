using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.ViewModel;
using MimeKit;
using MimeKit.Cryptography;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.MailProcessor.Ui.Services.StructureCache;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;

namespace Morestachio.MailProcessor.Ui.ViewModels.Steps
{
	public class PrepareMailDataStepViewModel : WizardStepBaseViewModel<PrepareMailDataStepViewModel.PrepareMailDataStepViewModelErrors>
	{
		public class PrepareMailDataStepViewModelErrors : ErrorCollection<PrepareMailDataStepViewModel>
		{
			public PrepareMailDataStepViewModelErrors()
			{
				var invalidExpression = new UiLocalizableString("DataImport.PrepareStep.Errors.InvalidExpression");
				var invalidAddress = new UiLocalizableString("DataImport.PrepareStep.Errors.InvalidAddress");

				//https://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx/
				var mailRegEx = new Regex(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
				                          + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
				                          + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$",
					RegexOptions.IgnoreCase);
				var defaultOptions = new ParserOptions()
				{
					Timeout = TimeSpan.FromSeconds(3)
				};

				async ValueTask<string> ExportValue(string expressionValue, object value)
				{
					var context = TokenzierContext.FromText(expressionValue);
					var expression = ExpressionParser.ParseExpression(expressionValue, context);
					if (expression == null)
					{
						return null;
					}

					return (await expression.GetValue(new ContextObject(defaultOptions, "", null, value),
							new ScopeData()))
						.Value?.ToString();
				}
				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleAddress = await ExportValue(e.MExpressionAddress, e.ExampleMailData.Data)) == null;
				}, nameof(MExpressionAddress)));
				Add(new Error<PrepareMailDataStepViewModel>(invalidAddress, e =>
				{
					return !mailRegEx.IsMatch(e.ExampleAddress);
				}, nameof(ExampleAddress), nameof(MExpressionAddress)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleName = await ExportValue(e.MExpressionName, e.ExampleMailData.Data)) == null;;
				}, nameof(MExpressionName)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleSubject = await ExportValue(e.MExpressionSubject, e.ExampleMailData.Data)) == null;
				}, nameof(MExpressionSubject)));


				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleFromName = await ExportValue(e.MExpressionFromName, e.ExampleMailData.Data)) == null;
				}, nameof(MExpressionFromName)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleFromAddress = await ExportValue(e.MExpressionFromAddress, e.ExampleMailData.Data)) == null;
				}, nameof(MExpressionFromAddress)));
				Add(new Error<PrepareMailDataStepViewModel>(invalidAddress, e =>
				{
					return !mailRegEx.IsMatch(e.ExampleFromAddress);
				}, nameof(ExampleFromAddress), nameof(MExpressionFromAddress)));
			}
		}

		public PrepareMailDataStepViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Prepare.Title");
			Description = new UiLocalizableString("MailDistributor.Prepare.Description");
			GroupKey = "MainGroup";
			Structure = new ThreadSaveObservableCollection<MailDataStructureViewModel>();
			//MExpressionFromName = "\"Mr Company\"";
			//MExpressionFromAddress = "\"mr.company@test.com\"";
			//MExpressionSubject = "\"Hot new Newsletter\"";
		}


		public ThreadSaveObservableCollection<MailDataStructureViewModel> Structure { get; set; }
		public MailData ExampleMailData { get; set; }

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		private string _mExpressionAddress;
		private string _mExpressionName;
		private string _exampleAddress;
		private string _exampleName;
		private string _exampleSubject;
		private string _mExpressionSubject;
		private string _mExpressionFromAddress;
		private string _mExpressionFromName;
		private string _exampleFromAddress;
		private string _exampleFromName;

		public string ExampleFromName
		{
			get { return _exampleFromName; }
			set { SetProperty(ref _exampleFromName, value); }
		}

		public string ExampleFromAddress
		{
			get { return _exampleFromAddress; }
			set { SetProperty(ref _exampleFromAddress, value); }
		}

		public string MExpressionFromName
		{
			get { return _mExpressionFromName; }
			set { SetProperty(ref _mExpressionFromName, value); }
		}

		public string MExpressionFromAddress
		{
			get { return _mExpressionFromAddress; }
			set { SetProperty(ref _mExpressionFromAddress, value); }
		}

		public string ExampleSubject
		{
			get { return _exampleSubject; }
			set { SetProperty(ref _exampleSubject, value); }
		}

		public string ExampleName
		{
			get { return _exampleName; }
			set { SetProperty(ref _exampleName, value); }
		}

		public string ExampleAddress
		{
			get { return _exampleAddress; }
			set { SetProperty(ref _exampleAddress, value); }
		}

		public string MExpressionSubject
		{
			get { return _mExpressionSubject; }
			set { SetProperty(ref _mExpressionSubject, value); }
		}

		public string MExpressionName
		{
			get { return _mExpressionName; }
			set { SetProperty(ref _mExpressionName, value); }
		}

		public string MExpressionAddress
		{
			get { return _mExpressionAddress; }
			set { SetProperty(ref _mExpressionAddress, value); }
		}

		private static string StringifyExpression(IMorestachioExpression expression)
		{
			if (expression == null)
			{
				return "";
			}

			var visitor = new ToParsableStringExpressionVisitor();
			visitor.Visit(expression);
			return visitor.StringBuilder.ToString();
		}

		public override async Task OnEntry(IDictionary<string, object> data,
			DefaultGenericImportStepConfigurator configurator)
		{
				
//#if DEBUG
//			MExpressionAddress = MExpressionAddress ?? "email";
//			MExpressionName = MExpressionName ?? "\"Mr or Ms \" + firstname + \", \" + lastname";
//			MExpressionSubject = MExpressionSubject ?? "\"Subject A\"";
//#endif

			var mailComposer = IoC.Resolve<MailComposer>();
			MExpressionAddress = MExpressionAddress ?? StringifyExpression(mailComposer.ToAddressExpression);
			MExpressionName = MExpressionName ?? StringifyExpression(mailComposer.ToNameExpression);
			MExpressionSubject = MExpressionSubject ?? StringifyExpression(mailComposer.SubjectExpression);
			MExpressionFromAddress = MExpressionFromAddress ?? StringifyExpression(mailComposer.FromAddressExpression);
			MExpressionFromName = MExpressionFromName ?? StringifyExpression(mailComposer.FromNameExpression);

			ExampleMailData = await mailComposer.MailDataStrategy.GetPreviewData();
			IoC.Resolve<StructureCacheService>().ExampleMailData = ExampleMailData;
			
			Structure.Clear();
			Structure.AddEach(MailDataStructureViewModel.GenerateStructure(ExampleMailData.Data));

			await base.OnEntry(data, configurator);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			var mailComposer = IoC.Resolve<MailComposer>();
			mailComposer.ToAddressExpression = ExpressionParser.ParseExpression(MExpressionAddress, out _);
			mailComposer.ToNameExpression = ExpressionParser.ParseExpression(MExpressionName, out _);
			mailComposer.SubjectExpression = ExpressionParser.ParseExpression(MExpressionSubject, out _);
			mailComposer.FromAddressExpression = ExpressionParser.ParseExpression(MExpressionFromAddress, out _);
			mailComposer.FromNameExpression = ExpressionParser.ParseExpression(MExpressionFromName, out _);
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}