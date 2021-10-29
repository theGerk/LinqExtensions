namespace Gerk.LinqExtensions
{
	using AsyncThen;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using LightweightCancellationToken;

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
		public static Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action)
			=> Task.WhenAll(self.Select(x => action(x)));

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/>.</param>
		/// <param name="breakToken">Stops the queuing new tasks <paramref name="self"/> after <see cref="LightweightCancellationTokenExtensions.Cancel(LightweightCancellationToken)"><c>.Cancel()</c></see> is called.</param>
		/// <returns></returns>
		public static Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action, LightweightCancellationToken breakToken)
			=> self.TakeWhile(_ => !breakToken.IsCancelled()).ForEachAsync(action);

		/// <summary>
		/// Runs an asynchronous function on each element of an enumerable. Execution order is not gaurenteed. This method does not support defered execution and will force execution on <paramref name="self"/>.
		/// </summary>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="action"><see langword="async"/> function that will act on each element of <paramref name="self"/>.</param>
		/// <param name="concurrencyLimit">Maximum tasks to have running in parallel.</param>
		/// <param name="breakToken">Stops the queuing new tasks <paramref name="self"/> after <see cref="LightweightCancellationTokenExtensions.Cancel(LightweightCancellationToken)"><c>.Cancel()</c></see> is called.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<In>(this IEnumerable<In> self, Func<In, Task> action, int concurrencyLimit, LightweightCancellationToken breakToken = default)
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
				var task = await Task.WhenAny(tasks);
				if (breakToken.IsCancelled())
					return;
				tasks.Remove(task);
				tasks.Add(action(inputEnumerator.Current));
			}

			await Task.WhenAll(tasks);
		}
		#endregion

		#region FindMatch
		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match.
		/// <para>This will not cancel tasks once the result has been found, they will just run in the background.</para>
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
			var breakToken = new LightweightCancellationToken();
			self.ForEachAsync(async x =>
			{
				if (await predicate(x))
				{
					breakToken.Cancel();
					tcs.TrySetResult((x, true));
				}
			}, breakToken).Then(() => tcs.TrySetResult((default, false)));
			return tcs.Task;
		}

		/// <summary>
		/// Finds an element in <paramref name="self"/> that returns true from <paramref name="predicate"/>. This is always the first element found, not nessicarily the first element in <paramref name="self"/> that match. Will never run more than <paramref name="concurrencyLimit"/> copies of <paramref name="predicate"/> at a time.
		/// </summary>
		/// <para>This will not cancel tasks once the result has been found, they will just run in the background.</para>
		/// <typeparam name="In">The input members (elements of <paramref name="self"/>).</typeparam>
		/// <param name="self">Enumerable that we are starting with.</param>
		/// <param name="predicate"><see langword="async"/> predicate function that will be applyed to elements of <paramref name="self"/>.</param>
		/// <param name="concurrencyLimit">Maximum number of instances of <paramref name="predicate"/> to run concurrently.</param>
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
		public static Task<(In Value, bool Found)> FindMatchAsync<In>(this IEnumerable<In> self, Func<In, Task<bool>> predicate, int concurrencyLimit)
		{
			var tcs = new TaskCompletionSource<(In, bool)>();
			var breakToken = new LightweightCancellationToken();
			self.ForEachAsync(async x =>
			{
				if (await predicate(x))
				{
					breakToken.Cancel();
					tcs.TrySetResult((x, true));
				}
			}, concurrencyLimit, breakToken).Then(() => tcs.TrySetResult((default, false)));
			return tcs.Task;
		}
		#endregion
	}
}
