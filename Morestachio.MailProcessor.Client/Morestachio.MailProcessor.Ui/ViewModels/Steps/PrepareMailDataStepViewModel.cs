using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
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
				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionAddress);
					e.ExampleAddress = (await ExpressionParser.EvaluateExpression(e.MExpressionAddress, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionAddress)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionName);
					e.ExampleName = (await ExpressionParser.EvaluateExpression(e.MExpressionName, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionName)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionSubject);
					e.ExampleSubject = (await ExpressionParser.EvaluateExpression(e.MExpressionSubject, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionSubject)));


				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionFromName);
					e.ExampleFromName = (await ExpressionParser.EvaluateExpression(e.MExpressionFromName, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionFromName)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionFromAddress);
					e.ExampleFromAddress = (await ExpressionParser.EvaluateExpression(e.MExpressionFromAddress, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionFromAddress)));
			}
		}

		public PrepareMailDataStepViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Prepare.Title");
			Description = new UiLocalizableString("MailDistributor.Prepare.Description");
			GroupKey = "MainGroup";
			MExpressionFromName = "\"Mr Company\"";
			MExpressionFromAddress = "\"mr.company@test.com\"";
			MExpressionSubject = "\"Hot new Newsletter\"";
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }

		public MailData ExampleMailData { get; set; }

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

			ExampleMailData = ExampleMailData ?? await mailComposer.MailDataStrategy.GetPreviewData();

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