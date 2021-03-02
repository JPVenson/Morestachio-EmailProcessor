using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using Morestachio;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using Morestachio.MailProcessor.Client;
using Morestachio.MailProcessor.Client.Services.UiWorkflow;
using Morestachio.MailProcessor.Client.ViewModels;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Import;
using Morestachio.Parsing.ParserErrors;

namespace MorestachioMailProcessor.ViewModels.Steps
{
	public class PrepareMailDataStepViewModel : WizardStepBaseViewModel<PrepareMailDataStepViewModel.PrepareMailDataStepViewModelErrors>
	{
		public class PrepareMailDataStepViewModelErrors : ErrorCollection<PrepareMailDataStepViewModel>
		{
			public PrepareMailDataStepViewModelErrors()
			{
				Add(new AsyncError<PrepareMailDataStepViewModel>("DataImport.PrepareStep.Errors.InvalidExpression", async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionAddress);
					e.ExampleAddress = (await ExpressionParser.EvaluateExpression(e.MExpressionAddress, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionAddress)));

				Add(new AsyncError<PrepareMailDataStepViewModel>("DataImport.PrepareStep.Errors.InvalidExpression", async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionName);
					e.ExampleName = (await ExpressionParser.EvaluateExpression(e.MExpressionName, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionName)));

				Add(new AsyncError<PrepareMailDataStepViewModel>("DataImport.PrepareStep.Errors.InvalidExpression", async e =>
				{
					var context = TokenzierContext.FromText(e.MExpressionSubject);
					e.ExampleSubject = (await ExpressionParser.EvaluateExpression(e.MExpressionSubject, new ParserOptions(), e.ExampleMailData.Data, context)).ToString();
					return context.Errors.Any();
				}, nameof(MExpressionSubject)));
			}
		}

		public PrepareMailDataStepViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Prepare.Title");
			Description = new UiLocalizableString("MailDistributor.Prepare.Description");
			GroupKey = "MainGroup";

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
				
#if DEBUG
			MExpressionAddress = MExpressionAddress ?? "email";
			MExpressionName = MExpressionName ?? "\"Mr or Ms \" + firstname + \", \" + lastname";
			MExpressionSubject = MExpressionSubject ?? "\"Subject A\"";
#endif

			var mailComposer = IoC.Resolve<MailComposer>();
			MExpressionAddress = MExpressionAddress ?? StringifyExpression(mailComposer.AddressExpression);
			MExpressionName = MExpressionName ?? StringifyExpression(mailComposer.NameExpression);
			MExpressionSubject = MExpressionSubject ?? StringifyExpression(mailComposer.SubjectExpression);
			ExampleMailData = ExampleMailData ?? await mailComposer.MailDataStrategy.GetPreviewData();
		

			await base.OnEntry(data, configurator);
		}

		public override bool OnGoNext(DefaultGenericImportStepConfigurator defaultGenericImportStepConfigurator)
		{
			var mailComposer = IoC.Resolve<MailComposer>();
			mailComposer.AddressExpression = ExpressionParser.ParseExpression(MExpressionAddress, out _);
			mailComposer.NameExpression = ExpressionParser.ParseExpression(MExpressionName, out _);
			mailComposer.SubjectExpression = ExpressionParser.ParseExpression(MExpressionSubject, out _);
			return base.OnGoNext(defaultGenericImportStepConfigurator);
		}
	}
}