using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class LocalModWatcher
{
	private static readonly LocalModFileLedger Files = new LocalModFileLedger();

	private static readonly LocalModScanRetry Retry = new LocalModScanRetry();

	private static FileSystemWatcher? _watcher;

	private static SceneTree? _tree;

	private static Func<LocalModScan, bool>? _onChanged;

	private static Action<string>? _onError;

	private static string _rootPath = string.Empty;

	private static string? _watcherError;

	private static long _lastChangeUtcTicks;

	private static int _dirty;

	private static int _removalHolds;

	internal static bool IsRunning => _watcher != null;

	internal static string GetLocalModsPath()
	{
		string directoryName = Path.GetDirectoryName(OS.GetExecutablePath());
		if (!string.IsNullOrWhiteSpace(directoryName))
		{
			return Path.Combine(directoryName, "mods");
		}
		return string.Empty;
	}

	internal static void MarkKnown(IEnumerable<string> manifestFiles)
	{
		foreach (string manifestFile in manifestFiles)
		{
			Files.MarkKnown(Path.GetFullPath(manifestFile));
		}
	}

	internal static void HoldRemovals()
	{
		_removalHolds++;
	}

	internal static void ReleaseRemovals()
	{
		if (_removalHolds > 0 && --_removalHolds == 0)
		{
			MarkDirty();
		}
	}

	internal static void Start(SceneTree tree, Func<LocalModScan, bool> onChanged, Action<string> onError)
	{
		Stop();
		_rootPath = GetLocalModsPath();
		if (string.IsNullOrWhiteSpace(_rootPath) || !Directory.Exists(_rootPath))
		{
			onError(MlmLocManager.Get("watcher.localFolderMissing"));
			return;
		}
		if (!ModManifestReader.IsAvailable)
		{
			onError(MlmLocManager.Get("watcher.manifestReaderMissing"));
			return;
		}
		Files.Reset(EnumerateJsonFiles(_rootPath, out string _) ?? Array.Empty<string>());
		Retry.Succeeded();
		_tree = tree;
		_onChanged = onChanged;
		_onError = onError;
		_watcherError = null;
		Volatile.Write(ref _dirty, 0);
		_watcher = new FileSystemWatcher(_rootPath)
		{
			IncludeSubdirectories = true,
			Filter = "*",
			NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite),
			EnableRaisingEvents = true
		};
		_watcher.Created += OnFileChanged;
		_watcher.Changed += OnFileChanged;
		_watcher.Deleted += OnFileChanged;
		_watcher.Renamed += OnFileRenamed;
		_watcher.Error += OnWatcherError;
		tree.ProcessFrame += Poll;
		GD.Print("[ModLaunchManager] watching local mods at " + _rootPath + ".");
	}

	internal static void Stop()
	{
		if (_tree != null)
		{
			_tree.ProcessFrame -= Poll;
		}
		_tree = null;
		if (_watcher != null)
		{
			_watcher.EnableRaisingEvents = false;
			_watcher.Created -= OnFileChanged;
			_watcher.Changed -= OnFileChanged;
			_watcher.Deleted -= OnFileChanged;
			_watcher.Renamed -= OnFileRenamed;
			_watcher.Error -= OnWatcherError;
			_watcher.Dispose();
			_watcher = null;
		}
		_onChanged = null;
		_onError = null;
		_watcherError = null;
		Retry.Succeeded();
		Volatile.Write(ref _dirty, 0);
	}

	private static void OnFileChanged(object sender, FileSystemEventArgs args)
	{
		MarkDirty();
	}

	private static void OnFileRenamed(object sender, RenamedEventArgs args)
	{
		MarkDirty();
	}

	private static void OnWatcherError(object sender, ErrorEventArgs args)
	{
		_watcherError = args.GetException().Message;
		MarkDirty();
	}

	private static void MarkDirty()
	{
		Volatile.Write(ref _lastChangeUtcTicks, DateTime.UtcNow.Ticks);
		Volatile.Write(ref _dirty, 1);
	}

	private static void Poll()
	{
		if (Volatile.Read(in _dirty) != 0 && DateTime.UtcNow.Ticks - Volatile.Read(in _lastChangeUtcTicks) >= TimeSpan.FromMilliseconds(900L, 0L).Ticks)
		{
			Volatile.Write(ref _dirty, 0);
			if (_watcherError != null)
			{
				string watcherError = _watcherError;
				_watcherError = null;
				_onError?.Invoke(MlmLocManager.Get("watcher.rescanRequired", ("error", watcherError)));
			}
			ScanForChanges();
		}
	}

	private static void ScanForChanges()
	{
		try
		{
			string error;
			string[] array = EnumerateJsonFiles(_rootPath, out error);
			if (array == null)
			{
				RetryLater(error);
				return;
			}
			List<Mod> parsed = new List<Mod>();
			if (Files.Scan((IReadOnlyCollection<string>)(object)array, _removalHolds > 0, FingerprintOf, delegate(string file)
			{
				Mod val = ModManifestReader.Read(file, (ModSource)1);
				if (val == null)
				{
					return false;
				}
				parsed.Add(val);
				return true;
			}, (IReadOnlyList<string> vanished) => _onChanged?.Invoke(new LocalModScan(parsed, vanished)) ?? false) == LocalModScanResult.NotApplied)
			{
				RetryLater(null);
			}
			else
			{
				Retry.Succeeded();
			}
		}
		catch (TargetInvocationException ex)
		{
			Exception ex2 = ex.InnerException ?? ex;
			_onError?.Invoke(MlmLocManager.Get("watcher.scanFailed", ("error", ex2.Message)));
			RetryLater(null);
		}
		catch (Exception ex3)
		{
			_onError?.Invoke(MlmLocManager.Get("watcher.scanFailed", ("error", ex3.Message)));
			RetryLater(null);
		}
	}

	private static void RetryLater(string? listingError)
	{
		bool gaveUp;
		TimeSpan? timeSpan = Retry.Failed(out gaveUp);
		if (timeSpan.HasValue)
		{
			TimeSpan valueOrDefault = timeSpan.GetValueOrDefault();
			Volatile.Write(ref _lastChangeUtcTicks, (DateTime.UtcNow + valueOrDefault).Ticks);
			Volatile.Write(ref _dirty, 1);
		}
		else if (gaveUp && listingError != null)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not list the local mods folder {_rootPath}; not scanning again until it changes: {listingError}");
			_onError?.Invoke(MlmLocManager.Get("watcher.folderUnreadable"));
		}
	}

	private static LocalModFileLedger.Fingerprint? FingerprintOf(string file)
	{
		FileInfo fileInfo = new FileInfo(file);
		if (!fileInfo.Exists)
		{
			return null;
		}
		return new LocalModFileLedger.Fingerprint(fileInfo.Length, fileInfo.LastWriteTimeUtc.Ticks);
	}

	private static string[]? EnumerateJsonFiles(string rootPath, out string? error)
	{
		error = null;
		try
		{
			EnumerationOptions enumerationOptions = new EnumerationOptions
			{
				RecurseSubdirectories = true,
				IgnoreInaccessible = true,
				AttributesToSkip = FileAttributes.None,
				MatchType = MatchType.Win32,
				MatchCasing = MatchCasing.CaseInsensitive
			};
			return Directory.EnumerateFiles(rootPath, "*.json", enumerationOptions).Select(Path.GetFullPath).ToArray();
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return null;
		}
	}
}
