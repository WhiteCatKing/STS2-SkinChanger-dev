using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModLaunchManager;

internal static class FriendFileChecks
{
	private sealed record Result(string? Fingerprint, int Round);

	private static readonly object Gate = new object();

	private static readonly StringComparer PathComparer = (OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

	private static readonly Dictionary<string, Result> Results = new Dictionary<string, Result>(PathComparer);

	private static readonly Queue<string> ToList = new Queue<string>();

	private static readonly Queue<string> ToRead = new Queue<string>();

	private static readonly HashSet<string> Queued = new HashSet<string>(PathComparer);

	private static CancellationTokenSource? _cancel;

	private static bool _working;

	private static int _round = 1;

	private static int _changes;

	internal const string SampleSameFiles = "sample:same-files";

	internal static bool Busy
	{
		get
		{
			lock (Gate)
			{
				return Queued.Count > 0;
			}
		}
	}

	internal static int Changes => Volatile.Read(in _changes);

	internal static bool? Matches(string? modPath, string? expected, bool current)
	{
		if (string.IsNullOrWhiteSpace(modPath) || string.IsNullOrWhiteSpace(expected))
		{
			return false;
		}
		if (string.Equals(expected, "sample:same-files", StringComparison.Ordinal))
		{
			return true;
		}
		lock (Gate)
		{
			Results.TryGetValue(modPath, out Result value);
			if (value == null || value.Round != _round)
			{
				Request(modPath);
				if (value == null || current)
				{
					return null;
				}
			}
			return value.Fingerprint != null && string.Equals(value.Fingerprint, expected, StringComparison.Ordinal);
		}
	}

	internal static void Recheck()
	{
		lock (Gate)
		{
			_round++;
		}
	}

	internal static void Cancel()
	{
		lock (Gate)
		{
			_cancel?.Cancel();
			_cancel = null;
			_working = false;
			ToList.Clear();
			ToRead.Clear();
			Queued.Clear();
		}
	}

	internal static void Reset()
	{
		lock (Gate)
		{
			Cancel();
			Results.Clear();
			_round++;
		}
	}

	private static void Request(string modPath)
	{
		if (!Queued.Add(modPath))
		{
			return;
		}
		ToList.Enqueue(modPath);
		if (!_working)
		{
			_working = true;
			if (_cancel == null)
			{
				_cancel = new CancellationTokenSource();
			}
			CancellationToken cancellation = _cancel.Token;
			Task.Factory.StartNew(delegate
			{
				Work(cancellation);
			}, cancellation, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}
	}

	private static void Work(CancellationToken cancellation)
	{
		while (true)
		{
			bool flag;
			string text;
			int round;
			lock (Gate)
			{
				if (cancellation.IsCancellationRequested)
				{
					break;
				}
				flag = ToList.Count == 0;
				if (flag && ToRead.Count == 0)
				{
					_working = false;
					break;
				}
				text = (flag ? ToRead.Dequeue() : ToList.Dequeue());
				round = _round;
			}
			string fingerprint;
			if (!flag)
			{
				if (!ModContentFingerprint.TryGetKnown(text, out string token))
				{
					lock (Gate)
					{
						if (!cancellation.IsCancellationRequested)
						{
							ToRead.Enqueue(text);
						}
					}
					continue;
				}
				fingerprint = token;
			}
			else
			{
				try
				{
					fingerprint = (ModContentFingerprint.TryCompute(text, out string token2, out string _, cancellation) ? token2 : null);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
			lock (Gate)
			{
				if (cancellation.IsCancellationRequested)
				{
					break;
				}
				Queued.Remove(text);
				Results[text] = new Result(fingerprint, round);
				_changes++;
			}
		}
	}
}
