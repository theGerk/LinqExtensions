using Gerk.AsyncThen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gerk.LinqExtensions
{
	/// <summary>
	/// Contains Extension methods specifically dealing with asynchronous operations and IEnumerables
	/// </summary>
	public static class AsyncExtensions
	{
		#region SelectAsync
		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <returns></returns>
		public static Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, Task<Out>> func) => self.SelectAsync((x, _) => func(x), CancellationToken.None);

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, Task<Out>> func, int concurrencyLimit) => self.SelectAsync((x, _) => func(x), CancellationToken.None, concurrencyLimit);

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <param name="cancellationToken">Cancellation token that will be passed to each call of <paramref name="func"/></param>
		/// <returns></returns>
		public static async Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, CancellationToken, Task<Out>> func, CancellationToken cancellationToken) => await Task.WhenAll(self.Select(x => func(x, cancellationToken)));

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <param name="cancellationToken">Cancellation token that will be passed to each call of <paramref name="func"/></param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static async Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, CancellationToken, Task<Out>> func, CancellationToken cancellationToken, int concurrencyLimit)
		{
			var inputList = self.ToList();
			Out[] output = new Out[inputList.Count];
			var tasks = new HashSet<Task>();

			// Run the function on the idx'th element from the input and then assign it into the output. This is all ecapsulated within a task that is put into our pool of tasks.
			void startExecution(int idx) => tasks.Add(func(inputList[idx], cancellationToken).Then(o => output[idx] = o));

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
		#endregion

		#region ForEach
		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/></param>
		/// <returns></returns>
		public static Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action) => self.ForEachAsync((x, _) => action(x), CancellationToken.None);

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/></param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action, int concurrencyLimit) => self.ForEachAsync((x, _) => action(x), CancellationToken.None, concurrencyLimit);

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/></param>
		/// <param name="cancellationToken">Cancellation token that will be passed to each call of <paramref name="action"/></param>
		/// <returns></returns>
		public static async Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task> action, CancellationToken cancellationToken) => await Task.WhenAll(self.Select(x => action(x, cancellationToken)));

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/></param>
		/// <param name="cancellationToken">Cancellation token that will be passed to each call of <paramref name="action"/></param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task> action, CancellationToken cancellationToken, int concurrencyLimit)
		{
			var inputList = self.ToList();
			var tasks = new HashSet<Task>();

			int i = 0;

			for (; i < concurrencyLimit && i < inputList.Count; i++)
			{
				tasks.Add(action(inputList[i], cancellationToken));
			}

			for (; i < inputList.Count; i++)
			{
				tasks.Remove(await Task.WhenAny(tasks));
				tasks.Add(action(inputList[i], cancellationToken));
			}

			await Task.WhenAll(tasks);
		}
		#endregion

		#region FindMatch
		private static async Task<(In Value, bool Found)> FindMatchAsyncHelper<In>(Func<Func<In, Task>, Task> forEach, Func<In, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken)
		{
			using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			{
				In output = default;
				bool found = false;
				try
				{
					await forEach(async item =>
					{
						if (await predicate(item, cts.Token))
						{
							output = item;
							found = true;
							cts.Cancel();
						}
					});
				}
				catch (TaskCanceledException)
				{
					if (found)
						return (output, true);
					else
						throw;
				}
				return (output, found);
			}
		}

		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, Task<bool>> predicate)
		{
			var tcs = new TaskCompletionSource<(In, bool)>();
			var tasks = new List<Task>();
			foreach (var elem in self)
			{
				var e = elem;
				tasks.Add(predicate(elem).Then(x =>
				{
					if (x)
					{
						tcs.TrySetResult((e, true));
					}
				}));

				if (tcs.Task.IsCompleted)
					break;
			}

			Task.WhenAll(tasks).Then(() =>
			{
				tcs.TrySetResult((default, false));
			});

			return tcs.Task;
		}

		public static async Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, Task<bool>> predicate, int concurrencyLimit)
		{
			var tasks = new HashSet<Task<(In Value, bool Found)>>(concurrencyLimit);
			void startNextTask(In elem) => tasks.Add(predicate(elem).Then(x => (elem, x)));
			async Task<(In Value, bool Found)> completeATask()
			{
				var t = await Task.WhenAny(tasks);
				var response = await t;

				tasks.Remove(t);

				return response;
			}

			var enumerator = self.GetEnumerator();
			for (int i = 0; i < concurrencyLimit && enumerator.MoveNext(); i++)
				startNextTask(enumerator.Current);

			while (enumerator.MoveNext())
			{
				var response = await completeATask();
				if (response.Found)
					return response;

				startNextTask(enumerator.Current);
			}

			while (tasks.Count != 0)
			{
				var response = await completeATask();
				if (response.Found)
					return response;
			}

			return (default, false);
		}

		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate) => self.FindMatchAsync(predicate, CancellationToken.None);

		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate, int concurrencyLimit) => self.FindMatchAsync(predicate, CancellationToken.None, concurrencyLimit);

		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken) => FindMatchAsyncHelper(f => self.ForEachAsync(f), predicate, cancellationToken);

		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken, int concurrencyLimit) => FindMatchAsyncHelper(f => self.ForEachAsync(f, concurrencyLimit), predicate, cancellationToken);
		#endregion
	}
}
