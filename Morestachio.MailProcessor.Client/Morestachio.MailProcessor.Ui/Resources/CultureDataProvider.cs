using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morestachio.MailProcessor.Ui.Resources
{
	public static class CultureDataProvider
	{
		static CultureDataProvider()
		{
			//flags can be used with
			//https://github.com/point-platform/famfamfam-flags-wpf
			KnownCultures = CultureInfo
				.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures)
				.Where(culture => (culture.LCID != 127 && (culture.CultureTypes & CultureTypes.NeutralCultures) != CultureTypes.NeutralCultures))
				.Distinct(e => e.LCID)
				.Select(culture =>
				{
					try
					{
						return new RegionInfo(culture.LCID);
					}
					catch
					{
						return null;
					}
				})
				.Where(ri => ri != null)
				.ToArray();
			Countries = KnownCultures.GroupBy(f => f.ThreeLetterISORegionName)
				.Select(e => e.First())
				.OrderBy(e => e.EnglishName)
				.ToArray();
		}
		
		public static IEnumerable<RegionInfo> KnownCultures { get; private set; }
		public static IEnumerable<RegionInfo> Countries { get; private set; }

		private static IEnumerable<T> Distinct<T, TE>(this IEnumerable<T> source, Func<T, TE> condition)
		{
			var keys = new HashSet<TE>();
			foreach (var item in source)
			{
				var value = condition(item);
				if (keys.Contains(value))
				{
					continue;
				}
				keys.Add(value);
				yield return item;
			}
		}
	}
}
