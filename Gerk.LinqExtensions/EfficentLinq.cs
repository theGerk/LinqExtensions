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


#if NET47_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER //ValueTuples
		/// <summary>
		/// Gets the first element if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <returns>
		///		<para>Value: The value of the first element, default if it does not exist.</para>
		///		<para>Exists: Was a value found?</para>
		/// </returns>
		public static (T Value, bool Exists) FirstIfExists<T>(this IEnumerable<T> self)
		{
			foreach (var item in self)
				return (item, true);
			return (default, false);
		}

		/// <summary>
		/// Gets the first element for which <paramref name="predicate"/> returns <see langword="true"/> if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="predicate">A function to limit which elements of <paramref name="self"/> count.</param>
		/// <returns>
		///		<para>Value: The value of the first element, default if it does not exist.</para>
		///		<para>Exists: Was a value found?</para>
		/// </returns>
		public static (T Value, bool Exists) FirstIfExists<T>(this IEnumerable<T> self, Predicate<T> predicate)
		{
			foreach (var item in self)
				if (predicate(item))
					return (item, true);
			return (default, false);
		}

		/// <summary>
		/// Gets the last element if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <returns>
		///		<para>Value: The value of the last element, default if it does not exist.</para>
		///		<para>Exists: Was a value found?</para>
		/// </returns>
		public static (T Value, bool Exists) LastIfExists<T>(this IEnumerable<T> self)
		{
			T value = default;
			bool found = false;
			foreach (var item in self)
				(value, found) = (item, true);
			return (value, found);
		}

		/// <summary>
		/// Gets the last element for which <paramref name="predicate"/> returns <see langword="true"/> if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="predicate">A function to limit which elements of <paramref name="self"/> count.</param>
		/// <returns>
		///		<para>Value: The value of the last element, default if it does not exist.</para>
		///		<para>Exists: Was a value found?</para>
		/// </returns>
		public static (T Value, bool Exists) LastIfExists<T>(this IEnumerable<T> self, Predicate<T> predicate)
		{
			T value = default;
			bool found = false;
			foreach (var item in self)
				if (predicate(item))
					(value, found) = (item, true);
			return (value, found);
		}

		/// <summary>
		/// Gets the first element if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="value">Out var to capture the first element if it exists.</param>
		/// <returns>
		/// Is there is a fist value?
		/// </returns>
		public static bool TryFirst<T>(this IEnumerable<T> self, out T value)
		{
			foreach (var item in self)
			{
				value = item;
				return true;
			}
			value = default;
			return false;
		}
#endif

		/// <summary>
		/// Gets the first element for which <paramref name="predicate"/> returns <see langword="true"/> if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="predicate">A function to limit which elements of <paramref name="self"/> count.</param>
		/// <param name="value">Out var to capture the first element meeting the <paramref name="predicate"/> if it exists.</param>
		/// <returns>
		/// Is there is a first value that meets the predicate?
		/// </returns>
		public static bool TryFirst<T>(this IEnumerable<T> self, Predicate<T> predicate, out T value)
		{
			foreach (var item in self)
			{
				if (predicate(item))
				{
					value = item;
					return true;
				}
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Gets the last element if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="value">Out var to capture the last element if it exists.</param>
		/// <returns>
		/// Is there is a last value?
		/// </returns>
		public static bool TryLast<T>(this IEnumerable<T> self, out T value)
		{
			value = default;
			bool found = false;
			foreach (var item in self)
			{
				value = item;
				found = true;
			}
			return found;
		}

		/// <summary>
		/// Gets the last element if there is one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self"></param>
		/// <param name="predicate">A function to limit which elements of <paramref name="self"/> count.</param>
		/// <param name="value">Out var to capture the last element meeting the <paramref name="predicate"/> if it exists.</param>
		/// <returns>
		/// Is there is a last value?
		/// </returns>
		public static bool TryLast<T>(this IEnumerable<T> self, Predicate<T> predicate, out T value)
		{
			value = default;
			bool found = false;
			foreach (var item in self)
			{
				if (predicate(item))
				{
					value = item;
					found = true;
				}
			}
			return found;
		}
	}
}
