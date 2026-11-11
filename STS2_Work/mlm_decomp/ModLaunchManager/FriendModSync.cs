using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using Steamworks;

namespace ModLaunchManager;

internal static class FriendModSync
{
	private sealed record PublicationSnapshot(FriendModList List, IReadOnlyList<string> Paths);

	private sealed record PreparedPublication(FriendModList List, FriendPresencePacket? Packet, FriendSyncError? Error);

	private const double FriendRefreshSeconds = 5.0;

	private const double DownloadPollSeconds = 0.5;

	private static readonly List<OnlineFriendMods> OnlineFriends = new List<OnlineFriendMods>();

	private static readonly WorkshopDownloadQueue DownloadQueue = new WorkshopDownloadQueue(Notify, WorkshopDownloadSource.Friend);

	private static readonly HashSet<ulong> RichPresenceRequested = new HashSet<ulong>();

	private static List<OnlineFriendMods>? _sampleSaved;

	private static Action? _onChanged;

	private static double _friendElapsed;

	private static double _downloadElapsed;

	private static int _lastPublishedChunks;

	private static string _lastPublishedHeader = string.Empty;

	private static bool _managerScreenPublished;

	private static LaunchPlan? _publicationPlan;

	private static string _publicationGameVersion = string.Empty;

	private static bool _available;

	private static FriendSyncError? _availabilityError;

	private static FriendSyncError? _publishError;

	private static PendingFriendImport? _pending;

	private static Task<PreparedPublication>? _publishing;

	private static PublicationSnapshot? _publishQueued;

	private static CancellationTokenSource? _publishCancel;

	private static bool _managerScreenAfterPublication;

	private static LaunchPlan? _downloadsWaitFor;

	private static int _checksSeen;

	internal static bool IsAvailable => _available;

	internal static bool ShowingSample => _sampleSaved != null;

	internal static bool CanOpen
	{
		get
		{
			if (!_available)
			{
				return _pending != null;
			}
			return true;
		}
	}

	internal static string? AvailabilityError => DescribeError(_availabilityError);

	internal static string? PublishError => DescribeError(_publishError);

	internal static IReadOnlyList<OnlineFriendMods> Friends => OnlineFriends;

	internal static IReadOnlyList<WorkshopDownloadJob> Downloads => DownloadQueue.Jobs;

	internal static int ActiveDownloadCount => DownloadQueue.ActiveCount;

	internal static PendingFriendImport? Pending => _pending;

	internal static bool DownloadsWaitForChecks => _downloadsWaitFor != null;

	internal static bool PublicationPending
	{
		get
		{
			if (_publishing == null)
			{
				return _publishQueued != null;
			}
			return true;
		}
	}

	internal static void BeginSample(IEnumerable<OnlineFriendMods> sample)
	{
		if (_sampleSaved == null)
		{
			_sampleSaved = OnlineFriends.ToList();
		}
		OnlineFriends.Clear();
		OnlineFriends.AddRange(sample);
		Notify();
	}

	internal static void EndSample()
	{
		if (_sampleSaved != null)
		{
			OnlineFriends.Clear();
			OnlineFriends.AddRange(_sampleSaved);
			_sampleSaved = null;
			Notify();
		}
	}

	internal static void Start(LaunchPlan plan, string gameVersion, Action onChanged)
	{
		CancelPublication();
		FriendFileChecks.Reset();
		_downloadsWaitFor = null;
		_onChanged = onChanged;
		_lastPublishedChunks = 0;
		_lastPublishedHeader = string.Empty;
		_publicationPlan = plan;
		_publicationGameVersion = gameVersion;
		_publishError = null;
		_friendElapsed = 5.0;
		_downloadElapsed = 0.5;
		DownloadQueue.Clear();
		OnlineFriends.Clear();
		RichPresenceRequested.Clear();
		_pending = FriendImportStore.Load();
		try
		{
			_available = WorkshopUpdates.IsAvailable;
			_availabilityError = (_available ? null : FriendSyncError.Create("friendSync.error.steamNotInitialized"));
			if (!_available)
			{
				Notify();
				return;
			}
			Publish(plan, gameVersion);
			if (_pending != null)
			{
				QueuePendingDownloads(plan);
			}
			if (PublicationPending)
			{
				_managerScreenAfterPublication = true;
			}
			else
			{
				PublishManagerScreen();
			}
			RefreshFriends();
		}
		catch (Exception value)
		{
			_available = false;
			_availabilityError = FriendSyncError.Create("friendSync.error.unavailableUnexpected");
			GD.PushWarning($"{"[ModLaunchManager]"} friend mod sync is unavailable: {value}");
			Notify();
		}
	}

