using System;
using System.Collections;
using System.Collections.Generic;

namespace Gerk.LinqExtensions
{
	internal class EnumeratorEnumerable<EnumeratorType> : IEnumerable where EnumeratorType : IEnumerator
	{
		/// <summary>
		/// The enumerator itself
		/// </summary>
		protected EnumeratorType self;

		public EnumeratorEnumerable(EnumeratorType enumerator) => self = enumerator;

		/// <inheritdoc/>
		public IEnumerator GetEnumerator() => self;

	}

	internal class EnumeratorEnumerable<T, EnumeratorType> : EnumeratorEnumerable<EnumeratorType>, IEnumerable<T>, IDisposable where EnumeratorType : IEnumerator<T>
	{
		public EnumeratorEnumerable(EnumeratorType enumerator) : base(enumerator) { }

		/// <inheritdoc/>
		public void Dispose() => self.Dispose();

		/// <inheritdoc/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => self;
	}
}
