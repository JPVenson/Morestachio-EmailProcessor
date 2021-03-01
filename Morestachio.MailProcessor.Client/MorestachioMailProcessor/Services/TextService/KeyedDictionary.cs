using System;
using System.Collections.ObjectModel;

namespace Morestachio.MailProcessor.Client.Services.TextService
{
	public class KeyedDictionary<T, TE> : KeyedCollection<T, TE>
	{
		private readonly Func<TE, T> _getKey;

		public KeyedDictionary(Func<TE, T> getKey)
		{
			_getKey = getKey;
		}

		protected override T GetKeyForItem(TE item)
		{
			return _getKey(item);
		}
	}
}