using System;
using System.Linq;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.DelegateCommand;
using Morestachio.MailProcessor.Framework;
using Morestachio.MailProcessor.Framework.Sender;
using Morestachio.MailProcessor.Framework.Sender.Strategies;
using Morestachio.MailProcessor.Ui.Services.UiWorkflow;
using Morestachio.MailProcessor.Ui.ViewModels;

namespace Morestachio.MailProcessor.Ui.Services.DataDistributor.Strategies
{
	public class SmtpMailDistributorViewModel : MailDistributorBaseViewModel<SmtpMailDistributorViewModel.SmtpMailDistributorViewModelErrors>
	{
		public class SmtpMailDistributorViewModelErrors : ErrorCollection<SmtpMailDistributorViewModel>
		{
			public SmtpMailDistributorViewModelErrors()
			{
				Add(new Error<SmtpMailDistributorViewModel>(
					new UiLocalizableString("MailDistributor.Strategy.Smtp.Errors.NoPassword"),
					e => string.IsNullOrWhiteSpace(e.AuthPassword),
					nameof(AuthPassword)));
				Add(new Error<SmtpMailDistributorViewModel>(
					new UiLocalizableString("MailDistributor.Strategy.Smtp.Errors.NoUsername"),
					e => string.IsNullOrWhiteSpace(e.AuthUserName),
					nameof(AuthUserName)));
				Add(new Error<SmtpMailDistributorViewModel>(
					new UiLocalizableString("MailDistributor.Strategy.Smtp.Errors.NoHost"),
					e => string.IsNullOrWhiteSpace(e.Host),
					nameof(Host)));
				Add(new Error<SmtpMailDistributorViewModel>(
					new UiLocalizableString("MailDistributor.Strategy.Smtp.Errors.InvalidHost"),
					e => Uri.TryCreate(e.Host, UriKind.Absolute, out _),
					nameof(Host)));
				Add(new Error<SmtpMailDistributorViewModel>(
					new UiLocalizableString("MailDistributor.Strategy.Smtp.Errors.InvalidPort"),
					e => e.HostPort < 0 || e.HostPort > 65556,
					nameof(HostPort)));
			}
		}

		public SmtpMailDistributorViewModel() : base(
			nameof(AuthPassword),
			nameof(AuthUserName),
			nameof(Host),
			nameof(HostPort))
		{
			Title = new UiLocalizableString("MailDistributor.Strategy.Smtp.Title");
			Description = new UiLocalizableString("MailDistributor.Strategy.Smtp.Description");
			IdKey = SmtpMailDistributor.IdKey;
			Name = new UiLocalizableString("MailDistributor.Strategy.Smtp.Name");

			HostPort = 587;
			base.ForceRefreshAsync();
		}
		private string _host;
		private int _hostPort;
		private string _authUserName;
		private string _authPassword;

		public string AuthPassword
		{
			get { return _authPassword; }
			set { SetProperty(ref _authPassword, value); }
		}

		public string AuthUserName
		{
			get { return _authUserName; }
			set { SetProperty(ref _authUserName, value); }
		}

		public int HostPort
		{
			get { return _hostPort; }
			set { SetProperty(ref _hostPort, value); }
		}

		public string Host
		{
			get { return _host; }
			set { SetProperty(ref _host, value); }
		}

		public override UiLocalizableString Title { get; }
		public override UiLocalizableString Description { get; }
		public override IMailDistributor Create()
		{
			return new SmtpMailDistributor()
			{
				AuthName = AuthUserName,
				AuthPassword = AuthPassword,
				Host = Host,
				HostPort = HostPort
			};
		}
	}
}
