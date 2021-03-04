using System;
using System.Globalization;
using Morestachio.MailProcessor.Ui.Services.TextService;

namespace Morestachio.MailProcessor.Ui.ViewModels.Localization
{
	public struct FormattableArgument
	{
		public FormattableArgument(string keyOrValue, bool isKey)
		{
			KeyOrValue = keyOrValue;
			IsKey = isKey;
		}

		public string KeyOrValue { get; private set; }
		public bool IsKey { get; private set; }

		public override string ToString()
		{
			if (IsKey)
			{
				return "Loc: " + KeyOrValue;
			}

			return KeyOrValue;
		}

		public object ToStringWith(ITextService textServiceValue)
		{
			if (IsKey)
			{
				return textServiceValue.Compile(KeyOrValue, CultureInfo.CurrentUICulture, out _);
			}

			return KeyOrValue;
		}

		//  User-defined conversion from double to Digit
		public static implicit operator String(FormattableArgument d)
		{
			return d.KeyOrValue;
		}

		//  User-defined conversion from double to Digit
		public static implicit operator FormattableArgument(string d)
		{
			return new FormattableArgument(d, false);
		}
	}
}