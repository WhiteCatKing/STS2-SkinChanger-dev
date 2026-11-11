using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopCatalogSession<T>
{
	private sealed class State
	{
		internal readonly SemaphoreSlim Gate = new SemaphoreSlim(1);

		internal readonly HashSet<ulong> Attempted = new HashSet<ulong>();

		internal Dictionary<ulong, T> Published = new Dictionary<ulong, T>();
	}

	private readonly Dictionary<string, State> _states = new Dictionary<string, State>(StringComparer.Ordinal);

	private State For(string language)
	{
		lock (_states)
		{
			if (!_states.TryGetValue(language, out State value))
			{
				value = (_states[language] = new State());
			}
			return value;
		}
	}

	internal IReadOnlyDictionary<ulong, T> Cached(string language)
	{
		return Volatile.Read(in For(language).Published);
	}

	internal Task<Dictionary<ulong, T>> Get(string language, ulong[] ids, Func<ulong[], Task<Dictionary<ulong, T>>> load, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		Task<Dictionary<ulong, T>> task = Load(For(language), ids, load);
		if (!token.CanBeCanceled)
		{
			return task;
		}
		return task.WaitAsync(token);
	}

	private static async Task<Dictionary<ulong, T>> Load(State state, ulong[] ids, Func<ulong[], Task<Dictionary<ulong, T>>> load)
	{
		await state.Gate.WaitAsync();
		try
		{
			ulong[] missing = (from id in ids.Distinct()
				where !state.Attempted.Contains(id)
				select id).ToArray();
			if (missing.Length == 0)
			{
				return state.Published;
			}
			Dictionary<ulong, T> obj = await load(missing);
			Dictionary<ulong, T> dictionary = new Dictionary<ulong, T>(state.Published);
			HashSet<ulong> hashSet = missing.ToHashSet();
			foreach (var (num2, value) in obj)
			{
				if (hashSet.Contains(num2))
				{
					dictionary[num2] = value;
				}
			}
			state.Attempted.UnionWith(missing);
			Volatile.Write(ref state.Published, dictionary);
			return dictionary;
		}
		finally
		{
			state.Gate.Release();
		}
	}
}
