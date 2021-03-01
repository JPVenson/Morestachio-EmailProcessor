using System.Collections;
using System.Collections.Generic;

namespace Morestachio.MailProcessor.Framework.Import
{
	public class CountKnownEnumerable<T> : IEnumerable<T>
	{
		public int Count { get; }
		private readonly IEnumerable<T> _enumerable;

		public CountKnownEnumerable(int count, IEnumerable<T> enumerable)
		{
			Count = count;
			_enumerable = enumerable;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _enumerable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _enumerable).GetEnumerator();
		}
	}
}