	internal static void Stop()
	{
		CancelPublication();
		FriendFileChecks.Reset();
		_downloadsWaitFor = null;
		ClearPublishedPresence();
		_publicationPlan = null;
		_publicationGameVersion = string.Empty;
		_onChanged = null;
		_available = false;
		_availabilityError = null;
		OnlineFriends.Clear();
		DownloadQueue.Clear();
		RichPresenceRequested.Clear();
	}

	internal static void Poll(double delta)
	{
		if (!_available)
		{
			return;
		}
		CompletePublication();
		DecideWaitingDownloads();
		_friendElapsed += delta;
		_downloadElapsed += delta;
		if (_friendElapsed >= 5.0)
		{
			_friendElapsed = 0.0;
			RefreshPublication();
			RefreshFriends();
		}
		if (_downloadElapsed >= 0.5)
		{
			double downloadElapsed = _downloadElapsed;
			_downloadElapsed = 0.0;
			int num = CompletedDownloads();
			DownloadQueue.Poll(downloadElapsed);
			if (CompletedDownloads() > num)
			{
				FriendFileChecks.Recheck();
				Notify();
			}
		}
	}

	private static int CompletedDownloads()
	{
		return DownloadQueue.Jobs.Count((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Complete);
	}

	private static void DecideWaitingDownloads()
	{
		LaunchPlan downloadsWaitFor = _downloadsWaitFor;
		if (downloadsWaitFor != null && (FriendFileChecks.Changes != _checksSeen || !FriendFileChecks.Busy))
		{
			_checksSeen = FriendFileChecks.Changes;
			QueuePendingDownloads(downloadsWaitFor);
			if (_downloadsWaitFor == null)
			{
				Notify();
			}
		}
	}

	internal static void RefreshNow()
	{
		if (!_available)
		{
			Notify();
			return;
		}
		_friendElapsed = 0.0;
		RichPresenceRequested.Clear();
		RefreshPublication();
		RefreshFriends();
	}

	private static void RefreshPublication()
	{
		if (_publicationPlan != null)
		{
			Publish(_publicationPlan, _publicationGameVersion);
		}
	}

	internal static void Publish(LaunchPlan plan, string gameVersion)
	{
		if (!_available)
		{
			return;
		}
		try
		{
			PublicationSnapshot publicationSnapshot = Capture(plan, gameVersion);
			if (_publishing != null)
			{
				_publishQueued = publicationSnapshot;
			}
			else
			{
				BeginPublication(publicationSnapshot);
			}
		}
		catch (Exception value)
		{
			SetPublishError(FriendSyncError.Create("friendSync.error.publishUnexpected"));
			GD.PushWarning($"{"[ModLaunchManager]"} could not publish the friend mod list: {value}");
		}
	}

	private static void BeginPublication(PublicationSnapshot snapshot)
	{
		if (_publishCancel == null)
		{
			_publishCancel = new CancellationTokenSource();
		}
		CancellationToken cancellation = _publishCancel.Token;
		_publishing = Task.Run(() => Prepare(snapshot, cancellation), cancellation);
	}

	private static PreparedPublication Prepare(PublicationSnapshot snapshot, CancellationToken cancellation)
	{
		for (int i = 0; i < snapshot.List.Mods.Count; i++)
		{
			ModContentFingerprint.TryCompute(snapshot.Paths[i], out string token, out string _, cancellation);
			snapshot.List.Mods[i].ContentHash = token;
		}
		FriendPresenceCodec.TryEncode(snapshot.List, out FriendPresencePacket packet, out FriendSyncError error2);
		return new PreparedPublication(snapshot.List, packet, error2);
	}

	private static void CompletePublication()
	{
		Task<PreparedPublication> publishing = _publishing;
		if (publishing != null && publishing.IsCompleted)
		{
			_publishing = null;
			try
			{
				ApplyPublication(publishing.GetAwaiter().GetResult());
			}
			catch (Exception value)
			{
				SetPublishError(FriendSyncError.Create("friendSync.error.publishUnexpected"));
				GD.PushWarning($"{"[ModLaunchManager]"} could not publish the friend mod list: {value}");
			}
			if (_managerScreenAfterPublication)
			{
				_managerScreenAfterPublication = false;
				PublishManagerScreen();
			}
			PublicationSnapshot publishQueued = _publishQueued;
			if ((object)publishQueued != null)
			{
				_publishQueued = null;
				BeginPublication(publishQueued);
			}
		}
	}

	private static void CancelPublication()
	{
		_publishCancel?.Cancel();
		_publishCancel = null;
		_publishing = null;
		_publishQueued = null;
		_managerScreenAfterPublication = false;
	}

	private static void ApplyPublication(PreparedPublication prepared)
	{
		FriendModList list = prepared.List;
		FriendPresencePacket packet = prepared.Packet;
		FriendSyncError error = prepared.Error;
		try
		{
			if (packet == null)
			{
				string value = ((error?.Key == "friendSync.error.listTooLarge") ? "overflow" : "invalid");
				string text = $"{1}|0|{value}|{list.Mods.Count}";
				if (_lastPublishedHeader != text)
				{
					if (!SteamFriends.SetRichPresence("mlm_mods", text))
					{
						SetPublishError(FriendSyncError.Create("friendSync.error.steamFailureHeaderRejected"));
						return;
					}
					_lastPublishedHeader = text;
				}
				SetPublishError(error);
			}
			else
			{
				if (packet.Header == _lastPublishedHeader && _publishError == null)
				{
					return;
				}
				for (int i = 0; i < packet.Chunks.Count; i++)
				{
					if (!SteamFriends.SetRichPresence(FriendPresenceCodec.ChunkKey(i), packet.Chunks[i]))
					{
						SetPublishError(FriendSyncError.Create("friendSync.error.steamChunkRejected", ("index", i + 1)));
						return;
					}
				}
				for (int j = packet.Chunks.Count; j < _lastPublishedChunks; j++)
				{
					SteamFriends.SetRichPresence(FriendPresenceCodec.ChunkKey(j), string.Empty);
				}
				if (!SteamFriends.SetRichPresence("mlm_mods", packet.Header))
				{
					SetPublishError(FriendSyncError.Create("friendSync.error.steamHeaderRejected"));
					return;
				}
				_lastPublishedChunks = packet.Chunks.Count;
				_lastPublishedHeader = packet.Header;
				SetPublishError(null);
				GD.Print($"{"[ModLaunchManager]"} published {list.Mods.Count} enabled mods in {packet.Chunks.Count} friend-sync chunk(s).");
			}
		}
		catch (Exception value2)
		{
			SetPublishError(FriendSyncError.Create("friendSync.error.publishUnexpected"));
			GD.PushWarning($"{"[ModLaunchManager]"} could not publish the friend mod list: {value2}");
		}
	}

	internal static FriendModComparison Compare(FriendModList remote, LaunchPlan local, FriendModEvaluation? evaluation = null)
	{
		if (evaluation == null)
		{
			evaluation = new FriendModEvaluation();
		}
		FriendComparisonResult<LaunchEntry> friendComparisonResult = FriendModComparisonLogic.Compare(remote.Mods, (from entry in local.Entries
			where !entry.IsManager
			select new FriendLocalCandidate<LaunchEntry>(entry, entry.Id, entry.Enabled, entry.Mod.manifest?.version ?? string.Empty, WorkshopUpdates.ItemIdFor(entry.Mod), (FriendModEntry mod) => evaluation.ContentMatches(entry, mod))).ToList());
		return new FriendModComparison
		{
			Rows = friendComparisonResult.Rows.Select((FriendComparisonDecision<LaunchEntry> row) => new FriendModComparisonRow(row.Remote, row.Local?.Value, row.State, row.LocalPosition)).ToList(),
			ExtraEnabled = friendComparisonResult.ExtraEnabled.Select((FriendLocalCandidate<LaunchEntry> entry) => entry.Value).ToList(),
			ExactOrder = friendComparisonResult.ExactOrder
		};
	}

	internal static FriendImportStart BeginImport(OnlineFriendMods friend, LaunchPlan plan, bool gameplayOnly = false)
	{
		if (friend.List == null)
		{
			return new FriendImportStart(friend.Error ?? FriendSyncError.Create("friendSync.error.noSharedList"));
		}
		if (_pending != null)
		{
			return new FriendImportStart(FriendSyncError.Create("friendSync.error.pendingImportExists"));
		}
		FriendModList friendModList = (gameplayOnly ? FriendImportSelection.GameplayOnly(friend.List, (string id) => plan.Entries.FirstOrDefault((LaunchEntry e) => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase))?.Mod.manifest?.dependencies?.Select((ModDependency d) => d.id).OfType<string>() ?? Array.Empty<string>()) : friend.List);
		if (gameplayOnly && friendModList.Mods.Count == 0)
		{
			return new FriendImportStart(FriendSyncError.Create("friendSync.error.noGameplay"));
		}
		PendingFriendImport pending = new PendingFriendImport
		{
			SteamId = friend.SteamId,
			FriendName = friend.PersonaName,
			CreatedUtc = DateTime.UtcNow.ToString("O"),
			List = friendModList,
			GameplayOnly = gameplayOnly
		};
		if (!FriendImportStore.Save(pending, out string error))
		{
			GD.PushWarning("[ModLaunchManager] could not save pending friend import: " + error);
			return new FriendImportStart(FriendSyncError.Create("friendSync.error.pendingSaveFailed"));
		}
		_pending = pending;
		DownloadQueue.Clear();
		FriendFileChecks.Recheck();
		QueuePendingDownloads(plan);
		Notify();
		return new FriendImportStart(null);
	}

