using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Steamworks;

namespace ModLaunchManager;

internal sealed class WorkshopDownloadQueue
{
	private const double RetrySeconds = 2.0;

	private const double TimeoutSeconds = 300.0;

	private readonly Dictionary<ulong, WorkshopDownloadJob> _jobs = new Dictionary<ulong, WorkshopDownloadJob>();

	private readonly Action _onChanged;

	private readonly WorkshopDownloadSource? _source;

	internal IReadOnlyList<WorkshopDownloadJob> Jobs => _jobs.Values.OrderBy<WorkshopDownloadJob, string>((WorkshopDownloadJob job) => job.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();

	internal int Count => _jobs.Count;

	internal int ActiveCount => _jobs.Values.Count(delegate(WorkshopDownloadJob job)
	{
		WorkshopDownloadState state = job.State;
		return (uint)state <= 1u;
	});

	internal bool HasFailures => _jobs.Values.Any((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Failed);

	internal WorkshopDownloadQueue(Action onChanged, WorkshopDownloadSource? source = null)
	{
		_onChanged = onChanged;
		_source = source;
	}

	internal WorkshopDownloadJob? Find(ulong itemId)
	{
		return _jobs.GetValueOrDefault(itemId);
	}

	internal void Clear()
	{
		_jobs.Clear();
	}

	internal void Forget(ulong itemId)
	{
		_jobs.Remove(itemId);
	}

	internal WorkshopDownloadJob Enqueue(ulong itemId, string displayName, bool subscribe)
	{
		if (_jobs.TryGetValue(itemId, out WorkshopDownloadJob value))
		{
			if (value.Failure == WorkshopDownloadFailure.Cancelled && !WorkshopDownloadCenter.IsRemoved(itemId))
			{
				WorkshopDownloadCenter.Allow(itemId);
				Reset(value);
				RequestDownload(value, subscribe);
				Report(value);
				Notify();
			}
			return value;
		}
		WorkshopDownloadJob workshopDownloadJob = new WorkshopDownloadJob
		{
			ItemId = itemId,
			DisplayName = displayName
		};
		_jobs[itemId] = workshopDownloadJob;
		if (WorkshopDownloadCenter.IsRemoved(itemId))
		{
			workshopDownloadJob.State = WorkshopDownloadState.Failed;
			workshopDownloadJob.Failure = WorkshopDownloadFailure.Cancelled;
			Notify();
			return workshopDownloadJob;
		}
		if (!WorkshopDownloadCenter.IsRemoving(itemId))
		{
			RequestDownload(workshopDownloadJob, subscribe);
		}
		Report(workshopDownloadJob);
		Notify();
		return workshopDownloadJob;
	}

	internal int RetryFailed(Func<WorkshopDownloadJob, bool> shouldSubscribe)
	{
		int num = 0;
		foreach (WorkshopDownloadJob item in _jobs.Values.Where((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Failed))
		{
			Reset(item);
			WorkshopDownloadCenter.Allow(item.ItemId);
			RequestDownload(item, shouldSubscribe(item));
			Report(item);
			num++;
		}
		if (num > 0)
		{
			Notify();
		}
		return num;
	}

	internal bool Retry(ulong itemId, bool subscribe)
	{
		if (!_jobs.TryGetValue(itemId, out WorkshopDownloadJob value) || value.State != WorkshopDownloadState.Failed)
		{
			return false;
		}
		Reset(value);
		WorkshopDownloadCenter.Allow(itemId);
		RequestDownload(value, subscribe);
		Report(value);
		Notify();
		return true;
	}

	internal void Poll(double delta)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		if (_jobs.Count == 0)
		{
			return;
		}
		bool flag = false;
		PublishedFileId_t val = default(PublishedFileId_t);
		ulong num2 = default(ulong);
		ulong num3 = default(ulong);
		foreach (WorkshopDownloadJob value in _jobs.Values)
		{
			if (WorkshopDownloadCenter.IsRemoving(value.ItemId))
			{
				continue;
			}
			if (WorkshopDownloadCenter.IsRemoved(value.ItemId))
			{
				flag |= value.Failure != WorkshopDownloadFailure.Cancelled;
				value.State = WorkshopDownloadState.Failed;
				value.Failure = WorkshopDownloadFailure.Cancelled;
				continue;
			}
			WorkshopDownloadState state = value.State;
			if ((uint)(state - 2) <= 1u)
			{
				continue;
			}
			WorkshopDownloadState state2 = value.State;
			float progress = value.Progress;
			value.Elapsed += delta;
			value.IdleElapsed += delta;
			value.RetryElapsed += delta;
			try
			{
				((PublishedFileId_t)(ref val))._002Ector(value.ItemId);
				EItemState val2 = (EItemState)SteamUGC.GetItemState(val);
				bool num = ((Enum)val2).HasFlag((Enum)(object)(EItemState)16) || ((Enum)val2).HasFlag((Enum)(object)(EItemState)32);
				bool flag2 = ((Enum)val2).HasFlag((Enum)(object)(EItemState)4);
				bool flag3 = ((Enum)val2).HasFlag((Enum)(object)(EItemState)8);
				if (num)
				{
					value.State = WorkshopDownloadState.Downloading;
					if (SteamUGC.GetItemDownloadInfo(val, ref num2, ref num3))
					{
						if (num2 > value.DownloadedBytes)
						{
							value.IdleElapsed = 0.0;
						}
						value.DownloadedBytes = num2;
						if (num3 != 0)
						{
							value.Progress = Math.Clamp((float)((double)num2 / (double)num3), 0f, 1f);
						}
					}
				}
				else if (flag2 && !flag3)
				{
					value.State = WorkshopDownloadState.Complete;
					value.Progress = 1f;
				}
				else if (value.IdleElapsed < 300.0 && value.RetryElapsed >= 2.0)
				{
					SteamUGC.SubscribeItem(val);
					SteamUGC.DownloadItem(val, true);
					value.RetryElapsed = 0.0;
					value.State = WorkshopDownloadState.Queued;
				}
				if (value.State != WorkshopDownloadState.Complete && value.IdleElapsed >= 300.0)
				{
					value.State = WorkshopDownloadState.Failed;
					value.Failure = WorkshopDownloadFailure.Timeout;
				}
			}
			catch (Exception exception)
			{
				FailUnexpected(value, "poll", exception);
			}
			flag |= state2 != value.State || Math.Abs(progress - value.Progress) > 0.005f;
		}
		if (flag)
		{
			Notify();
		}
	}

	private void Report(WorkshopDownloadJob job)
	{
		WorkshopDownloadSource? source = _source;
		if (source.HasValue)
		{
			WorkshopDownloadSource valueOrDefault = source.GetValueOrDefault();
			WorkshopDownloadCenter.Track(job.ItemId, job.DisplayName, valueOrDefault);
		}
	}

	private static void Reset(WorkshopDownloadJob job)
	{
		job.State = WorkshopDownloadState.Queued;
		job.Progress = 0f;
		job.Elapsed = 0.0;
		job.IdleElapsed = 0.0;
		job.DownloadedBytes = 0uL;
		job.RetryElapsed = 0.0;
		job.Failure = null;
	}

	private static void RequestDownload(WorkshopDownloadJob job, bool subscribe)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PublishedFileId_t val = default(PublishedFileId_t);
			((PublishedFileId_t)(ref val))._002Ector(job.ItemId);
			if (subscribe)
			{
				SteamUGC.SubscribeItem(val);
			}
			if (!SteamUGC.DownloadItem(val, true))
			{
				EItemState val2 = (EItemState)SteamUGC.GetItemState(val);
				if (((Enum)val2).HasFlag((Enum)(object)(EItemState)4) && !((Enum)val2).HasFlag((Enum)(object)(EItemState)8))
				{
					job.State = WorkshopDownloadState.Complete;
					job.Progress = 1f;
					return;
				}
			}
			job.State = WorkshopDownloadState.Queued;
			job.RetryElapsed = 0.0;
		}
		catch (Exception exception)
		{
			FailUnexpected(job, "request", exception);
		}
	}

	private static void FailUnexpected(WorkshopDownloadJob job, string operation, Exception exception)
	{
		job.State = WorkshopDownloadState.Failed;
		job.Failure = WorkshopDownloadFailure.Unexpected;
		GD.PushWarning($"{"[ModLaunchManager]"} Workshop download {operation} failed: {exception}");
	}

	private void Notify()
	{
		try
		{
			_onChanged();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} Workshop download UI update failed: {value}");
		}
	}
}
