using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class DeferredRegistrationQueue<TValue>
{
	private readonly Dictionary<string, TValue> _pending = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _completed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public int PendingCount => _pending.Count;

	public bool IsCompleted(string key)
	{
		return _completed.Contains(key);
	}

	public DeferredRegistrationResult TryRegister(string key, TValue value, bool isReady, Action<TValue> register)
	{
		if (_completed.Contains(key))
		{
			return DeferredRegistrationResult.AlreadyCompleted;
		}
		_pending[key] = value;
		if (!isReady)
		{
			return DeferredRegistrationResult.Deferred;
		}
		register(value);
		_pending.Remove(key);
		_completed.Add(key);
		return DeferredRegistrationResult.Completed;
	}

	public int RetryPending(bool isReady, Action<TValue> register, Action<string, Exception>? onFailure = null)
	{
		if (!isReady)
		{
			return 0;
		}
		int num = 0;
		KeyValuePair<string, TValue>[] array = _pending.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, TValue> keyValuePair = array[i];
			try
			{
				if (TryRegister(keyValuePair.Key, keyValuePair.Value, isReady: true, register) == DeferredRegistrationResult.Completed)
				{
					num++;
				}
			}
			catch (Exception arg)
			{
				onFailure?.Invoke(keyValuePair.Key, arg);
			}
		}
		return num;
	}
}
