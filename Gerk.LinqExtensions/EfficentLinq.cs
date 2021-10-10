using System;
using System.Collections.Generic;
using System.Linq;

namespace Gerk.LinqExtensions
{
	/// <summary>
	/// Contains Extension methods specifically dealing with more efficent operations that LINQ already supports.
	/// </summary>
	public static class EfficentLinq
	{
		/// <summary>
		/// Creates an array from <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <typeparam name="T">Elements of the collection.</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="length">Number of elements in <paramref name="self"/>.</param>
		/// <returns>Array version of <paramref name="self"/></returns>
		public static T[] ToArray<T>(this IEnumerable<T> self, int length)
		{
			var output = new T[length];
			var enumerator = self.GetEnumerator();
			for (int i = 0; enumerator.MoveNext(); i++)
			{
				output[i] = enumerator.Current;
			}
			return output;
		}

		/// <summary>
		/// Creates a list from <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <typeparam name="T">Elements of the collection.</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="length">Number of elements in <paramref name="self"/>.</param>
		/// <returns>List version of <paramref name="self"/></returns>
		public static List<T> ToList<T>(this IEnumerable<T> self, int length)
		{
			var output = new List<T>(length);
			output.AddRange(self);
			return output;
		}
	}
}
