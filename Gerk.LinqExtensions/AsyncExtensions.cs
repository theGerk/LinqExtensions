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
		#region Other
		// Maybe in the future this can be removed and replaced with a normal new HashSet<T>(capacity)
		private static HashSet<T> MakeHashSetWithCapacity<T>(int capacity)
			=> new HashSet<T>(
#if NETSTANDARD2_1_OR_GREATER
				capacity
#endif
			);
		#endregion

		#region SelectAsync
		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <returns></returns>
		public static Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, Task<Out>> func) => Task.WhenAll(self.Select(x => func(x)));

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Ordering is maintained from input to output, but internal execution order is not gaurenteed to match this. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>)</typeparam>
		/// <typeparam name="Out">The output type</typeparam>
		/// <param name="self">List that we are starting with.</param>
		/// <param name="func"><see langword="async"/> function that maps from elements of <paramref name="self"/> to an output type.</param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static async Task<Out[]> SelectAsync<In, Out>(this IEnumerable<In> self, Func<In, Task<Out>> func, int concurrencyLimit)
		{
			var inputList = self.ToList();
			Out[] output = new Out[inputList.Count];
			var tasks = MakeHashSetWithCapacity<Task>(concurrencyLimit);

			// Run the function on the idx'th element from the input and then assign it into the output. This is all ecapsulated within a task that is put into our pool of tasks.
			void startExecution(int idx) => tasks.Add(func(inputList[idx]).Then(o => output[idx] = o));

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
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/>.</param>
		/// <returns></returns>
		public static Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action) => Task.WhenAll(self.Select(x => action(x)));

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/>.</param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action, int concurrencyLimit)
		{
			var inputEnumerator = self.GetEnumerator();
			var tasks = MakeHashSetWithCapacity<Task>(concurrencyLimit);

			int i = 0;

			for (; i < concurrencyLimit && inputEnumerator.MoveNext(); i++)
			{
				tasks.Add(action(inputEnumerator.Current));
			}

			for (; inputEnumerator.MoveNext(); i++)
			{
				tasks.Remove(await Task.WhenAny(tasks));
				tasks.Add(action(inputEnumerator.Current));
			}

			await Task.WhenAll(tasks);
		}
		#endregion

		#region FindMatch
		/// <summary>
		/// Helper function for for FindMatchAsync overloads that accept a <see cref="CancellationToken"/>.
		/// </summary>
		/// <typeparam name="In">Underlying elments type that we act on.</typeparam>
		/// <param name="forEach">This is a function that will apply a function to each element in a collection with whatever concurrency limiting or what that collection is already built in.</param>
		/// <param name="predicate">An asynchronous predicate. Takes in an element of type <typeparamref name="In"/> and returns a <see cref="bool"/> and can be cancled using its second argument of a <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">A cancelation token.</param>
		/// <returns>Task with the result.</returns>
		private static async Task<(In Value, bool Found)> FindMatchAsyncHelper<In>(Func<Func<In, Task>, Task> forEach, Func<In, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default)
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

		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="predicate"><see langword="async"/> predicate function that will be applyed to elements of <paramref name="self"/>.</param>
		/// <returns>
		///		<list type="bullet">
		///			<item>
		///				<term>Value</term>
		///				<description>The element that was matched (if there was a match).</description>
		///			</item>	
		///			<item>
		///				<term>Found</term>
		///				<description>Whether or not there is a match at all.</description>
		///			</item>
		///		</list>
		/// </returns>
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

		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="predicate"><see langword="async"/> predicate function that will be applyed to elements of <paramref name="self"/>.</param>
		/// <param name="concurrencyLimit">Number of instances of <paramref name="predicate"/></param>
		/// <returns>
		///		<list type="bullet">
		///			<item>
		///				<term>Value</term>
		///				<description>The element that was matched (if there was a match).</description>
		///			</item>	
		///			<item>
		///				<term>Found</term>
		///				<description>Whether or not there is a match at all.</description>
		///			</item>
		///		</list>
		/// </returns>
		public static async Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, Task<bool>> predicate, int concurrencyLimit)
		{
			var tasks = MakeHashSetWithCapacity<Task<(In Value, bool Found)>>(concurrencyLimit);

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

		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="predicate"><see langword="async"/> predicate function that will be applyed to elements of <paramref name="self"/>.</param>
		/// <param name="cancellationToken">Cancelation token to cancel the entire process.</param>
		/// <returns>
		///		<list type="bullet">
		///			<item>
		///				<term>Value</term>
		///				<description>The element that was matched (if there was a match).</description>
		///			</item>	
		///			<item>
		///				<term>Found</term>
		///				<description>Whether or not there is a match at all.</description>
		///			</item>
		///		</list>
		/// </returns>
		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate, CancellationToken cancellationToken = default) => FindMatchAsyncHelper(f => self.ForEachAsync(f), predicate, cancellationToken);

		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="predicate"><see langword="async"/> predicate function that will be applyed to elements of <paramref name="self"/>.</param>
		/// <param name="cancellationToken">Cancelation token to cancel the entire process.</param>
		/// <param name="concurrencyLimit">Number of instances of <paramref name="predicate"/></param>
		/// <returns>
		///		<list type="bullet">
		///			<item>
		///				<term>Value</term>
		///				<description>The element that was matched (if there was a match).</description>
		///			</item>	
		///			<item>
		///				<term>Found</term>
		///				<description>Whether or not there is a match at all.</description>
		///			</item>
		///		</list>
		/// </returns>
		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, CancellationToken, Task<bool>> predicate, int concurrencyLimit, CancellationToken cancellationToken = default) => FindMatchAsyncHelper(f => self.ForEachAsync(f, concurrencyLimit), predicate, cancellationToken);
		#endregion
	}
}
