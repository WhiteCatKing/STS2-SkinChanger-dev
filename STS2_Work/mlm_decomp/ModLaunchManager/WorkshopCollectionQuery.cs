using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Steamworks;

namespace ModLaunchManager;

internal sealed class WorkshopCollectionQuery : IDisposable
{
	private sealed class LiveQuery(UGCQueryHandle_t handle) : IDisposable
	{
		private IDisposable? _callback;

		private bool _released;

		internal void Attach(IDisposable callback)
		{
			_callback = callback;
		}

		internal void Release()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			_callback = null;
			if (_released)
			{
				return;
			}
			_released = true;
			try
			{
				SteamUGC.ReleaseQueryUGCRequest(handle);
			}
			catch
			{
			}
		}

		public void Dispose()
		{
			try
			{
				_callback?.Dispose();
			}
			finally
			{
				Release();
			}
		}
	}

	private const int MaxIdsPerQuery = 50;

	private readonly uint _appId;

	private readonly string _language;

	private readonly List<LiveQuery> _live = new List<LiveQuery>();

	private int _generation;

	internal WorkshopCollectionQuery(uint appId, string steamLanguage)
	{
		_appId = appId;
		_language = steamLanguage;
	}

	internal void Fetch(IReadOnlyList<ulong> ids, Action<IReadOnlyList<WorkshopItemFacts>?> done)
	{
		int generation = _generation;
		List<WorkshopItemFacts> results = new List<WorkshopItemFacts>();
		int pending = 0;
		bool failed = false;
		bool sending = true;
		for (int i = 0; i < ids.Count; i += 50)
		{
			PublishedFileId_t[] batch2 = ids.Skip(i).Take(50).Select((Func<ulong, PublishedFileId_t>)((ulong id) => new PublishedFileId_t(id)))
				.ToArray();
			pending++;
			if (!Send(() => SteamUGC.CreateQueryUGCDetailsRequest(batch2, (uint)batch2.Length), Read, Answer))
			{
				pending--;
				failed = true;
				break;
			}
		}
		sending = false;
		if (pending == 0)
		{
			done(failed ? null : results);
		}
		void Answer(IReadOnlyList<WorkshopItemFacts>? batch)
		{
			if (generation == _generation)
			{
				if (batch == null)
				{
					failed = true;
				}
				else
				{
					results.AddRange(batch);
				}
				pending--;
				if (pending == 0 && !sending)
				{
					done(failed ? null : results);
				}
			}
		}
	}

	internal void ListMine(Action<IReadOnlyList<CollectionChoice>?> done)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		int generation = _generation;
		IReadOnlyList<CollectionChoice> own = null;
		IReadOnlyList<CollectionChoice> favorited = null;
		int pending = 0;
		bool failed = false;
		bool sending = true;
		try
		{
			CSteamID steamID = SteamUser.GetSteamID();
			AccountID_t account = ((CSteamID)(ref steamID)).GetAccountID();
			EUserUGCList[] array = (EUserUGCList[])(object)new EUserUGCList[2]
			{
				default(EUserUGCList),
				(EUserUGCList)5
			};
			foreach (EUserUGCList list in array)
			{
				bool isFavorites = (int)list == 5;
				int num = pending;
				pending = num + 1;
				if (!Send(() => SteamUGC.CreateQueryUserUGCRequest(account, list, (EUGCMatchingUGCType)3, (EUserUGCListSortOrder)3, AppId_t.Invalid, new AppId_t(_appId), 1u), (SteamUGCQueryCompleted_t result) => ReadChoices(result, isFavorites), delegate(List<CollectionChoice>? found)
				{
					if (generation == _generation)
					{
						failed = failed || found == null;
						if (isFavorites)
						{
							favorited = found;
						}
						else
						{
							own = found;
						}
						int num2 = pending;
						pending = num2 - 1;
						Finish();
					}
				}))
				{
					num = pending;
					pending = num - 1;
					failed = true;
					break;
				}
			}
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not list the player's collections: {value}");
			failed = true;
		}
		sending = false;
		Finish();
		void Finish()
		{
			if (pending == 0 && !sending)
			{
				done(failed ? null : WorkshopCollectionChoices.Merge(own ?? Array.Empty<CollectionChoice>(), favorited ?? Array.Empty<CollectionChoice>()));
			}
		}
	}

	internal void Cancel()
	{
		_generation++;
		foreach (LiveQuery item in _live.ToList())
		{
			try
			{
				item.Dispose();
			}
			catch
			{
			}
		}
		_live.Clear();
	}

	public void Dispose()
	{
		Cancel();
	}

	private bool Send<T>(Func<UGCQueryHandle_t> create, Func<SteamUGCQueryCompleted_t, T> read, Action<T?> answer) where T : class
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			UGCQueryHandle_t val = create();
			if (val == UGCQueryHandle_t.Invalid)
			{
				return false;
			}
			LiveQuery live = new LiveQuery(val);
			SteamUGC.SetReturnChildren(val, true);
			SteamUGC.SetLanguage(val, _language);
			SteamAPICall_t val2 = SteamUGC.SendQueryUGCRequest(val);
			if (val2 == SteamAPICall_t.Invalid)
			{
				live.Release();
				return false;
			}
			CallResult<SteamUGCQueryCompleted_t> val3 = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)delegate(SteamUGCQueryCompleted_t result, bool ioFailure)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				T obj = null;
				try
				{
					if (!ioFailure && (int)result.m_eResult == 1)
					{
						obj = read(result);
					}
					else
					{
						GD.PushWarning("[ModLaunchManager] collection query failed: " + (ioFailure ? "IO failure" : ((object)(EResult)(ref result.m_eResult)/*cast due to .constrained prefix*/).ToString()));
					}
				}
				catch (Exception value)
				{
					GD.PushWarning($"{"[ModLaunchManager]"} collection query threw: {value}");
				}
				finally
				{
					live.Release();
					_live.Remove(live);
				}
				answer(obj);
			});
			live.Attach((IDisposable)val3);
			_live.Add(live);
			val3.Set(val2, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
			return true;
		}
		catch (Exception value2)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not send a collection query: {value2}");
			return false;
		}
	}

	private List<CollectionChoice> ReadChoices(SteamUGCQueryCompleted_t result, bool favorited)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		List<CollectionChoice> list = new List<CollectionChoice>();
		SteamUGCDetails_t val = default(SteamUGCDetails_t);
		for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
		{
			if (SteamUGC.GetQueryUGCResult(result.m_handle, num, ref val) && (int)val.m_eResult == 1 && (int)val.m_eFileType == 2 && val.m_nConsumerAppID.m_AppId == _appId)
			{
				list.Add(new CollectionChoice(val.m_nPublishedFileId.m_PublishedFileId, ((SteamUGCDetails_t)(ref val)).m_rgchTitle ?? string.Empty, val.m_unNumChildren, favorited));
			}
		}
		return list;
	}

	private List<WorkshopItemFacts> Read(SteamUGCQueryCompleted_t result)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		List<WorkshopItemFacts> list = new List<WorkshopItemFacts>();
		SteamUGCDetails_t val = default(SteamUGCDetails_t);
		for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
		{
			if (!SteamUGC.GetQueryUGCResult(result.m_handle, num, ref val))
			{
				continue;
			}
			ulong publishedFileId = val.m_nPublishedFileId.m_PublishedFileId;
			if ((int)val.m_eResult != 1 || val.m_bBanned)
			{
				list.Add(new WorkshopItemFacts(publishedFileId, WorkshopItemKind.Unavailable, ((SteamUGCDetails_t)(ref val)).m_rgchTitle ?? string.Empty, Array.Empty<ulong>()));
				continue;
			}
			WorkshopItemKind workshopItemKind;
			if (val.m_nConsumerAppID.m_AppId != _appId)
			{
				workshopItemKind = WorkshopItemKind.OtherGame;
				goto IL_00a7;
			}
			EWorkshopFileType eFileType = val.m_eFileType;
			if ((int)eFileType <= 1)
			{
				goto IL_009b;
			}
			WorkshopItemKind workshopItemKind2;
			if ((int)eFileType != 2)
			{
				if ((int)eFileType == 15)
				{
					goto IL_009b;
				}
				workshopItemKind2 = WorkshopItemKind.NotAMod;
			}
			else
			{
				workshopItemKind2 = WorkshopItemKind.Collection;
			}
			goto IL_00a3;
			IL_00a3:
			workshopItemKind = workshopItemKind2;
			goto IL_00a7;
			IL_009b:
			workshopItemKind2 = WorkshopItemKind.Item;
			goto IL_00a3;
			IL_00a7:
			WorkshopItemKind workshopItemKind3 = workshopItemKind;
			ulong[] children = Array.Empty<ulong>();
			bool flag = val.m_unNumChildren != 0;
			if (flag)
			{
				bool flag2 = (uint)workshopItemKind3 <= 1u;
				flag = flag2;
			}
			if (flag)
			{
				PublishedFileId_t[] array = (PublishedFileId_t[])(object)new PublishedFileId_t[val.m_unNumChildren];
				if (SteamUGC.GetQueryUGCChildren(result.m_handle, num, array, val.m_unNumChildren))
				{
					children = (from child in array
						select child.m_PublishedFileId into child
						where child != 0
						select child).ToArray();
				}
			}
			list.Add(new WorkshopItemFacts(publishedFileId, workshopItemKind3, ((SteamUGCDetails_t)(ref val)).m_rgchTitle ?? string.Empty, children));
		}
		return list;
	}
}
