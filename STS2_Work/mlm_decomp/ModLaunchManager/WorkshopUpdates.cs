using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace ModLaunchManager;

internal static class WorkshopUpdates
{
	private sealed class LiveQueryRegistration(Action releaseHandle) : IDisposable
	{
		private IDisposable? _callback;

		private Action? _releaseHandle = releaseHandle;

		internal void Attach(IDisposable callback)
		{
			_callback = callback;
		}

		internal void Complete()
		{
			_callback = null;
			releaseHandle?.Invoke();
			releaseHandle = null;
		}

		public void Dispose()
		{
			try
			{
				_callback?.Dispose();
			}
			finally
			{
				Complete();
			}
		}
	}

	private const double QueryTimeoutSeconds = 20.0;

	private const double InitialRetrySeconds = 2.0;

	private const double MaximumRetrySeconds = 60.0;

	private const int MaxIdsPerQuery = 50;

	private static readonly Dictionary<ulong, WorkshopStatus> Statuses = new Dictionary<ulong, WorkshopStatus>();

	private static readonly Dictionary<Mod, ulong> ModItems = new Dictionary<Mod, ulong>();

	private static readonly List<LiveQueryRegistration> LiveQueries = new List<LiveQueryRegistration>();

	private static readonly List<ulong> ScanQueue = new List<ulong>();

	private static readonly Stopwatch ScanClock = new Stopwatch();

	private static Action? _onChanged;

	private static int _scanIndex;

	private static int _pendingQueries;

	private static int _queryGeneration;

	private static int _retryAttempt;

	private static double _queryElapsed;

	private static double _retryRemaining;

	private static bool _scanComplete = true;

	private static bool _checking;

	private static bool _checkFailed;

	private static bool _activeQueryFailed;

	private static bool _retryScheduled;

	private static bool _metadataAvailable;