	internal static bool PendingCanApply(LaunchPlan plan, FriendModEvaluation? evaluation = null)
	{
		if (_pending == null || ActiveDownloadCount > 0 || _downloadsWaitFor != null || !PendingHasInstalledMod(plan))
		{
			return false;
		}
		if (evaluation == null)
		{
			evaluation = new FriendModEvaluation(current: true);
		}
		return _pending.List.Mods.All((FriendModEntry remote) => HasApplicableCopy(plan, remote, evaluation).HasValue);
	}

	internal static bool PendingHasInstalledMod(LaunchPlan plan)
	{
		return _pending?.List.Mods.Any((FriendModEntry remote) => plan.Entries.Any((LaunchEntry entry) => string.Equals(entry.Id, remote.Id, StringComparison.OrdinalIgnoreCase))) ?? false;
	}

	internal static int PendingUnresolvedCount(LaunchPlan plan, FriendModEvaluation? evaluation = null)
	{
		if (_pending == null)
		{
			return 0;
		}
		if (evaluation == null)
		{
			evaluation = new FriendModEvaluation(current: true);
		}
		return _pending.List.Mods.Count((FriendModEntry remote) => HasApplicableCopy(plan, remote, evaluation) == false);
	}

	internal static LaunchProfile? PendingProfile(LaunchPlan plan)
	{
		if (_pending == null)
		{
			return null;
		}
		FriendModEvaluation friendModEvaluation = new FriendModEvaluation(current: true);
		List<ProfileMod> list = new List<ProfileMod>();
		List<LaunchEntry> list2 = new List<LaunchEntry>();
		foreach (FriendModEntry remote in _pending.List.Mods)
		{
			List<LaunchEntry> candidates = plan.Entries.Where((LaunchEntry entry) => string.Equals(entry.Id, remote.Id, StringComparison.OrdinalIgnoreCase)).ToList();
			LaunchEntry launchEntry = FindApplicableCopy(candidates, remote, friendModEvaluation);
			if (friendModEvaluation.Checking)
			{
				return null;
			}
			if (launchEntry == null)
			{
				launchEntry = (from entry in ImportCandidates(candidates, remote)
					orderby string.Equals(entry.Mod.manifest?.version, remote.Version, StringComparison.OrdinalIgnoreCase) descending
					select entry).FirstOrDefault();
			}
			if (launchEntry != null)
			{
				list2.Add(launchEntry);
				list.Add(new ProfileMod
				{
					Id = remote.Id,
					Source = ((remote.WorkshopId != 0) ? $"workshop:{remote.WorkshopId}" : remote.Source),
					Enabled = true,
					PreferredPhysicalKey = launchEntry.Key
				});
			}
		}
		Dictionary<string, LaunchEntry> providers = plan.Entries.GroupBy<LaunchEntry, string>((LaunchEntry entry) => entry.Id, StringComparer.Ordinal).ToDictionary<IGrouping<string, LaunchEntry>, string, LaunchEntry>((IGrouping<string, LaunchEntry> group) => group.Key, (IGrouping<string, LaunchEntry> group) => group.FirstOrDefault((LaunchEntry entry) => entry.Enabled) ?? group.First(), StringComparer.Ordinal);
		foreach (LaunchEntry item in list2)
		{
			providers[item.Id] = item;
		}
		Dictionary<string, bool> loadable = new Dictionary<string, bool>(StringComparer.Ordinal);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Enabled = CanLoad(list2[i].Id, new HashSet<string>(StringComparer.Ordinal));
		}
		HashSet<string> selectedIds;
		Dictionary<string, LaunchEntry> localById;
		if (_pending.GameplayOnly)
		{
			HashSet<string> hashSet = list.Select((ProfileMod m) => m.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			selectedIds = (from m in list
				where m.Enabled
				select m.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			localById = plan.Entries.Where((LaunchEntry e) => e.Enabled).GroupBy<LaunchEntry, string>((LaunchEntry e) => e.Id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, LaunchEntry>, string, LaunchEntry>((IGrouping<string, LaunchEntry> g) => g.Key, (IGrouping<string, LaunchEntry> g) => g.First(), StringComparer.OrdinalIgnoreCase);
			foreach (LaunchEntry item2 in plan.Entries.Where(delegate(LaunchEntry e)
			{
				if (e.Enabled && !e.IsManager)
				{
					ModManifest manifest = e.Mod.manifest;
					if (manifest == null)
					{
						return false;
					}
					return !manifest.affectsGameplay;
				}
				return false;
			}))
			{
				if (!hashSet.Contains(item2.Id) && CanKeep(item2, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) && selectedIds.Add(item2.Id))
				{
					ProfileMod obj = new ProfileMod
					{
						Id = item2.Id
					};
					ulong num = WorkshopUpdates.ItemIdFor(item2.Mod);
					obj.Source = ((num != 0) ? $"workshop:{num}" : "l");
					obj.Enabled = true;
					obj.PreferredPhysicalKey = item2.Key;
					list.Add(obj);
				}
			}
		}
		return new LaunchProfile
		{
			Name = (string.IsNullOrWhiteSpace(_pending.FriendName) ? MlmLocManager.Get("friendSync.profile.defaultName") : _pending.FriendName),
			Mods = list
		};
		bool CanKeep(LaunchEntry candidate, HashSet<string> visited)
		{
			if (selectedIds.Contains(candidate.Id) || candidate.IsManager || !visited.Add(candidate.Id))
			{
				return true;
			}
			ModManifest manifest2 = candidate.Mod.manifest;
			if (manifest2 == null || manifest2.affectsGameplay)
			{
				return false;
			}
			LaunchEntry value3;
			return candidate.Mod.manifest?.dependencies?.All((ModDependency d) => d.id != null && (selectedIds.Contains(d.id) || (localById.TryGetValue(d.id, out value3) && CanKeep(value3, visited)))) ?? true;
		}
		bool CanLoad(string id, HashSet<string> visiting)
		{
			if (loadable.TryGetValue(id, out var value))
			{
				return value;
			}
			if (!providers.TryGetValue(id, out LaunchEntry value2))
			{
				return loadable[id] = false;
			}
			if (!visiting.Add(id))
			{
				return true;
			}
			bool flag2 = value2.Mod.manifest?.dependencies?.All((ModDependency dependency) => dependency.id == null || CanLoad(dependency.id, visiting)) ?? true;
			visiting.Remove(id);
			return loadable[id] = flag2;
		}
	}

	internal static void CompletePending()
	{
		_pending = null;
		_downloadsWaitFor = null;
		DownloadQueue.Clear();
		FriendImportStore.Clear();
		Notify();
	}

	internal static void CancelPending()
	{
		_pending = null;
		_downloadsWaitFor = null;
		DownloadQueue.Clear();
		FriendImportStore.Clear();
		Notify();
	}

	internal static int RetryFailedDownloads(LaunchPlan plan)
	{
		if (_pending == null)
		{
			return 0;
		}
		return DownloadQueue.RetryFailed((WorkshopDownloadJob job) => !plan.Entries.Any((LaunchEntry entry) => WorkshopUpdates.ItemIdFor(entry.Mod) == job.ItemId));
	}

	private static PublicationSnapshot Capture(LaunchPlan plan, string gameVersion)
	{
		LaunchEntry launchEntry = plan.Entries.FirstOrDefault((LaunchEntry entry) => entry.IsManager);
		List<LaunchEntry> source = plan.Entries.Where((LaunchEntry entry) => entry.Enabled && !entry.IsManager).ToList();
		return new PublicationSnapshot(new FriendModList
		{
			GameVersion = gameVersion,
			ManagerVersion = (launchEntry?.Mod.manifest?.version ?? string.Empty),
			Mods = source.Select((LaunchEntry entry) => new FriendModEntry
			{
				Id = entry.Id,
				Name = (string.Equals(entry.Name, entry.Id, StringComparison.Ordinal) ? null : entry.Name),
				Version = (entry.Mod.manifest?.version ?? string.Empty),
				WorkshopId = WorkshopUpdates.ItemIdFor(entry.Mod),
				Source = (((int)entry.Mod.modSource == 2) ? "w" : "l"),
				AffectsGameplay = (entry.Mod.manifest?.affectsGameplay ?? false),
				Dependencies = entry.Mod.manifest?.dependencies?.Select((ModDependency d) => d.id).OfType<string>().ToArray()
			}).ToList()
		}, source.Select((LaunchEntry entry) => entry.Mod.path).ToList());
	}

	private static void RefreshFriends()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (_sampleSaved != null)
		{
			return;
		}
		try
		{
			List<OnlineFriendMods> list = new List<OnlineFriendMods>();
			HashSet<ulong> hashSet = new HashSet<ulong>();
			int friendCount = SteamFriends.GetFriendCount((EFriendFlags)4);
			FriendGameInfo_t val = default(FriendGameInfo_t);
			for (int i = 0; i < friendCount; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, (EFriendFlags)4);
				if ((int)SteamFriends.GetFriendPersonaState(friendByIndex) == 0 || !SteamFriends.GetFriendGamePlayed(friendByIndex, ref val) || (ulong)((CGameID)(ref val.m_gameID)).AppID().m_AppId != 2868840)
				{
					continue;
				}
				hashSet.Add(friendByIndex.m_SteamID);
				if (RichPresenceRequested.Add(friendByIndex.m_SteamID))
				{
					SteamFriends.RequestFriendRichPresence(friendByIndex);
				}
				if (!FriendPresenceCodec.IsManagerScreen(SteamFriends.GetFriendRichPresence(friendByIndex, "mlm_screen")))
				{
					continue;
				}
				string friendPersonaName = SteamFriends.GetFriendPersonaName(friendByIndex);
				string friendRichPresence = SteamFriends.GetFriendRichPresence(friendByIndex, "mlm_mods");
				if (string.IsNullOrWhiteSpace(friendRichPresence))
				{
					list.Add(new OnlineFriendMods(friendByIndex.m_SteamID, friendPersonaName, FriendPresenceState.NotSharing, null, string.Empty, null));
					continue;
				}
				if (!FriendPresenceCodec.TryGetChunkCount(friendRichPresence, out int chunkCount, out FriendSyncError error))
				{
					list.Add(new OnlineFriendMods(friendByIndex.m_SteamID, friendPersonaName, FriendPresenceState.Invalid, null, friendRichPresence, error));
					continue;
				}
				List<string> list2 = new List<string>(chunkCount);
				for (int j = 0; j < chunkCount; j++)
				{
					list2.Add(SteamFriends.GetFriendRichPresence(friendByIndex, FriendPresenceCodec.ChunkKey(j)));
				}
				FriendModList list3;
				FriendSyncError error2;
				bool flag = FriendPresenceCodec.TryDecode(friendRichPresence, list2, out list3, out error2);
				list.Add(new OnlineFriendMods(friendByIndex.m_SteamID, friendPersonaName, (!flag) ? FriendPresenceState.Invalid : FriendPresenceState.Ready, list3, friendRichPresence, error2));
			}
			RichPresenceRequested.IntersectWith(hashSet);
			list = list.OrderBy((OnlineFriendMods friend) => (friend.State != FriendPresenceState.Ready) ? 1 : 0).ThenBy<OnlineFriendMods, string>((OnlineFriendMods friend) => friend.PersonaName, StringComparer.CurrentCultureIgnoreCase).ToList();
			string text = FriendSignature(OnlineFriends);
			string text2 = FriendSignature(list);
			if (!(text == text2))
			{
				OnlineFriends.Clear();
				OnlineFriends.AddRange(list);
				Notify();
			}
		}
		catch (Exception value)
		{
			_availabilityError = FriendSyncError.Create("friendSync.error.refreshUnexpected");
			GD.PushWarning($"{"[ModLaunchManager]"} friend list refresh failed: {value}");
			Notify();
		}
	}

