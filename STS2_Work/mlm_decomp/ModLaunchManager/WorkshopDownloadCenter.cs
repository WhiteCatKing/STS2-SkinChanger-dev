using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

namespace ModLaunchManager;

internal static class WorkshopDownloadCenter
{
	private const double TimeoutSeconds = 300.0;

	private static readonly Dictionary<ulong, WorkshopDownloadRecord> Records = new Dictionary<ulong, WorkshopDownloadRecord>();

	private static readonly HashSet<ulong> Removed = new HashSet<ulong>();

	private static readonly HashSet<ulong> Removing = new HashSet<ulong>();

	private static long _order;

	internal static Action? Changed { get; set; }

	internal static IReadOnlyList<WorkshopDownloadRecord> Downloads => Records.Values.OrderBy((WorkshopDownloadRecord record) => record.Order).ToList();

	internal static int ActiveCount => Records.Values.Count(IsActive);

	internal static int FailedCount => Records.Values.Count((WorkshopDownloadRecord record) => record.State == WorkshopDownloadState.Failed);

	internal static int CompleteCount => Records.Values.Count((WorkshopDownloadRecord record) => record.State == WorkshopDownloadState.Complete);

	internal static bool IsRemoved(ulong id)
	{
		return Removed.Contains(id);
	}

	internal static bool IsRemoving(ulong id)
	{
		return Removing.Contains(id);
	}

	internal static void BeginRemoval(ulong id)
	{
		Removing.Add(id);
	}

	internal static void EndRemoval(ulong id)
	{
		Removing.Remove(id);
	}

	internal static void Allow(ulong id)
	{
		Removed.Remove(id);
	}

	internal static void Remove(ulong id)
	{
		Removing.Remove(id);
		Removed.Add(id);
		Records.Remove(id);
		Notify();
	}

	internal static WorkshopDownloadRecord? Find(ulong itemId)
	{
		return Records.GetValueOrDefault(itemId);
	}

	internal static bool IsActive(WorkshopDownloadRecord record)
	{
		WorkshopDownloadState state = record.State;
		if ((uint)state <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static void Track(ulong itemId, string name, WorkshopDownloadSource source, string? origin = null)
	{
		if (itemId == 0L || IsRemoved(itemId))
		{
			return;
		}
		if (Records.TryGetValue(itemId, out WorkshopDownloadRecord value))
		{
			if (!IsActive(value))
			{
				Restart(value);
				value.Order = ++_order;
			}
		}
		else
		{
			value = new WorkshopDownloadRecord
			{
				ItemId = itemId,
				Name = name,
				Source = source,
				Order = ++_order
			};
			Records[itemId] = value;
		}
		if (!string.IsNullOrWhiteSpace(name))
		{
			value.Name = name;
		}
		value.Source = source;
		value.Origin = origin ?? value.Origin;
		Notify();
	}

	internal static void Poll(double delta)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		PublishedFileId_t val = default(PublishedFileId_t);
		ulong num = default(ulong);
		ulong num2 = default(ulong);
		foreach (WorkshopDownloadRecord item in Records.Values.Where(IsActive))
		{
			if (IsRemoving(item.ItemId))
			{
				continue;
			}
			WorkshopDownloadState state = item.State;
			float progress = item.Progress;
			item.IdleElapsed += delta;
			try
			{
				((PublishedFileId_t)(ref val))._002Ector(item.ItemId);
				EItemState val2 = (EItemState)SteamUGC.GetItemState(val);
				if (((Enum)val2).HasFlag((Enum)(object)(EItemState)16) || ((Enum)val2).HasFlag((Enum)(object)(EItemState)32))
				{
					item.State = WorkshopDownloadState.Downloading;
					if (SteamUGC.GetItemDownloadInfo(val, ref num, ref num2))
					{
						if (num > item.DownloadedBytes)
						{
							item.IdleElapsed = 0.0;
						}
						item.DownloadedBytes = num;
						item.TotalBytes = num2;
						if (num2 != 0)
						{
							item.Progress = Math.Clamp((float)((double)num / (double)num2), 0f, 1f);
						}
					}
				}
				else if (((Enum)val2).HasFlag((Enum)(object)(EItemState)4) && !((Enum)val2).HasFlag((Enum)(object)(EItemState)8))
				{
					item.State = WorkshopDownloadState.Complete;
					item.Progress = 1f;
				}
				if (IsActive(item) && item.IdleElapsed >= 300.0)
				{
					item.State = WorkshopDownloadState.Failed;
				}
			}
			catch
			{
			}
			flag |= state != item.State || Math.Abs(progress - item.Progress) > 0.005f;
		}
		if (flag)
		{
			Notify();
		}
	}

	internal static bool Retry(ulong itemId)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!Records.TryGetValue(itemId, out WorkshopDownloadRecord value) || value.State != WorkshopDownloadState.Failed)
		{
			return false;
		}
		try
		{
			PublishedFileId_t val = default(PublishedFileId_t);
			((PublishedFileId_t)(ref val))._002Ector(itemId);
			if (value.Source != WorkshopDownloadSource.Update)
			{
				SteamUGC.SubscribeItem(val);
			}
			SteamUGC.DownloadItem(val, true);
		}
		catch
		{
			return false;
		}
		Restart(value);
		Notify();
		return true;
	}

	internal static int ClearFinished()
	{
		List<ulong> list = (from record in Records.Values
			where record.State == WorkshopDownloadState.Complete
			select record.ItemId).ToList();
		foreach (ulong item in list)
		{
			Records.Remove(item);
		}
		if (list.Count > 0)
		{
			Notify();
		}
		return list.Count;
	}

	internal static void Reset()
	{
		Records.Clear();
		Removed.Clear();
		Removing.Clear();
		_order = 0L;
		Changed = null;
	}

	private static void Restart(WorkshopDownloadRecord record)
	{
		record.State = WorkshopDownloadState.Queued;
		record.Progress = 0f;
		record.DownloadedBytes = 0uL;
		record.TotalBytes = 0uL;
		record.IdleElapsed = 0.0;
	}

	private static void Notify()
	{
		try
		{
			Changed?.Invoke();
		}
		catch
		{
		}
	}
}
