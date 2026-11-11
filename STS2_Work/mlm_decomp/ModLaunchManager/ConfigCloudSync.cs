using System;
using System.Collections.Generic;
using Godot;

namespace ModLaunchManager;

internal static class ConfigCloudSync
{
	private const double FlushDelaySeconds = 2.0;

	internal static readonly IReadOnlyList<CloudSyncedFile> Files = new global::_003C_003Ez__ReadOnlyArray<CloudSyncedFile>(new CloudSyncedFile[4]
	{
		new CloudSyncedFile("mod_launch_manager_settings.json", "mod_launch_manager/settings.json", new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "saveProfileId", "autoUnlockAll", "cloudSync", "acknowledgedUpdateRevision" })),
		new CloudSyncedFile("mod_launch_options.json", "mod_launch_manager/options.json", Array.Empty<string>()),
		new CloudSyncedFile("mod_launch_organization.json", "mod_launch_manager/organization.json", Array.Empty<string>()),
		new CloudSyncedFile("mod_launch_profiles.json", "mod_launch_manager/profiles.json", Array.Empty<string>())
	});

	private static bool _flushScheduled;

	private static bool _dirty;

	private static SceneTreeTimer? _flushTimer;

	internal static ConfigCloudStatus Status { get; private set; } = ConfigCloudStatus.Unknown;

	private static string Root => LauncherDataPaths.CloudRoot;

	internal static event Action? StatusChanged;

	internal static CloudSyncReport? SyncAtStartup()
	{
		ConfigCloudSyncEngine.Log = delegate(string message)
		{
			GD.Print("[ModLaunchManager] " + message);
		};
		ConfigCloudChanges.Changed = OnLocalChanged;
		CloudSyncReport? cloudSyncReport = Run(pushOnly: false);
		if ((object)cloudSyncReport != null && cloudSyncReport.ChangedLocal("mod_launch_manager_settings.json"))
		{
			MlmLocManager.ReloadPreferences();
		}
		if ((object)cloudSyncReport != null && cloudSyncReport.ChangedLocal("mod_launch_organization.json"))
		{
			BaseLibSourceAliases.RefreshAliases(ModOrganizationStore.Load());
		}
		return cloudSyncReport;
	}

	internal static void Flush()
	{
		if (_dirty)
		{
			Run(pushOnly: true);
		}
	}

	internal static void Stop()
	{
		if (_flushTimer != null && GodotObject.IsInstanceValid((GodotObject)(object)_flushTimer))
		{
			_flushTimer.Timeout -= FlushDelayed;
		}
		_flushTimer = null;
		_flushScheduled = false;
		ConfigCloudChanges.Changed = null;
		Flush();
	}

	internal static void Resume()
	{
		_dirty = true;
		Flush();
	}

	private static void OnLocalChanged()
	{
		_dirty = true;
		if (!_flushScheduled)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				_flushScheduled = true;
				_flushTimer = val.CreateTimer(2.0, true, false, false);
				_flushTimer.Timeout += FlushDelayed;
			}
		}
	}

	private static void FlushDelayed()
	{
		_flushTimer = null;
		_flushScheduled = false;
		Flush();
	}

	private static CloudSyncReport? Run(bool pushOnly)
	{
		try
		{
			if (!MlmLocManager.CloudSync)
			{
				_dirty = false;
				SetStatus(ConfigCloudStatus.Off);
				return null;
			}
			ConfigCloudStatus unavailable;
			SteamConfigCloud steamConfigCloud = SteamConfigCloud.TryOpen(out unavailable);
			if (steamConfigCloud == null)
			{
				_dirty = false;
				SetStatus(unavailable);
				return null;
			}
			CloudSyncReport cloudSyncReport = ConfigCloudSyncEngine.Run(Root, steamConfigCloud, Files, pushOnly);
			_dirty = false;
			SetStatus(cloudSyncReport.HasProblems ? ConfigCloudStatus.Failed : ((cloudSyncReport.HasPending && !steamConfigCloud.CanWrite) ? ConfigCloudStatus.WaitingForGameSaves : ConfigCloudStatus.Synced));
			return cloudSyncReport;
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} launcher settings cloud sync failed: {value}");
			SetStatus(ConfigCloudStatus.Failed);
			return null;
		}
	}

	private static void SetStatus(ConfigCloudStatus status)
	{
		if (Status == status)
		{
			return;
		}
		Status = status;
		try
		{
			ConfigCloudSync.StatusChanged?.Invoke();
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] cloud sync status display failed: " + ex.Message);
		}
	}
}
