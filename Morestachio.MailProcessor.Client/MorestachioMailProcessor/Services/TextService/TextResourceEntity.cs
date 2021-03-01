﻿using System.Globalization;

namespace Morestachio.MailProcessor.Client.Services.TextService
{
	public struct TextResourceEntity
	{
		public CultureInfo Lang { get; set; }
		public string Key { get; set; }
		public object Text { get; set; }
		public string Page { get; set; }
	}
}