	private static string FriendSignature(IEnumerable<OnlineFriendMods> friends)
	{
		return string.Join('\n', friends.Select((OnlineFriendMods friend) => $"{friend.SteamId}|{friend.PersonaName}|{friend.State}|{friend.Header}|{friend.Error?.Key}"));
	}

	private static void PublishManagerScreen()
	{
		try
		{
			_managerScreenPublished = SteamFriends.SetRichPresence("mlm_screen", "manager");
			if (!_managerScreenPublished)
			{
				SetPublishError(FriendSyncError.Create("friendSync.error.steamScreenRejected"));
			}
		}
		catch (Exception value)
		{
			_managerScreenPublished = false;
			SetPublishError(FriendSyncError.Create("friendSync.error.steamScreenRejected"));
			GD.PushWarning($"{"[ModLaunchManager]"} could not publish manager-screen presence: {value}");
		}
	}

	private static void ClearPublishedPresence()
	{
		if (!_available && !_managerScreenPublished && string.IsNullOrEmpty(_lastPublishedHeader))
		{
			return;
		}
		try
		{
			SteamFriends.SetRichPresence("mlm_screen", string.Empty);
			SteamFriends.SetRichPresence("mlm_mods", string.Empty);
			for (int i = 0; i < 15; i++)
			{
				SteamFriends.SetRichPresence(FriendPresenceCodec.ChunkKey(i), string.Empty);
			}
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not clear friend-sync presence: {value}");
		}
		finally
		{
			_managerScreenPublished = false;
			_lastPublishedChunks = 0;
			_lastPublishedHeader = string.Empty;
		}
	}

