using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopSessionCache<T>
{
	private readonly Dictionary<string, Lazy<Task<T>>> _entries = new Dictionary<string, Lazy<Task<T>>>(StringComparer.Ordinal);

	public Task<T> Get(string key, Func<Task<T>> load, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		Lazy<Task<T>> value;
		lock (_entries)
		{
			if (!_entries.TryGetValue(key, out value))
			{
				value = (_entries[key] = new Lazy<Task<T>>(load, LazyThreadSafetyMode.ExecutionAndPublication));
			}
		}
		Task<T> value2 = value.Value;
		if (!token.CanBeCanceled)
		{
			return value2;
		}
		return value2.WaitAsync(token);
	}

	public bool TryGet(string key, out T value)
	{
		lock (_entries)
		{
			if (_entries.TryGetValue(key, out Lazy<Task<T>> value2) && value2.IsValueCreated && value2.Value.IsCompletedSuccessfully)
			{
				value = value2.Value.Result;
				return true;
			}
		}
		value = default(T);
		return false;
	}
}
