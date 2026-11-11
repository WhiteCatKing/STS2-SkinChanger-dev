using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal static class CombatOverlayPreload
{
	private const float PollIntervalSeconds = 0.1f;

	private static readonly Dictionary<string, List<Resource>> Held = new Dictionary<string, List<Resource>>();

	private static readonly Dictionary<string, int> Generations = new Dictionary<string, int>();

	public static void ScheduleThreadedWhileEnabled(string key, float delaySeconds, Func<bool> isEnabled, IReadOnlyList<string>? directoryRoots, IReadOnlyList<string>? explicitPaths = null, Action? onCompleted = null, Action<Exception>? onFailed = null)
	{
		ArgumentNullException.ThrowIfNull(isEnabled, "isEnabled");
		Release(key);
		int scheduledGeneration = Generations[key];
		try
		{
			if (!isEnabled())
			{
				return;
			}
			GodotScheduler.Delay(Math.Max(0.01f, delaySeconds), delegate
			{
				try
				{
					if (Generations.TryGetValue(key, out var value) && value == scheduledGeneration)
					{
						if (!isEnabled())
						{
							Release(key);
						}
						else
						{
							HoldThreaded(key, directoryRoots, explicitPaths, delegate
							{
								try
								{
									if (!isEnabled())
									{
										Release(key);
									}
									else
									{
										onCompleted?.Invoke();
									}
								}
								catch (Exception obj)
								{
									Release(key);
									onFailed?.Invoke(obj);
								}
							});
						}
					}
				}
				catch (Exception obj2)
				{
					Release(key);
					onFailed?.Invoke(obj2);
				}
			});
		}
		catch (Exception obj3)
		{
			if (Generations.TryGetValue(key, out var value2) && value2 == scheduledGeneration)
			{
				Release(key);
			}
			onFailed?.Invoke(obj3);
		}
	}

	public static void HoldThreaded(string key, IReadOnlyList<string>? directoryRoots, IReadOnlyList<string>? explicitPaths = null, Action? onCompleted = null)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Release(key);
		int value;
		int generation = (Generations.TryGetValue(key, out value) ? value : 0);
		List<string> list = new List<string>();
		if (directoryRoots != null)
		{
			foreach (string directoryRoot in directoryRoots)
			{
				CollectDirectoryPaths(directoryRoot, list);
			}
		}
		if (explicitPaths != null)
		{
			list.AddRange(explicitPaths);
		}
		List<string> list2 = new List<string>();
		foreach (string item in list)
		{
			try
			{
				if ((int)ResourceLoader.LoadThreadedRequest(item, "", false, (CacheMode)1) == 0)
				{
					list2.Add(item);
				}
			}
			catch
			{
			}
		}
		if (list2.Count == 0)
		{
			onCompleted?.Invoke();
			return;
		}
		Held[key] = new List<Resource>(list2.Count);
		PollPending(key, generation, list2, onCompleted);
	}

	public static void Release(string key)
	{
		Held.Remove(key);
		Generations[key] = ((!Generations.TryGetValue(key, out var value)) ? 1 : (value + 1));
	}

	private static void PollPending(string key, int generation, List<string> pending, Action? onCompleted)
	{
		GodotScheduler.Delay(0.1f, delegate
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Invalid comparison between Unknown and I8
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Invalid comparison between Unknown and I8
			int value;
			bool flag = !Generations.TryGetValue(key, out value) || value != generation;
			List<Resource> value2 = null;
			if (!flag && !Held.TryGetValue(key, out value2))
			{
				flag = true;
			}
			for (int num = pending.Count - 1; num >= 0; num--)
			{
				string text = pending[num];
				ThreadLoadStatus val = ResourceLoader.LoadThreadedGetStatus(text, (Array)null);
				if ((long)val != 1)
				{
					pending.RemoveAt(num);
					if ((long)val == 3)
					{
						try
						{
							Resource val2 = ResourceLoader.LoadThreadedGet(text);
							if (!flag && val2 != null)
							{
								value2.Add(val2);
							}
						}
						catch
						{
						}
					}
				}
			}
			if (pending.Count > 0)
			{
				PollPending(key, generation, pending, onCompleted);
			}
			else if (!flag)
			{
				onCompleted?.Invoke();
			}
		});
	}

	private static void CollectDirectoryPaths(string root, List<string> paths)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			DirAccess val = DirAccess.Open(root);
			try
			{
				if (val == null)
				{
					return;
				}
				val.ListDirBegin();
				string next = val.GetNext();
				while (!string.IsNullOrEmpty(next))
				{
					if (!(next == ".") && !(next == ".."))
					{
						string text = root.TrimEnd('/') + "/" + next;
						if (val.CurrentIsDir())
						{
							CollectDirectoryPaths(text, paths);
						}
						else if (text.EndsWith(".tres", StringComparison.Ordinal) || text.EndsWith(".tscn", StringComparison.Ordinal))
						{
							paths.Add(text);
						}
					}
					next = val.GetNext();
				}
				val.ListDirEnd();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
	}
}
