using AsyncThen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gerk.LinqExtensions
{
	/// <summary>
	/// Contains Extension methods specifically dealing with asynchronous operations and IEnumerables
	/// </summary>
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
			Out[] output = new Out[inputList.Count];
			var tasks = new HashSet<Task>();

			// Run the function on the idx'th element from the inpu tand then assign it into the output. This is all ecapsulated within a task that is put into our pool of tasks.
			Action<int> startExecution = idx => tasks.Add(func(inputList[idx]).Then(o => output[idx] = o));

			int i = 0;

			for (; i < concurrencyLimit && i < inputList.Count; i++)
			{
				startExecution(i);
			}

			for (; i < inputList.Count; i++)
			{
				tasks.Remove(await Task.WhenAny(tasks));
				startExecution(i);
			}

			await Task.WhenAll(tasks);
			return output;
		}
	}
}
