using AsyncThen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gerk.LinqExtensions
{
	public static class AsyncExtensions
	{
		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel. A <c>0</c> will have no limit.</param>
		/// <returns></returns>
		public static async Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, Task<Out>> func, int concurrencyLimit = 0)
		{
			if (concurrencyLimit <= 0)
				return await Task.WhenAll(self.Select(func));

			var inputList = self.ToList();
			var enumerator = inputList.GetEnumerator();
			Out[] output = new Out[inputList.Count];
			Func<int, Action<Out>> callbackGenerator = i => o => output[i] = o;
			var tasks = new HashSet<Task>();

			Action<In, int> startExecution = (val, i) =>
			{
				tasks.Add(func(val).Then(o => output[i] = o));
			};

			int i = 0;

			for (; i < concurrencyLimit && enumerator.MoveNext(); i++)
			{
				startExecution(enumerator.Current, i);
			}

			while (enumerator.MoveNext())
			{
				tasks.Remove(await Task.WhenAny(tasks));
				startExecution(enumerator.Current, i++);
			}

			await Task.WhenAll(tasks);
			return output;
		}
	}
}
