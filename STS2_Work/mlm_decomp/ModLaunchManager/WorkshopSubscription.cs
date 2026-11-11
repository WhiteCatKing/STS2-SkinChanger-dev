using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

namespace ModLaunchManager;

internal static class WorkshopSubscription
{
	private sealed class Request(Action<bool> done)
	{
		internal Action<bool> Done = done;

		internal IDisposable? Callback;

		internal double Elapsed;
	}

	private static readonly Dictionary<ulong, Request> Pending = new Dictionary<ulong, Request>();

	internal static bool Busy => Pending.Count > 0;

	internal static void Remove(ulong itemId, Action<bool> done)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (itemId == 0L || Pending.ContainsKey(itemId))
		{
			done(obj: false);
			return;
		}
		Request request = new Request(done);
		Pending[itemId] = request;
		WorkshopDownloadCenter.BeginRemoval(itemId);
		try
		{
			CallResult<RemoteStorageUnsubscribePublishedFileResult_t> val = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create((APIDispatchDelegate<RemoteStorageUnsubscribePublishedFileResult_t>)delegate(RemoteStorageUnsubscribePublishedFileResult_t result, bool failed)
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Invalid comparison between Unknown and I4
				Finish(itemId, request, !failed && (int)result.m_eResult == 1);
			});
			request.Callback = (IDisposable?)val;
			SteamAPICall_t val2 = SteamUGC.UnsubscribeItem(new PublishedFileId_t(itemId));
			if (val2 == SteamAPICall_t.Invalid)
			{
				Finish(itemId, request, success: false);
			}
			else
			{
				val.Set(val2, (APIDispatchDelegate<RemoteStorageUnsubscribePublishedFileResult_t>)null);
			}
		}
		catch
		{
			Finish(itemId, request, success: false);
		}
	}

	private static void Finish(ulong id, Request request, bool success)
	{
		if (Pending.TryGetValue(id, out Request value) && request == value)
		{
			Pending.Remove(id);
			request.Callback?.Dispose();
			WorkshopDownloadCenter.EndRemoval(id);
			if (success)
			{
				WorkshopDownloadCenter.Remove(id);
			}
			request.done(success);
		}
	}

	internal static void Poll(double delta)
	{
		KeyValuePair<ulong, Request>[] array = Pending.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<ulong, Request> keyValuePair = array[i];
			keyValuePair.Value.Elapsed += delta;
			if (keyValuePair.Value.Elapsed >= 30.0)
			{
				Finish(keyValuePair.Key, keyValuePair.Value, success: false);
			}
		}
	}

	internal static void Reset()
	{
		foreach (KeyValuePair<ulong, Request> item in Pending)
		{
			item.Value.Callback?.Dispose();
			WorkshopDownloadCenter.EndRemoval(item.Key);
		}
		Pending.Clear();
	}
}
