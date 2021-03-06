﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Morestachio.MailProcessor.Ui.Services.Settings;
using Morestachio.MailProcessor.Ui.Services.StructureCache;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.Util.ObjectSchema;
using Morestachio.MailProcessor.Ui.ViewModels.Localization;

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
					if (string.IsNullOrWhiteSpace(expressionValue) || value == null)
					{
						return null;
					}

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
					return (e.ExampleAddress = await ExportValue(e.MExpressionAddress, e.ExampleMailData)) == null;
				}, nameof(MExpressionAddress), nameof(ExampleMailData)));

				Add(new Error<PrepareMailDataStepViewModel>(invalidAddress, e =>
				{
					if (string.IsNullOrWhiteSpace(e.ExampleAddress))
					{
						return true;
					}

					return !mailRegEx.IsMatch(e.ExampleAddress);
				}, nameof(ExampleAddress), nameof(MExpressionAddress), nameof(ExampleMailData)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleName = await ExportValue(e.MExpressionName, e.ExampleMailData)) == null; ;
				}, nameof(MExpressionName), nameof(ExampleMailData)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleSubject = await ExportValue(e.MExpressionSubject, e.ExampleMailData)) == null;
				}, nameof(MExpressionSubject), nameof(ExampleMailData)));


				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleFromName = await ExportValue(e.MExpressionFromName, e.ExampleMailData)) == null;
				}, nameof(MExpressionFromName), nameof(ExampleMailData)));

				Add(new AsyncError<PrepareMailDataStepViewModel>(invalidExpression, async e =>
				{
					return (e.ExampleFromAddress = await ExportValue(e.MExpressionFromAddress, e.ExampleMailData)) == null;
				}, nameof(MExpressionFromAddress), nameof(ExampleMailData)));

				Add(new Error<PrepareMailDataStepViewModel>(invalidAddress, e =>
				{
					if (string.IsNullOrWhiteSpace(e.ExampleFromAddress))
					{
						return true;
					}
					return !mailRegEx.IsMatch(e.ExampleFromAddress);
				}, nameof(ExampleFromAddress), nameof(MExpressionFromAddress), nameof(ExampleMailData)));
			}
		}

		public PrepareMailDataStepViewModel()
		{
			Title = new UiLocalizableString("MailDistributor.Prepare.Title");
			Description = new UiLocalizableString("MailDistributor.Prepare.Description");
			GroupKey = "MainGroup";
			MailComposer = IoC.Resolve<MailComposer>();
			StructureCacheService = IoC.Resolve<StructureCacheService>();
			ExampleMailData = new MailData();
			//MExpressionFromName = "\"Mr Company\"";
			//MExpressionFromAddress = "\"mr.company@test.com\"";
			//MExpressionSubject = "\"Hot new Newsletter\"";
		}


		public MailComposer MailComposer { get; set; }
		public StructureCacheService StructureCacheService { get; set; }

		public MailData ExampleMailData
		{
			get { return _exampleMailData; }
			set { SetProperty(ref _exampleMailData, value); }
		}

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
		private MailData _exampleMailData;

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
				return null;
			}

			var visitor = new ToParsableStringExpressionVisitor();
			visitor.Visit(expression);
			return visitor.StringBuilder.ToString();
		}

		public override async Task<IDictionary<string, string>> SaveSetting()
		{
			await Task.CompletedTask;
			return new Dictionary<string, string>()
			{
				{nameof(MExpressionAddress), MExpressionAddress},
				{nameof(MExpressionName), MExpressionName},
				{nameof(MExpressionSubject), MExpressionSubject},
				{nameof(MExpressionFromAddress), MExpressionFromAddress},
				{nameof(MExpressionFromName), MExpressionFromName},
			};
		}

		public override async Task ReadSettings(IDictionary<string, string> settings)
		{
			using (base.DeferNotification())
			{
				MExpressionAddress = settings.GetOrNull(nameof(MExpressionAddress))?.ToString();
				MExpressionName = settings.GetOrNull(nameof(MExpressionName))?.ToString();
				MExpressionSubject = settings.GetOrNull(nameof(MExpressionSubject))?.ToString();
				MExpressionFromAddress = settings.GetOrNull(nameof(MExpressionFromAddress))?.ToString();
				MExpressionFromName = settings.GetOrNull(nameof(MExpressionFromName))?.ToString();
			}
			await base.ReadSettings(settings);
		}

		public override async Task OnEntry(IDictionary<string, object> data,
			DefaultStepConfigurator configurator)
		{
			ExampleMailData = StructureCacheService.ExampleMailData;

			using (base.DeferNotification())
			{
				MExpressionAddress = MExpressionAddress ?? StringifyExpression(MailComposer.ToAddressExpression) ?? GuessAddressExpression();
				MExpressionName = MExpressionName ?? StringifyExpression(MailComposer.ToNameExpression) ?? GuessNameExpression();
				MExpressionSubject = MExpressionSubject ?? StringifyExpression(MailComposer.SubjectExpression) ?? "\"subject\"";
				MExpressionFromAddress = MExpressionFromAddress ?? StringifyExpression(MailComposer.FromAddressExpression);
				MExpressionFromName = MExpressionFromName ?? StringifyExpression(MailComposer.FromNameExpression);
			}
			await base.OnEntry(data, configurator);
		}

		private string GetLike(string value)
		{
			foreach (var o in ExampleMailData.Data)
			{
				if (o.Key.Contains(value, StringComparison.InvariantCultureIgnoreCase) ||
					o.Key.Equals(value, StringComparison.InvariantCultureIgnoreCase))
				{
					return "Data." + o.Key;
				}
			}

			return null;
		}

		private string GuessAddressExpression()
		{
			return GetLike("mail")
				   ?? GetLike("webaddress") ?? GetLike("web-address") ?? GetLike("web_address") ?? GetLike("web address")
				   ?? GetLike("www")
				   ?? GetLike("web")
				   ?? GetLike("message")
				   ?? GetLike("e-dress");
		}

		private string GuessNameExpression()
		{
			return GetLike("firstname")
				   ?? GetLike("christian name") ?? GetLike("christian_name") ?? GetLike("christianname")
				   ?? GetLike("given name") ?? GetLike("given_name") ?? GetLike("givenname")
				   ?? GetLike("prename")
				   ?? GetLike("birth name") ?? GetLike("birth_name") ?? GetLike("birthname")
				   ?? GetLike("fore name") ?? GetLike("fore_name") ?? GetLike("forename")
				   ?? GetLike("proper name") ?? GetLike("proper_name") ?? GetLike("propername")
				   ?? GetLike("maiden name") ?? GetLike("maiden_name") ?? GetLike("maidenname")
				   ?? GetLike("salutation")
				   ?? GetLike("family name") ?? GetLike("family_name") ?? GetLike("familyname")
				   ?? GetLike("surname")
				   ?? GetLike("last name") ?? GetLike("last_name") ?? GetLike("lastname")
				   ?? GetLike("name");
		}

		public override Task<bool> OnGoNext(DefaultStepConfigurator defaultStepConfigurator)
		{
			var mailComposer = IoC.Resolve<MailComposer>();
			mailComposer.ToAddressExpression = ExpressionParser.ParseExpression(MExpressionAddress, out _);
			mailComposer.ToNameExpression = ExpressionParser.ParseExpression(MExpressionName, out _);
			mailComposer.SubjectExpression = ExpressionParser.ParseExpression(MExpressionSubject, out _);
			mailComposer.FromAddressExpression = ExpressionParser.ParseExpression(MExpressionFromAddress, out _);
			mailComposer.FromNameExpression = ExpressionParser.ParseExpression(MExpressionFromName, out _);
			ExampleMailData.MailInfo.ToAddress = ExampleAddress;
			ExampleMailData.MailInfo.ToName = ExampleName;

			ExampleMailData.MailInfo.FromAddress = ExampleFromAddress;
			ExampleMailData.MailInfo.FromName = ExampleFromName;
			ExampleMailData.MailInfo.Subject = ExampleSubject;

			return base.OnGoNext(defaultStepConfigurator);
		}
	}
}