using System.Collections;
using System.Collections.Generic;

namespace Gerk.LinqExtensions
{
	/// <summary>
	/// Allows for handling Enumerators as easily as we use Enumerable.
	/// </summary>
	public static class EnumeratorExtensions
	{
		/// <summary>
		/// Creates an Enumerable that uses the given enumerator behind it. Should be fairly efficent.
		/// </summary>
		/// <param name="enumerator">The enumerator to warp.</param>
		/// <returns>Enumerable wrapping <paramref name="enumerator"/></returns>
		public static IEnumerable AsEnumerable(this IEnumerator enumerator) => new EnumeratorEnumerable<IEnumerator>(enumerator);

		/// <summary>
		/// Creates an Enumerable that uses the given enumerator behind it. Should be fairly efficent.
		/// </summary>
		/// <typeparam name="T">Element type</typeparam>
		/// <param name="enumerator">The enumerator to warp.</param>
		/// <returns>Enumerable wrapping <paramref name="enumerator"/></returns>
		public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator) => new EnumeratorEnumerable<T, IEnumerator<T>>(enumerator);
	}
}
