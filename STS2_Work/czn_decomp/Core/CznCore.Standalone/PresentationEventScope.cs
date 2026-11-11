using System;
using System.Collections.Generic;
using CznCore.Presentation;

namespace CznCore.Standalone;

public sealed class PresentationEventScope : IDisposable
{
	private sealed class Subscription(PresentationEventScope scope, (Type, string) identity) : IDisposable
	{
		internal IDisposable? Inner;

		internal bool Active = true;

		public void Dispose()
		{
			lock (scope._sync)
			{
				if (Active)
				{
					Active = false;
					Inner?.Dispose();
					Inner = null;
					scope._subscriptions.Remove(identity);
				}
			}
		}
	}

	private readonly object _sync = new object();

	private readonly Dictionary<(Type, string), IDisposable> _subscriptions = new Dictionary<(Type, string), IDisposable>();

	private readonly Action<string, Exception> _reportFailure;

	private bool _disposed;

	public string Owner { get; }

	public PresentationEventScope(string owner, Action<string, Exception> reportFailure)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(owner, "owner");
		ArgumentNullException.ThrowIfNull(reportFailure, "reportFailure");
		Owner = owner;
		_reportFailure = reportFailure;
	}

	public IDisposable Subscribe<T>(string key, Action<T> handler)
	{
		return Subscribe(key, (T _) => true, handler);
	}

	internal IDisposable SubscribeOwned<T>(CharacterSkinProfile profile, string key, Action<T> handler)
	{
		SkinEventOwnership.RequireClassified(typeof(T));
		return Subscribe(key, (T value) => SkinEventOwnership.Accepts(profile, value), handler);
	}

	public IDisposable Subscribe<T>(string key, Predicate<T> accepts, Action<T> handler)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key, "key");
		ArgumentNullException.ThrowIfNull(accepts, "accepts");
		ArgumentNullException.ThrowIfNull(handler, "handler");
		lock (_sync)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			(Type, string) tuple = (typeof(T), key);
			if (_subscriptions.ContainsKey(tuple))
			{
				throw new InvalidOperationException("Duplicate presentation subscription: " + Owner + "/" + key);
			}
			Subscription lease = new Subscription(this, tuple);
			lease.Inner = SkinEventHub.Subscribe(Owner + "/" + key, delegate(T value)
			{
				lock (_sync)
				{
					if (_disposed || !lease.Active)
					{
						return;
					}
					try
					{
						if (accepts(value))
						{
							handler(value);
						}
					}
					catch (Exception arg)
					{
						try
						{
							_reportFailure(Owner + "/" + key, arg);
						}
						catch
						{
						}
					}
				}
			});
			_subscriptions.Add(tuple, lease);
			return lease;
		}
	}

	public void Dispose()
	{
		lock (_sync)
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			foreach (IDisposable item in new List<IDisposable>(_subscriptions.Values))
			{
				item.Dispose();
			}
			_subscriptions.Clear();
		}
	}
}
