using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace CznCore.Presentation;

internal static class RunStartResourcePreloader
{
	private const ulong ThreadedLoadTimeoutMsec = 5000uL;

	private const int MaxInFlight = 4;

	public static Task PreloadResource(Node host, string path)
	{
		return PreloadResource(host, path, null);
	}

	public static Task PreloadResource(Node host, string path, Action<Resource>? retain)
	{
		return PreloadAllAsync(host, new _003C_003Ez__ReadOnlySingleElementList<string>(path), () => true, delegate(string _, Resource resource)
		{
			retain?.Invoke(resource);
		});
	}

	public static async Task PreloadAllAsync(Node host, IEnumerable<string?> paths, Func<bool> enabled, Action<string, Resource> loaded)
	{
		Queue<string> queue = new Queue<string>((from path in paths
			where !string.IsNullOrWhiteSpace(path)
			select (path)).Distinct<string>(StringComparer.Ordinal));
		List<(string Path, ulong StartedMsec)> inFlight = new List<(string, ulong)>(4);
		while (enabled() && (queue.Count > 0 || inFlight.Count > 0))
		{
			string result;
			while (inFlight.Count < 4 && queue.TryDequeue(out result))
			{
				if (!ResourceLoader.Exists(result, ""))
				{
					WarmRawFile(result);
				}
				else if (ResourceLoader.HasCached(result))
				{
					loaded(result, ResourceLoader.Load<Resource>(result, string.Empty, (CacheMode)1));
				}
				else if (Request(result))
				{
					inFlight.Add((result, Time.GetTicksMsec()));
				}
				else
				{
					Skip(result, "request rejected");
				}
			}
			for (int num = inFlight.Count - 1; num >= 0; num--)
			{
				(string Path, ulong StartedMsec) tuple = inFlight[num];
				string item = tuple.Path;
				ulong item2 = tuple.StartedMsec;
				ThreadLoadStatus val = ResourceLoader.LoadThreadedGetStatus(item, (Array)null);
				if ((long)val == 1)
				{
					if (Time.GetTicksMsec() - item2 < 5000)
					{
						continue;
					}
					SkinLog.Warn("Threaded preload timed out; continuing native flow path=" + item);
					CollectWhenDone(host, item);
				}
				else if ((long)val == 3)
				{
					loaded(item, ResourceLoader.LoadThreadedGet(item));
				}
				else
				{
					Skip(item, ((object)(ThreadLoadStatus)(ref val)/*cast due to .constrained prefix*/).ToString());
				}
				inFlight.RemoveAt(num);
			}
			if (queue.Count > 0 || inFlight.Count > 0)
			{
				await AwaitFrame(host);
			}
		}
		foreach (var item4 in inFlight)
		{
			string item3 = item4.Path;
			CollectWhenDone(host, item3);
		}
	}

	private static bool Request(string path)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I8
		return (long)ResourceLoader.LoadThreadedRequest(path, string.Empty, CznRuntimeCapabilities.UseSubThreadsForWarmupResourceLoads, (CacheMode)1) == 0;
	}

	private static void Skip(string path, string reason)
	{
		SkinLog.Warn("Threaded preload skipped path=" + path + " reason=" + reason);
	}

	private static async Task CollectWhenDone(Node host, string path)
	{
		while ((long)ResourceLoader.LoadThreadedGetStatus(path, (Array)null) == 1)
		{
			await AwaitFrame(host);
		}
		if ((long)ResourceLoader.LoadThreadedGetStatus(path, (Array)null) == 3)
		{
			ResourceLoader.LoadThreadedGet(path);
		}
	}

	public static void WarmRawFile(string path)
	{
		try
		{
			if (!FileAccess.FileExists(path))
			{
				return;
			}
			FileAccess val = FileAccess.Open(path, (ModeFlags)1);
			try
			{
				ulong num = ((val != null) ? val.GetLength() : 0);
				if (val != null && num != 0L)
				{
					val.GetBuffer((long)Math.Min(num, 2147483647uL));
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Run-start raw preload skipped file " + path + ": " + SkinLog.FormatException(ex), 3);
		}
	}

	public static async Task AwaitFrame(Node host)
	{
		try
		{
			await NodeUtil.AwaitProcessFrame(host, default(CancellationToken));
		}
		catch
		{
			await Task.Yield();
		}
	}
}