	private static void QueuePendingDownloads(LaunchPlan plan)
	{
		if (_pending == null)
		{
			_downloadsWaitFor = null;
			return;
		}
		FriendModEvaluation evaluation = new FriendModEvaluation(current: true);
		bool flag = false;
		foreach (FriendModEntry remote in _pending.List.Mods)
		{
			if (remote.WorkshopId == 0L)
			{
				continue;
			}
			List<LaunchEntry> list = plan.Entries.Where((LaunchEntry entry) => string.Equals(entry.Id, remote.Id, StringComparison.OrdinalIgnoreCase)).ToList();
			if (list.Count > 0)
			{
				bool? flag2 = HasApplicableCopy(list, remote, evaluation);
				if (flag2 != false)
				{
					flag |= !flag2.HasValue;
					continue;
				}
			}
			bool flag3 = list.Any((LaunchEntry local) => WorkshopUpdates.ItemIdFor(local.Mod) == remote.WorkshopId);
			DownloadQueue.Enqueue(remote.WorkshopId, remote.DisplayName, !flag3);
		}
		_downloadsWaitFor = (flag ? plan : null);
	}

	private static bool? MatchesRemoteContent(LaunchEntry local, FriendModEntry remote, FriendModEvaluation evaluation)
	{
		if (!string.IsNullOrWhiteSpace(remote.Version) && !string.Equals(local.Mod.manifest?.version, remote.Version, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return evaluation.ContentMatches(local, remote);
	}

	private static bool? HasApplicableCopy(LaunchPlan plan, FriendModEntry remote, FriendModEvaluation evaluation)
	{
		return HasApplicableCopy(plan.Entries.Where((LaunchEntry entry) => string.Equals(entry.Id, remote.Id, StringComparison.OrdinalIgnoreCase)), remote, evaluation);
	}

	private static bool? HasApplicableCopy(IEnumerable<LaunchEntry> candidates, FriendModEntry remote, FriendModEvaluation evaluation)
	{
		bool? result = false;
		foreach (LaunchEntry item in ImportCandidates(candidates, remote))
		{
			bool? flag = MatchesRemoteContent(item, remote, evaluation);
			if (flag == true)
			{
				return true;
			}
			if (!flag.HasValue)
			{
				result = null;
			}
		}
		return result;
	}

	private static LaunchEntry? FindApplicableCopy(IEnumerable<LaunchEntry> candidates, FriendModEntry remote, FriendModEvaluation evaluation)
	{
		foreach (LaunchEntry item in ImportCandidates(candidates, remote))
		{
			bool? flag = MatchesRemoteContent(item, remote, evaluation);
			if (flag.HasValue)
			{
				if (flag == true)
				{
					return item;
				}
				continue;
			}
			return null;
		}
		return null;
	}

	private static IEnumerable<LaunchEntry> ImportCandidates(IEnumerable<LaunchEntry> candidates, FriendModEntry remote)
	{
		List<LaunchEntry> source = candidates.ToList();
		if (remote.WorkshopId != 0)
		{
			LaunchEntry launchEntry = source.FirstOrDefault((LaunchEntry entry) => WorkshopUpdates.ItemIdFor(entry.Mod) == remote.WorkshopId);
			if (launchEntry != null)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(launchEntry);
			}
		}
		return source.OrderByDescending((LaunchEntry entry) => entry.Enabled);
	}

	internal static string DescribeDownloadError(WorkshopDownloadJob job)
	{
		return MlmLocManager.Get((job.Failure == WorkshopDownloadFailure.Timeout) ? "friendSync.error.downloadTimeout" : "friendSync.error.downloadUnexpected");
	}

	private static void SetPublishError(FriendSyncError? error)
	{
		if (!string.Equals(ErrorSignature(_publishError), ErrorSignature(error), StringComparison.Ordinal))
		{
			_publishError = error;
			Notify();
		}
	}

	internal static string? DescribeError(FriendSyncError? error)
	{
		if (error == null)
		{
			return null;
		}
		return MlmLocManager.Get(error.Key, error.Arguments.Select<KeyValuePair<string, object>, (string, object)>((KeyValuePair<string, object> pair) => (Key: pair.Key, Value: pair.Value)).ToArray());
	}

	private static string ErrorSignature(FriendSyncError? error)
	{
		if (!(error == null))
		{
			return error.Key + "|" + string.Join('|', from pair in error.Arguments
				orderby pair.Key
				select pair.Value);
		}
		return string.Empty;
	}

	private static void Notify()
	{
		try
		{
			_onChanged?.Invoke();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} friend-sync UI update failed: {value}");
		}
	}
}
