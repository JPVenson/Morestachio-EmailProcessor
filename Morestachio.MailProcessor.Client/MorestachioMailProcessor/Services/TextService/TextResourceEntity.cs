using System.Globalization;

namespace MorestachioMailProcessor.Services.TextService
{
	public struct TextResourceEntity
	{
		public CultureInfo Lang { get; set; }
		public string Key { get; set; }
		public object Text { get; set; }
		public string Page { get; set; }
	}
}