	private static readonly Dictionary<string, string> OfficialTagKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Acts"] = "workshop.tag.acts",
		["Ancients"] = "workshop.tag.ancients",
		["Audio"] = "workshop.tag.audio",
		["Cards"] = "workshop.tag.cards",
		["Characters"] = "workshop.tag.characters",
		["Cosmetics"] = "workshop.tag.cosmetics",
		["Events"] = "workshop.tag.events",
		["Expansion"] = "workshop.tag.expansion",
		["Extensions"] = "workshop.tag.extensions",
		["Humor"] = "workshop.tag.humor",
		["Modifiers"] = "workshop.tag.modifiers",
		["Monsters"] = "workshop.tag.monsters",
		["Potions"] = "workshop.tag.potions",
		["QoL"] = "workshop.tag.qol",
		["Relics"] = "workshop.tag.relics",
		["Rooms"] = "workshop.tag.rooms",
		["Tools & APIs"] = "workshop.tag.toolsApis",
		["Utility"] = "workshop.tag.utility",
		["Misc"] = "workshop.tag.misc"
	};

	internal static bool IsAvailable
	{
		get
		{
			try
			{
				return SteamInitializer.Initialized;
			}
			catch
			{
				return false;
			}
		}
	}

	internal static bool Checking => _checking;

	internal static bool CheckFailed => _checkFailed;

	internal static bool MetadataRefreshing => _pendingQueries > 0;

	internal static bool MetadataRetryScheduled => _retryScheduled;

	internal static bool MetadataAvailable => _metadataAvailable;

	internal static bool RestartNeeded => Statuses.Values.Any((WorkshopStatus status) => status.State == WorkshopUpdateState.RestartRequired);

	internal static int TrackedCount => Statuses.Count;

	internal static IReadOnlyCollection<WorkshopStatus> AllStatuses => Statuses.Values;

	internal static int UpdateCount => Statuses.Values.Count((WorkshopStatus status) => status.HasAvailableUpdate);

	internal static int IncompatibleUpdateCount => Statuses.Values.Count((WorkshopStatus status) => status.HasIncompatibleAvailableUpdate);

	internal static int DownloadingCount => Statuses.Values.Count((WorkshopStatus status) => status.State == WorkshopUpdateState.Downloading && !WorkshopBranches.IsPinned(status.ItemId));

	internal static int UpdatedCount => Statuses.Values.Count((WorkshopStatus status) => status.State == WorkshopUpdateState.Updated);

	internal static int RestartCount => Statuses.Values.Count((WorkshopStatus status) => status.State == WorkshopUpdateState.RestartRequired);

	internal static bool HasPendingScan => !_scanComplete;

	internal static WorkshopStatus? For(Mod mod)
	{
		if (!ModItems.TryGetValue(mod, out var value) || !Statuses.TryGetValue(value, out WorkshopStatus value2))
		{
			return null;
		}
		return value2;
	}

	internal static string TagLabel(WorkshopTag tag)
	{
		if (!OfficialTagKeys.TryGetValue(tag.Name, out string value))
		{
			return tag.DisplayName;
		}
		return MlmLocManager.Get(value);
	}

	internal static string TagSearchText(Mod mod)
	{
		IReadOnlyList<WorkshopTag> readOnlyList = For(mod)?.Tags;
		if (readOnlyList == null || readOnlyList.Count <= 0)
		{
			return string.Empty;
		}
		return string.Join('\n', readOnlyList.SelectMany((WorkshopTag tag) => new string[3]
		{
			tag.Name,
			tag.DisplayName,
			TagLabel(tag)
		}));
	}

	internal static ulong ItemIdFor(Mod mod)
	{
		if (!ModItems.TryGetValue(mod, out var value))
		{
			return ParseItemId(WorkshopBranches.SourcePathFor(mod));
		}
		return value;
	}

	internal static void RefreshLocalizedTitles()
	{
		if (!IsAvailable || Statuses.Count == 0)
		{
			return;
		}
		foreach (WorkshopStatus value in Statuses.Values)
		{
			value.Title = null;
		}
		BeginMetadataQuery();
	}

	internal static void Prepare(IEnumerable<Mod> mods, Action onChanged)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		Stop();
		_onChanged = onChanged;
		if (!IsAvailable)
		{
			return;
		}
		try
		{
			foreach (Mod mod in mods)
			{
				if ((int)mod.modSource != 2)
				{
					continue;
				}
				ulong num = ParseItemId(WorkshopBranches.SourcePathFor(mod));
				if (num != 0L)
				{
					ModItems[mod] = num;
					if (!Statuses.ContainsKey(num))
					{
						Statuses[num] = new WorkshopStatus
						{
							ItemId = num
						};
						ScanQueue.Add(num);
					}
				}
			}
			if (ScanQueue.Count != 0)
			{
				_scanComplete = false;
				_checking = true;
				ScanClock.Restart();
			}
		}
		catch (Exception value)
		{
			_scanComplete = true;
			_checking = false;
			_checkFailed = true;
			GD.PushWarning($"{"[ModLaunchManager]"} workshop update check failed to start: {value}");
		}
	}

	internal static bool ScanChunk(int maxItems)
	{
		if (_scanComplete || !IsAvailable)
		{
			return true;
		}
		try
		{
			return ScanChunkCore(maxItems);
		}
		catch (Exception value)
		{
			_scanComplete = true;
			_checking = false;
			_checkFailed = true;
			GD.PushWarning($"{"[ModLaunchManager]"} workshop scan failed: {value}");
			Notify();
			return true;
		}
	}

	private static bool ScanChunkCore(int maxItems)
	{
		int num = Math.Min(_scanIndex + maxItems, ScanQueue.Count);
		while (_scanIndex < num)
		{
			ReadLocalState(Statuses[ScanQueue[_scanIndex]]);
			_scanIndex++;
		}
		if (_scanIndex < ScanQueue.Count)
		{
			return false;
		}
		_scanComplete = true;
		_checking = false;
		ScanClock.Stop();
		GD.Print($"{"[ModLaunchManager]"} workshop update check finished in {ScanClock.ElapsedMilliseconds} ms: {UpdateCount} update(s), {Statuses.Values.Count((WorkshopStatus status) => status.State == WorkshopUpdateState.UpToDate)} up to date.");
		DumpDiagnostics();
		BeginMetadataQuery();
		Notify();
		return true;
	}

	internal static void Poll(double delta)
	{
		if (!IsAvailable || Statuses.Count == 0)
		{
			return;
		}
		try
		{
			PollCore(delta);
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] workshop poll failed: " + ex.Message);
		}
	}

	internal static void Forget(ulong itemId)
	{
		Statuses.Remove(itemId);
		Mod[] array = (from pair in ModItems
			where pair.Value == itemId
			select pair.Key).ToArray();
		foreach (Mod key in array)
		{
			ModItems.Remove(key);
		}
	}

	internal static bool StartDownload(ulong itemId)
	{
		if (!IsAvailable)
		{
			return false;
		}
		try
		{
			return StartDownloadCore(itemId);
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not start workshop download: " + ex.Message);
			return false;
		}
	}

	internal static int StartAllDownloads()
	{
		int num = 0;
		foreach (ulong item in (from pair in Statuses
			where pair.Value.State == WorkshopUpdateState.UpdateAvailable && !WorkshopBranches.IsPinned(pair.Key) && !pair.Value.IsLatestBranchUnsupported
			select pair.Key).ToList())
		{
			if (StartDownload(item))
			{
				num++;
			}
		}
		return num;
	}

	internal static void Stop()
	{
		CancelLiveQueries();
		Statuses.Clear();
		ModItems.Clear();
		ScanQueue.Clear();
		ScanClock.Reset();
		_onChanged = null;
		_scanIndex = 0;
		_scanComplete = true;
		_pendingQueries = 0;
		_queryGeneration++;
		_retryAttempt = 0;
		_queryElapsed = 0.0;
		_retryRemaining = 0.0;
		_checking = false;
		_checkFailed = false;
		_activeQueryFailed = false;
		_retryScheduled = false;
		_metadataAvailable = false;
	}

	internal static ulong ParseItemId(string? path)
	{
		return PhysicalModIdentity.ParseWorkshopItemId(path);
	}

	private static void ReadLocalState(WorkshopStatus status)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t val = new PublishedFileId_t(status.ItemId);
		ulong localSize = default(ulong);
		string text = default(string);
		uint localUpdated = default(uint);
		if (SteamUGC.GetItemInstallInfo(val, ref localSize, ref text, 1024u, ref localUpdated))
		{
			status.LocalUpdated = localUpdated;
			status.LocalSize = localSize;
		}
		EItemState val2 = (EItemState)SteamUGC.GetItemState(val);
		if (((Enum)val2).HasFlag((Enum)(object)(EItemState)16) || ((Enum)val2).HasFlag((Enum)(object)(EItemState)32))
		{
			status.State = WorkshopUpdateState.Downloading;
		}
		else if (((Enum)val2).HasFlag((Enum)(object)(EItemState)8))
		{
			status.State = WorkshopUpdateState.UpdateAvailable;
		}
		else
		{
			status.State = WorkshopUpdateState.UpToDate;
		}
	}

	private static void BeginMetadataQuery()
	{
		CancelLiveQueries();
		_retryScheduled = false;
		_retryRemaining = 0.0;
		_queryElapsed = 0.0;
		_pendingQueries = 0;
		_activeQueryFailed = false;
		int generation = ++_queryGeneration;
		List<ulong> list = Statuses.Keys.ToList();
		for (int i = 0; i < list.Count; i += 50)
		{
			SendDetailsQuery(list.GetRange(i, Math.Min(50, list.Count - i)), generation);
		}
		if (_pendingQueries == 0)
		{
			CompleteMetadataQuery(generation);
		}
	}

	private static void SendDetailsQuery(List<ulong> itemIds, int generation)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t[] array = ((IEnumerable<ulong>)itemIds).Select((Func<ulong, PublishedFileId_t>)((ulong id) => new PublishedFileId_t(id))).ToArray();
		UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(array, (uint)array.Length);
		if (handle == UGCQueryHandle_t.Invalid)
		{
			_activeQueryFailed = true;
			return;
		}
		SteamUGC.SetLanguage(handle, SteamApiLanguage(MlmLocManager.EffectiveLocale));
		SteamAPICall_t val = SteamUGC.SendQueryUGCRequest(handle);
		if (val == SteamAPICall_t.Invalid)
		{
			SteamUGC.ReleaseQueryUGCRequest(handle);
			_activeQueryFailed = true;
			return;
		}
		LiveQueryRegistration registration = new LiveQueryRegistration(delegate
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			SteamUGC.ReleaseQueryUGCRequest(handle);
		});
		CallResult<SteamUGCQueryCompleted_t> val2 = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)delegate(SteamUGCQueryCompleted_t result, bool ioFailure)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			OnQueryCompleted(result, ioFailure, generation, registration);
		});
		registration.Attach((IDisposable)val2);
		LiveQueries.Add(registration);
		_pendingQueries++;
		try
		{
			val2.Set(val, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		}
		catch
		{
			_pendingQueries--;
			LiveQueries.Remove(registration);
			registration.Dispose();
			_activeQueryFailed = true;
		}
	}

	internal static string SteamApiLanguage(string locale)
	{
		string text = locale.Replace('_', '-').ToLowerInvariant();
		if (text.StartsWith("zh-hant", StringComparison.Ordinal) || text.StartsWith("zh-tw", StringComparison.Ordinal) || text.StartsWith("zh-hk", StringComparison.Ordinal))
		{
			return "tchinese";
		}
		if (text.StartsWith("zh", StringComparison.Ordinal))
		{
			return "schinese";
		}
		return text switch
		{
			"ar" => "arabic", 
			"bg" => "bulgarian", 
			"cs" => "czech", 
			"da" => "danish", 
			"de" => "german", 
			"el" => "greek", 
			"en" => "english", 
			"es-419" => "latam", 
			"es" => "spanish", 
			"fi" => "finnish", 
			"fr" => "french", 
			"hu" => "hungarian", 
			"id" => "indonesian", 
			"it" => "italian", 
			"ja" => "japanese", 
			"ko" => "koreana", 
			"ms" => "malay", 
			"nl" => "dutch", 
			"no" => "norwegian", 
			"pl" => "polish", 
			"pt-br" => "brazilian", 
			"pt" => "portuguese", 
			"ro" => "romanian", 
			"ru" => "russian", 
			"sv" => "swedish", 
			"th" => "thai", 
			"tr" => "turkish", 
			"uk" => "ukrainian", 
			"vi" => "vietnamese", 
			_ => "english", 
		};
	}

	private static void OnQueryCompleted(SteamUGCQueryCompleted_t result, bool ioFailure, int generation, LiveQueryRegistration registration)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (generation != _queryGeneration)
		{
			try
			{
				registration.Complete();
			}
			catch
			{
			}
			LiveQueries.Remove(registration);
			return;
		}
		_pendingQueries = Math.Max(0, _pendingQueries - 1);
		try
		{
			if (ioFailure || (int)result.m_eResult != 1)
			{
				_activeQueryFailed = true;
				GD.PushWarning("[ModLaunchManager] workshop details query failed: " + (ioFailure ? "IO failure" : ((object)(EResult)(ref result.m_eResult)/*cast due to .constrained prefix*/).ToString()));
				return;
			}
			SteamUGCDetails_t val = default(SteamUGCDetails_t);
			for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
			{
				if (SteamUGC.GetQueryUGCResult(result.m_handle, num, ref val) && (int)val.m_eResult == 1 && Statuses.TryGetValue(val.m_nPublishedFileId.m_PublishedFileId, out WorkshopStatus value))
				{
					value.Title = ((SteamUGCDetails_t)(ref val)).m_rgchTitle;
					value.Tags = ReadTags(result.m_handle, num, ((SteamUGCDetails_t)(ref val)).m_rgchTags);
					value.ServerUpdated = val.m_rtimeUpdated;
					value.ServerSize = (ulong)((val.m_nFileSize > 0) ? val.m_nFileSize : 0);
					ReadLatestBranchSupport(result.m_handle, num, value);
					if (value.State == WorkshopUpdateState.UpToDate && value.ServerUpdated != value.DismissedListingUpdated && WorkshopListingUpdate.ShowsNewerFiles(value.LocalUpdated, value.LocalSize, value.ServerUpdated, value.ServerSize, value.AllBranchesUnrestricted))
					{
						value.State = WorkshopUpdateState.UpdateAvailable;
						value.DetectedFromListing = true;
						GD.Print($"{"[ModLaunchManager]"} [workshop] {value.ItemId} update found from the listing before Steam flagged it: localFiles={WorkshopStatus.FormatTime(value.LocalUpdated)} size={value.LocalSize} listing={WorkshopStatus.FormatTime(value.ServerUpdated)} size={value.ServerSize}");
					}
				}
			}
		}
		catch (Exception value2)
		{
			_activeQueryFailed = true;
			GD.PushWarning($"{"[ModLaunchManager]"} workshop details query threw: {value2}");
		}
		finally
		{
			try
			{
				registration.Complete();
			}
			catch
			{
			}
			LiveQueries.Remove(registration);
			if (_pendingQueries == 0)
			{
				CompleteMetadataQuery(generation);
			}
		}
	}

	private static void CompleteMetadataQuery(int generation)
	{
		if (generation != _queryGeneration)
		{
			return;
		}
		if (_activeQueryFailed)
		{
			if (ScheduleMetadataRetry("Steam returned no usable Workshop metadata"))
			{
				Notify();
			}
			return;
		}
		bool num = _retryAttempt > 0;
		_checkFailed = false;
		_metadataAvailable = true;
		_retryAttempt = 0;
		_retryScheduled = false;
		_retryRemaining = 0.0;
		if (num)
		{
			GD.Print("[ModLaunchManager] Workshop metadata query recovered; background data refreshed.");
		}
		Notify();
	}

	private static bool ScheduleMetadataRetry(string reason)
	{
		bool result = _retryAttempt == 0;
		_checkFailed = true;
		_metadataAvailable = false;
		_retryAttempt++;
		_retryRemaining = Math.Min(2.0 * Math.Pow(2.0, _retryAttempt - 1), 60.0);
		_retryScheduled = true;
		GD.PushWarning($"{"[ModLaunchManager]"} {reason}; retrying Workshop metadata in {_retryRemaining:F0}s (attempt {_retryAttempt}).");
		return result;
	}

	private static IReadOnlyList<WorkshopTag> ReadTags(UGCQueryHandle_t handle, uint resultIndex, string? joined)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<WorkshopTag> list = new List<WorkshopTag>();
		try
		{
			ReadEachTag(handle, resultIndex, list);
		}
		catch (Exception ex)
		{
			list.Clear();
			GD.Print("[ModLaunchManager] [workshop] per-tag details unavailable, using the tag list: " + ex.Message);
			string[] array = (joined ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			foreach (string text in array)
			{
				list.Add(new WorkshopTag(text, text));
			}
		}
		return list.DistinctBy<WorkshopTag, string>((WorkshopTag tag) => tag.Name, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ReadEachTag(UGCQueryHandle_t handle, uint resultIndex, List<WorkshopTag> tags)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		uint queryUGCNumTags = SteamUGC.GetQueryUGCNumTags(handle, resultIndex);
		string text = default(string);
		string text2 = default(string);
		for (uint num = 0u; num < queryUGCNumTags; num++)
		{
			if (SteamUGC.GetQueryUGCTag(handle, resultIndex, num, ref text, 256u) && !string.IsNullOrWhiteSpace(text))
			{
				string text3 = ((SteamUGC.GetQueryUGCTagDisplayName(handle, resultIndex, num, ref text2, 256u) && !string.IsNullOrWhiteSpace(text2)) ? text2 : text);
				tags.Add(new WorkshopTag(text.Trim(), text3.Trim()));
			}
		}
	}

	private static void ReadLatestBranchSupport(UGCQueryHandle_t handle, uint resultIndex, WorkshopStatus status)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			uint numSupportedGameVersions = SteamUGC.GetNumSupportedGameVersions(handle, resultIndex);
			if (numSupportedGameVersions == 0)
			{
				status.LatestBranchCompatibility = WorkshopBranchCompatibility.Supported;
				status.LatestBranchRanges = null;
				status.LatestMaxSupportedBranch = null;
				status.AvailableBranches = new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "public", "public-beta" });
				status.AllBranchesUnrestricted = true;
				return;
			}
			List<(string MinBranch, string MaxBranch)> ranges = new List<(string, string)>();
			string text = default(string);
			string text2 = default(string);
			for (uint num = 0u; num < numSupportedGameVersions; num++)
			{
				if (SteamUGC.GetSupportedGameVersionData(handle, resultIndex, num, ref text, ref text2, 999u))
				{
					ranges.Add((text ?? string.Empty, text2 ?? string.Empty));
				}
			}
			if (ranges.Count != numSupportedGameVersions)
			{
				status.LatestBranchCompatibility = WorkshopBranchCompatibility.Unknown;
				status.LatestBranchRanges = null;
				status.LatestMaxSupportedBranch = null;
				status.AvailableBranches = null;
				status.AllBranchesUnrestricted = false;
				_activeQueryFailed = true;
				GD.PushWarning($"{"[ModLaunchManager]"} workshop item {status.ItemId} reported {numSupportedGameVersions} branch range(s), but only {ranges.Count} could be read.");
			}
			else
			{
				ValueTuple<bool, PlatformBranch?> valueTuple = ModManager.EvaluateBranchSupport(PlatformUtil.GetPlatformBranch(), (IReadOnlyList<ValueTuple<string, string>>)ranges);
				bool item = valueTuple.Item1;
				PlatformBranch? item2 = valueTuple.Item2;
				status.LatestBranchCompatibility = (item ? WorkshopBranchCompatibility.Supported : WorkshopBranchCompatibility.Unsupported);
				status.LatestBranchRanges = FormatBranchRanges(ranges);
				status.LatestMaxSupportedBranch = (item2.HasValue ? PlatformBranchExtensions.ToName(item2.GetValueOrDefault()) : null);
				status.AllBranchesUnrestricted = ranges.All<(string, string)>(((string MinBranch, string MaxBranch) range) => string.IsNullOrEmpty(range.MinBranch) && string.IsNullOrEmpty(range.MaxBranch));
				status.AvailableBranches = (from branch in (IEnumerable<PlatformBranch>)(object)new PlatformBranch[2]
					{
						(PlatformBranch)1,
						(PlatformBranch)2
					}
					where ModManager.EvaluateBranchSupport(branch, (IReadOnlyList<ValueTuple<string, string>>)ranges).Item1
					select PlatformBranchExtensions.ToName(branch)).ToArray();
			}
		}
		catch (Exception ex)
		{
			status.LatestBranchCompatibility = WorkshopBranchCompatibility.Unknown;
			status.LatestBranchRanges = null;
			status.LatestMaxSupportedBranch = null;
			status.AvailableBranches = null;
			status.AllBranchesUnrestricted = false;
			_activeQueryFailed = true;
			GD.PushWarning($"{"[ModLaunchManager]"} could not read Steam branch support for workshop item {status.ItemId}: {ex.Message}");
		}
	}

	private static string FormatBranchRanges(IEnumerable<(string MinBranch, string MaxBranch)> ranges)
	{
		return string.Join(", ", ranges.Select<(string, string), string>(delegate((string MinBranch, string MaxBranch) range)
		{
			bool num = !string.IsNullOrWhiteSpace(range.MinBranch);
			bool flag = !string.IsNullOrWhiteSpace(range.MaxBranch);
			bool flag2 = flag;
			if (!num)
			{
				if (!flag2)
				{
					return "*";
				}
				return "<=" + range.MaxBranch;
			}
			if (!flag2)
			{
				return ">=" + range.MinBranch;
			}
			return string.Equals(range.MinBranch, range.MaxBranch, StringComparison.OrdinalIgnoreCase) ? range.MinBranch : (range.MinBranch + ".." + range.MaxBranch);
		}));
	}

	private static void PollCore(double delta)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (_pendingQueries > 0)
		{
			_queryElapsed += delta;
			if (_queryElapsed > 20.0)
			{
				CancelLiveQueries();
				_queryGeneration++;
				_pendingQueries = 0;
				_activeQueryFailed = false;
				flag |= ScheduleMetadataRetry($"Workshop metadata query timed out after {20.0:F0}s");
			}
		}
		if (_retryScheduled)
		{
			_retryRemaining -= delta;
			if (_retryRemaining <= 0.0)
			{
				BeginMetadataQuery();
			}
		}
		PublishedFileId_t val = default(PublishedFileId_t);
		ulong num = default(ulong);
		ulong num2 = default(ulong);
		foreach (WorkshopStatus value in Statuses.Values)
		{
			if (value.State != WorkshopUpdateState.Downloading)
			{
				continue;
			}
			((PublishedFileId_t)(ref val))._002Ector(value.ItemId);
			if (SteamUGC.GetItemDownloadInfo(val, ref num, ref num2) && num2 != 0)
			{
				float num3 = Math.Clamp((float)((double)num / (double)num2), 0f, 1f);
				if (Math.Abs(num3 - value.Progress) > 0.005f)
				{
					value.Progress = num3;
					flag = true;
				}
			}
			EItemState val2 = (EItemState)SteamUGC.GetItemState(val);
			if (((Enum)val2).HasFlag((Enum)(object)(EItemState)16) || ((Enum)val2).HasFlag((Enum)(object)(EItemState)32))
			{
				continue;
			}
			if (((Enum)val2).HasFlag((Enum)(object)(EItemState)8))
			{
				value.State = WorkshopUpdateState.UpdateAvailable;
				value.Progress = 0f;
			}
			else if (value.DetectedFromListing && ListingStillNewer(value, val))
			{
				value.DownloadWait += delta;
				if (value.DownloadWait < 30.0)
				{
					continue;
				}
				GD.PushWarning($"{"[ModLaunchManager]"} workshop item {value.ItemId} did not start downloading within {30.0:F0}s; " + "treating the listing-detected update as up to date.");
				value.DismissedListingUpdated = value.ServerUpdated;
				value.DetectedFromListing = false;
				value.State = WorkshopUpdateState.UpToDate;
				value.Progress = 0f;
			}
			else
			{
				CompleteDownload(value, val);
			}
			flag = true;
		}
		if (flag)
		{
			Notify();
		}
	}

	private static void CancelLiveQueries()
	{
		foreach (LiveQueryRegistration item in LiveQueries.ToList())
		{
			try
			{
				item.Dispose();
			}
			catch
			{
			}
		}
		LiveQueries.Clear();
	}

	private static void CompleteDownload(WorkshopStatus status, PublishedFileId_t itemId)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		status.Progress = 1f;
		status.DetectedFromListing = false;
		ulong localSize = default(ulong);
		string text = default(string);
		uint localUpdated = default(uint);
		if (SteamUGC.GetItemInstallInfo(itemId, ref localSize, ref text, 1024u, ref localUpdated))
		{
			status.LocalUpdated = localUpdated;
			status.LocalSize = localSize;
		}
		Mod val = ModFor(status.ItemId);
		if (val == null)
		{
			status.State = WorkshopUpdateState.RestartRequired;
			status.RefreshError = "the mod entry could not be found";
			return;
		}
		if (WorkshopBranches.IsPinned(status.ItemId))
		{
			status.State = WorkshopUpdateState.UpToDate;
			return;
		}
		status.PreviousVersion = val.manifest?.version;
		if (ModManifestReader.RefreshInPlace(val, out string error))
		{
			ApplyLatestBranchVerdict(val, status);
			status.NewVersion = val.manifest?.version;
			status.State = WorkshopUpdateState.Updated;
			GD.Print($"{"[ModLaunchManager]"} workshop item {status.ItemId} refreshed in place: {val.manifest?.id} {status.PreviousVersion} -> {status.NewVersion}");
		}
		else
		{
			status.State = WorkshopUpdateState.RestartRequired;
			status.RefreshError = error;
			GD.PushWarning($"{"[ModLaunchManager]"} workshop item {status.ItemId} downloaded but its manifest could not be refreshed ({error}); a restart is required.");
		}
	}

	internal static void CompleteMovedDownload(Mod mod)
	{
		try
		{
			if (Statuses.TryGetValue(ItemIdFor(mod), out WorkshopStatus value) && value.State == WorkshopUpdateState.RestartRequired && !WorkshopBranches.IsPinned(value.ItemId))
			{
				ApplyLatestBranchVerdict(mod, value);
				value.NewVersion = mod.manifest?.version;
				value.RefreshError = null;
				value.State = WorkshopUpdateState.Updated;
				GD.Print($"{"[ModLaunchManager]"} workshop item {value.ItemId} refreshed from its new folder: {mod.manifest?.id} {value.PreviousVersion} -> {value.NewVersion}");
				Notify();
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not finish a moved workshop update: " + ex.Message);
		}
	}

	private static void ApplyLatestBranchVerdict(Mod mod, WorkshopStatus status)
	{
		ModErrorText.ClearSteamBranchMismatch(mod);
		if (status.IsLatestBranchUnsupported)
		{
			ModErrorText.AddSteamBranchMismatch(mod, status.LatestMaxSupportedBranch);
		}
	}

	private static Mod? ModFor(ulong itemId)
	{
		return ModItems.FirstOrDefault<KeyValuePair<Mod, ulong>>((KeyValuePair<Mod, ulong> pair) => pair.Value == itemId).Key;
	}

	private static bool StartDownloadCore(ulong itemId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (WorkshopBranches.IsPinned(itemId) || !Statuses.TryGetValue(itemId, out WorkshopStatus value) || !SteamUGC.DownloadItem(new PublishedFileId_t(itemId), true))
		{
			return false;
		}
		value.State = WorkshopUpdateState.Downloading;
		value.Progress = 0f;
		value.DownloadWait = 0.0;
		Mod val = ModFor(itemId);
		WorkshopDownloadCenter.Track(itemId, string.IsNullOrWhiteSpace(val?.manifest?.name) ? (value.Title ?? itemId.ToString()) : val.manifest.name, WorkshopDownloadSource.Update);
		Notify();
		return true;
	}

	private static bool ListingStillNewer(WorkshopStatus status, PublishedFileId_t itemId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		ulong localSize = default(ulong);
		string text = default(string);
		uint localUpdated = default(uint);
		if (!SteamUGC.GetItemInstallInfo(itemId, ref localSize, ref text, 1024u, ref localUpdated))
		{
			return false;
		}
		return WorkshopListingUpdate.ShowsNewerFiles(localUpdated, localSize, status.ServerUpdated, status.ServerSize, status.AllBranchesUnrestricted);
	}

	private static void DumpDiagnostics()
	{
		foreach (KeyValuePair<Mod, ulong> modItem in ModItems)
		{
			if (Statuses.TryGetValue(modItem.Value, out WorkshopStatus value) && value.State != WorkshopUpdateState.UpToDate)
			{
				GD.Print($"{"[ModLaunchManager]"} [workshop] {modItem.Value} state={value.State} id={modItem.Key.manifest?.id} version={modItem.Key.manifest?.version} localFiles={WorkshopStatus.FormatTime(value.LocalUpdated)} listing={WorkshopStatus.FormatTime(value.ServerUpdated)} branch={value.LatestBranchCompatibility} ranges=\"{value.LatestBranchRanges}\" title=\"{value.Title}\"");
			}
		}
	}

	private static void Notify()
	{
		try
		{
			_onChanged?.Invoke();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} workshop UI refresh failed: {value}");
		}
	}

	internal static string StateText(WorkshopStatus status)
	{
		return status.State switch
		{
			WorkshopUpdateState.UpToDate => MlmLocManager.Get("workshop.state.upToDate"), 
			WorkshopUpdateState.UpdateAvailable => MlmLocManager.Get("workshop.state.updateAvailable"), 
			WorkshopUpdateState.Downloading => MlmLocManager.Get("workshop.state.downloading", ("percent", status.Progress * 100f)), 
			WorkshopUpdateState.Updated => (status.NewVersion == null) ? MlmLocManager.Get("workshop.state.updatedActive") : MlmLocManager.Get("workshop.state.updatedVersion", ("previous", status.PreviousVersion ?? "?"), ("current", status.NewVersion)), 
			WorkshopUpdateState.RestartRequired => MlmLocManager.Get("workshop.state.restartRequired"), 
			_ => MlmLocManager.Get("workshop.state.checking"), 
		};
	}

	internal static string LatestBranchText(WorkshopStatus status)
	{
		if (status.State == WorkshopUpdateState.Unknown || status.LatestBranchCompatibility == WorkshopBranchCompatibility.Unknown)
		{
			return MetadataFallbackText();
		}
		switch (status.LatestBranchCompatibility)
		{
		case WorkshopBranchCompatibility.Supported:
			if (status.AllBranchesUnrestricted)
			{
				return MlmLocManager.Get("workshop.branch.noRestriction");
			}
			if (string.IsNullOrWhiteSpace(status.LatestBranchRanges))
			{
				return MlmLocManager.Get("workshop.branch.noRestriction");
			}
			return MlmLocManager.Get("workshop.branch.supported", ("ranges", status.LatestBranchRanges));
		case WorkshopBranchCompatibility.Unsupported:
			return MlmLocManager.Get("workshop.branch.unsupported", ("branch", ModErrorText.CurrentBranchName()), ("ranges", status.LatestBranchRanges ?? MlmLocManager.Get("common.value.unknown")));
		default:
			return MlmLocManager.Get("workshop.state.checking");
		}
	}

	internal static string AvailableBranchesText(WorkshopBranchInfo info)
	{
		return info.Kind switch
		{
			WorkshopBranchKind.AllVersions => MlmLocManager.Get("workshop.branch.noRestriction"), 
			WorkshopBranchKind.SharedPublicVersion => MlmLocManager.Get("workshop.branch.sharedPublicVersion"), 
			WorkshopBranchKind.NoPublicVersion => MlmLocManager.Get("workshop.branch.noPublicBuild"), 
			WorkshopBranchKind.Versioned => string.Join(" / ", info.Branches), 
			_ => MlmLocManager.Get("workshop.branch.unknown"), 
		};
	}

	internal static string MetadataFallbackText()
	{
		if (!MetadataRefreshing)
		{
			if (!MetadataRetryScheduled)
			{
				return "—";
			}
			return MlmLocManager.Get("workshop.metadata.retrying");
		}
		return MlmLocManager.Get("workshop.state.checking");
	}
}
