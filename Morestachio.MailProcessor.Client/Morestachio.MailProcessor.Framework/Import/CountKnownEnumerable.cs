using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Morestachio.MailProcessor.Framework.Import
{
	public class CountKnownEnumerable<T> : IAsyncEnumerable<T>
	{
		public int Count { get; }
		private readonly IAsyncEnumerable<T> _enumerable;

		public CountKnownEnumerable(int count, IAsyncEnumerable<T> enumerable)
		{
			Count = count;
			_enumerable = enumerable;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
		{
			return _enumerable.GetAsyncEnumerator(cancellationToken);
		}
	}
}