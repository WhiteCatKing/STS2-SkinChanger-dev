using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class WorkshopBranches
{
	private static readonly Dictionary<Mod, string> SourcePaths = new Dictionary<Mod, string>();

	private static readonly Dictionary<ulong, WorkshopBranchStatus> Items = new Dictionary<ulong, WorkshopBranchStatus>();

	private static readonly StringComparer PathComparer = (OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

	private static readonly Dictionary<string, (DateTime Stamp, long Length)> CatalogStamps = new Dictionary<string, (DateTime, long)>(PathComparer);

	private static Dictionary<ulong, WorkshopBranchSelection> _selections = new Dictionary<ulong, WorkshopBranchSelection>();

	private static SteamWorkshopNativeDownload? _downloader;

	private static string _root = string.Empty;

	private static string? _clientPath;

	private static bool? _nativeAvailable;

	private static bool _storageFailed;

	private static Action<WorkshopBranchStatus?>? _onChanged;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	internal static bool IsBusy => Items.Values.Any((WorkshopBranchStatus item) => item.Busy);

	internal static bool DownloadPending
	{
		get
		{
			if (!IsBusy)
			{
				return _downloader?.HasPendingDownloads ?? false;
			}
			return true;
		}
	}

	internal static int DownloadingCount => Items.Values.Count((WorkshopBranchStatus item) => item.Busy);

	internal static bool CanDownload
	{
		get
		{
			if (_nativeAvailable.HasValue)
			{
				return _nativeAvailable.Value;
			}
			try
			{
				_clientPath = SteamWorkshopNativeDownload.LoadedClientPath();
				bool? flag = (_nativeAvailable = _clientPath != null && SteamWorkshopNativeDownload.IsVerifiedClient(_clientPath));
				return flag.Value;
			}
			catch (Exception ex)
			{
				GD.PushWarning("[ModLaunchManager] branch downloader unavailable: " + ex.Message);
				bool? flag = (_nativeAvailable = false);
				return flag.Value;
			}
		}
	}

	internal static string SourcePathFor(Mod mod)
	{
		return SourcePaths.GetValueOrDefault(mod) ?? mod.path;
	}

	internal static WorkshopBranchStatus? For(Mod mod)
	{
		return Items.GetValueOrDefault(WorkshopUpdates.ParseItemId(SourcePathFor(mod)));
	}

	internal static bool IsPinned(ulong itemId)
	{
		return Items.GetValueOrDefault(itemId)?.Active != null;
	}

	internal static bool BlocksLaunch(Mod mod)
	{
		return For(mod)?.RestoreFailed ?? false;
	}

	internal static WorkshopBranchInfo InfoFor(Mod mod)
	{
		WorkshopStatus workshopStatus = WorkshopUpdates.For(mod);
		return WorkshopBranchInfo.Resolve(For(mod)?.Catalog, workshopStatus?.AvailableBranches, workshopStatus?.AllBranchesUnrestricted ?? false);
	}

	internal static void Prepare(IReadOnlyList<Mod> mods, Action<WorkshopBranchStatus?> onChanged)
	{
		Dictionary<Mod, string> dictionary = mods.ToDictionary((Mod mod) => mod, SourcePathFor);
		Stop();
		Items.Clear();
		CatalogStamps.Clear();
		SourcePaths.Clear();
		_storageFailed = false;
		_nativeAvailable = null;
		_selections = new Dictionary<ulong, WorkshopBranchSelection>();
		_onChanged = onChanged;
		_root = string.Empty;
		try
		{
			_root = LauncherDataPaths.AccountWorkshop(CanonicalLaunchStore.LegacyPath);
			string path = Path.Combine(_root, "selections.json");
			_selections = (File.Exists(path) ? (JsonSerializer.Deserialize<Dictionary<ulong, WorkshopBranchSelection>>(File.ReadAllText(path, Encoding.UTF8)) ?? throw new InvalidDataException("Empty branch selections")) : new Dictionary<ulong, WorkshopBranchSelection>());
		}
		catch (Exception value)
		{
			_storageFailed = true;
			GD.PushWarning($"{"[ModLaunchManager]"} could not read Workshop branch selections: {value}");
		}
		foreach (Mod mod in mods)
		{
			Register(mod, dictionary[mod]);
		}
		RefreshCatalogs();
		foreach (WorkshopBranchStatus value2 in Items.Values)
		{
			Restore(value2);
		}
	}

	internal static WorkshopBranchStatus? ForItem(ulong id)
	{
		return Items.GetValueOrDefault(id);
	}

	internal static void AddDiscoveredMods(IEnumerable<Mod> mods)
	{
		HashSet<WorkshopBranchStatus> hashSet = new HashSet<WorkshopBranchStatus>();
		foreach (Mod mod in mods)
		{
			WorkshopBranchStatus workshopBranchStatus = Register(mod, SourcePathFor(mod));
			if (workshopBranchStatus != null)
			{
				hashSet.Add(workshopBranchStatus);
				CatalogStamps.Remove(workshopBranchStatus.CacheFile);
				if (workshopBranchStatus.Active != null)
				{
					workshopBranchStatus.RestoreFailed = true;
				}
			}
		}
		RefreshCatalogs();
		foreach (WorkshopBranchStatus item in hashSet.Where((WorkshopBranchStatus item) => !item.Busy))
		{
			Restore(item);
		}
	}

	private static WorkshopBranchStatus? Register(Mod mod, string sourcePath)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)mod.modSource != 2)
		{
			return null;
		}
		ulong num = WorkshopUpdates.ParseItemId(sourcePath);
		if (num != 0L)
		{
			(string, string)? tuple = SteamWorkshopCache.Locate(sourcePath, 2868840u, num);
			if (tuple.HasValue)
			{
				(string, string) valueOrDefault = tuple.GetValueOrDefault();
				SourcePaths[mod] = sourcePath;
				if (!Items.TryGetValue(num, out WorkshopBranchStatus value))
				{
					Dictionary<ulong, WorkshopBranchStatus> items = Items;
					WorkshopBranchStatus obj = new WorkshopBranchStatus
					{
						ItemId = num,
						CacheFile = valueOrDefault.Item1,
						ItemRoot = valueOrDefault.Item2
					};
					value = obj;
					items[num] = obj;
				}
				if (value.Mods.Contains(mod))
				{
					return null;
				}
				value.Mods.Add(mod);
				return value;
			}
		}
		return null;
	}

	private static void Restore(WorkshopBranchStatus item)
	{
		if (_storageFailed)
		{
			item.ErrorKey = "workshop.switch.settingsUnavailable";
		}
		else
		{
			if (!_selections.TryGetValue(item.ItemId, out WorkshopBranchSelection value) || item.Locked)
			{
				return;
			}
			try
			{
				string branch = value.Branch;
				bool flag = ((branch == "public" || branch == "public-beta") ? true : false);
				if (!flag || value.Version.ManifestId == 0L || !value.Version.Supports(value.Branch))
				{
					throw new InvalidDataException("Invalid stored Workshop branch");
				}
				Activate(ParseFiles(item, WorkshopBranchFiles.CacheFolder(_root, item.ItemId, value.Folder), value.Version));
				item.Active = value;
				item.RestoreFailed = false;
				item.ErrorKey = null;
			}
			catch (Exception value2)
			{
				item.RestoreFailed = true;
				item.ErrorKey = "workshop.switch.restoreFailed";
				GD.PushWarning($"{"[ModLaunchManager]"} could not restore Workshop branch for {item.ItemId}: {value2}");
			}
		}
	}

	internal static void RefreshCatalogs()
	{
		foreach (IGrouping<string, WorkshopBranchStatus> item in Items.Values.GroupBy<WorkshopBranchStatus, string>((WorkshopBranchStatus item) => item.CacheFile, PathComparer))
		{
			try
			{
				FileInfo fileInfo = new FileInfo(item.Key);
				(DateTime, long) tuple = (fileInfo.LastWriteTimeUtc, fileInfo.Length);
				if (!CatalogStamps.TryGetValue(item.Key, out (DateTime, long) value))
				{
					goto IL_00a6;
				}
				(DateTime, long) tuple2 = value;
				(DateTime, long) tuple3 = tuple;
				if (!(tuple2.Item1 == tuple3.Item1) || tuple2.Item2 != tuple3.Item2)
				{
					goto IL_00a6;
				}
				goto end_IL_0045;
				IL_00a6:
				IReadOnlyDictionary<ulong, WorkshopItemVersions> dictionary = SteamWorkshopCache.Read(item.Key);
				foreach (WorkshopBranchStatus item2 in item)
				{
					item2.Catalog = dictionary.GetValueOrDefault(item2.ItemId);
				}
				CatalogStamps[item.Key] = tuple;
				end_IL_0045:;
			}
			catch (Exception ex)
			{
				GD.PushWarning("[ModLaunchManager] could not refresh Workshop branch snapshots: " + ex.Message);
			}
		}
	}

	internal static PlanResult Switch(Mod mod, string branch)
	{
		WorkshopBranchStatus item = For(mod);
		if (item == null || item.Busy || item.Locked || _storageFailed)
		{
			return PlanResult.Error(MlmLocManager.Get("workshop.switch.unavailable"));
		}
		if (((branch == null || branch.Length != 0) && !(branch == "public") && !(branch == "public-beta")) || 1 == 0)
		{
			return PlanResult.Error(MlmLocManager.Get("workshop.switch.versionUnavailable"));
		}
		if (branch.Length > 0 && DownloadPending)
		{
			return PlanResult.Error(MlmLocManager.Get("workshop.switch.nativeBusy"));
		}
		item.ErrorKey = null;
		try
		{
			RefreshCatalogs();
			if (branch.Length == 0)
			{
				WorkshopStatus? workshopStatus = WorkshopUpdates.For(mod);
				if (workshopStatus != null && workshopStatus.State == WorkshopUpdateState.Downloading)
				{
					return PlanResult.Error(MlmLocManager.Get("workshop.switch.waitForSteam"));
				}
				List<(Mod Live, Mod Parsed)> mods = ParseFiles(item, item.ItemRoot, item.Catalog?.Versions.FirstOrDefault((WorkshopItemVersion version) => version.ManifestId == item.Catalog.InstalledManifest));
				SaveSelection(item.ItemId, null);
				Activate(mods);
				item.Active = null;
				item.RestoreFailed = false;
				Notify(item);
				return PlanResult.Ok(MlmLocManager.Get("workshop.switch.followingGame"));
			}
			if (item.Catalog?.SharedPublicVersion != null)
			{
				return PlanResult.Ok(MlmLocManager.Get("workshop.switch.sharedVersion"));
			}
			if (!CanDownload || !WorkshopUpdates.IsAvailable)
			{
				return PlanResult.Error(MlmLocManager.Get("workshop.switch.clientUnsupported"));
			}
			WorkshopItemVersion workshopItemVersion = item.Catalog?.ForBranch(branch);
			if (workshopItemVersion == null)
			{
				return PlanResult.Error(MlmLocManager.Get("workshop.switch.versionUnavailable"));
			}
			string text = Guid.NewGuid().ToString("N");
			string target = WorkshopBranchFiles.CacheFolder(_root, item.ItemId, text);
			Directory.CreateDirectory(target);
			if (_downloader == null)
			{
				_downloader = new SteamWorkshopNativeDownload(_clientPath);
			}
			item.TargetBranch = branch;
			item.RequestFolder = text;
			item.Busy = true;
			item.Elapsed = 0.0;
			WorkshopBranchSelection selection = new WorkshopBranchSelection(branch, workshopItemVersion, text);
			_downloader.Start(2868840u, workshopItemVersion.ManifestId, target, delegate(bool success, string? error)
			{
				Complete(item, selection, target, success, error);
			});
			Notify(item);
			return PlanResult.Ok(MlmLocManager.Get("workshop.switch.downloading", ("branch", branch)));
		}
		catch (Exception value)
		{
			item.Busy = false;
			item.TargetBranch = null;
			item.RequestFolder = null;
			item.ErrorKey = "workshop.switch.failed";
			GD.PushWarning($"{"[ModLaunchManager]"} branch switch failed for {item.ItemId}: {value}");
			return PlanResult.Error(MlmLocManager.Get(item.ErrorKey));
		}
	}

	private static void Complete(WorkshopBranchStatus item, WorkshopBranchSelection selection, string path, bool success, string? error)
	{
		if (!item.Busy || item.RequestFolder != selection.Folder || Items.GetValueOrDefault(item.ItemId) != item)
		{
			return;
		}
		try
		{
			if (!success)
			{
				throw new IOException(error ?? "Steam download failed");
			}
			if (item.Locked)
			{
				throw new InvalidOperationException("Workshop item was already loaded");
			}
			List<(Mod, Mod)> list = ParseFiles(item, path, selection.Version);
			SaveSelection(item.ItemId, selection);
			Activate(list);
			item.Active = selection;
			item.RestoreFailed = false;
			item.ErrorKey = null;
			GD.Print($"{"[ModLaunchManager]"} Workshop item {item.ItemId}: selected {selection.Branch}, manifest {selection.Version.ManifestId}; refreshed {list.Count} mod(s).");
		}
		catch (Exception value)
		{
			item.ErrorKey = "workshop.switch.failed";
			GD.PushWarning($"{"[ModLaunchManager]"} Workshop branch activation failed for {item.ItemId}: {value}");
		}
		item.Busy = false;
		item.TargetBranch = null;
		item.RequestFolder = null;
		Notify(item);
	}

	private static List<(Mod Live, Mod Parsed)> ParseFiles(WorkshopBranchStatus item, string directory, WorkshopItemVersion? version)
	{
		IReadOnlyDictionary<string, string> readOnlyDictionary = WorkshopBranchFiles.Validate(directory, item.Mods.Select((Mod mod) => mod.manifest?.id ?? string.Empty));
		List<(Mod, Mod)> list = new List<(Mod, Mod)>();
		foreach (Mod mod in item.Mods)
		{
			Mod val = ModManifestReader.Read(readOnlyDictionary[mod.manifest.id], (ModSource)2);
			if (val?.manifest != null && !(val.manifest.id != mod.manifest.id))
			{
				List<LocString> errors = val.errors;
				if (errors == null || errors.Count <= 0)
				{
					if (version != null && !version.Supports(ModErrorText.CurrentBranchName()))
					{
						ModErrorText.AddSteamBranchMismatch(val, version.MaxBranch);
					}
					list.Add((mod, val));
					continue;
				}
			}
			throw new InvalidDataException("The downloaded mod manifest could not be read");
		}
		return list;
	}

	private static void Activate(IEnumerable<(Mod Live, Mod Parsed)> mods)
	{
		foreach (var mod in mods)
		{
			Mod item = mod.Live;
			Mod item2 = mod.Parsed;
			item.path = item2.path;
			item.manifest = item2.manifest;
			item.version = null;
			item.errors = item2.errors;
		}
	}

	private static void SaveSelection(ulong itemId, WorkshopBranchSelection? selection)
	{
		Dictionary<ulong, WorkshopBranchSelection> dictionary = new Dictionary<ulong, WorkshopBranchSelection>(_selections);
		if (selection == null)
		{
			dictionary.Remove(itemId);
		}
		else
		{
			dictionary[itemId] = selection;
		}
		Directory.CreateDirectory(_root);
		string text = Path.Combine(_root, "selections.json");
		string text2 = text + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			File.WriteAllText(text2, JsonSerializer.Serialize(dictionary, JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			File.Move(text2, text, overwrite: true);
			_selections = dictionary;
		}
		finally
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
		}
	}

	internal static void Poll(double delta)
	{
		bool flag = !IsBusy && DownloadPending;
		try
		{
			_downloader?.Poll();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} branch download poll failed: {value}");
		}
		if (flag && !DownloadPending)
		{
			Notify(null);
		}
		foreach (WorkshopBranchStatus item in Items.Values.Where((WorkshopBranchStatus item) => item.Busy))
		{
			item.Elapsed += delta;
			if (!(item.Elapsed < 1800.0))
			{
				item.Busy = false;
				item.TargetBranch = null;
				item.RequestFolder = null;
				item.ErrorKey = "workshop.switch.timeout";
				Notify(item);
			}
		}
	}

	private static void Notify(WorkshopBranchStatus? item)
	{
		try
		{
			_onChanged?.Invoke(item);
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} branch UI refresh failed: {value}");
		}
	}

	internal static void Stop()
	{
		_onChanged = null;
		foreach (WorkshopBranchStatus value in Items.Values)
		{
			value.Busy = false;
			value.TargetBranch = null;
			value.RequestFolder = null;
		}
		_downloader?.Dispose();
		_downloader = null;
	}
}
