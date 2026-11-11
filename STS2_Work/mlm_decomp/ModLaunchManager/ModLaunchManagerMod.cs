using System;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

[ModInitializer("Initialize")]
public static class ModLaunchManagerMod
{
	internal const string ModId = "ModLaunchManager";

	internal const string CompatModId = "CrossVersionCompat";

	internal const string LegacyDemoId = "EarlyModManagerDemo";

	internal const string LogPrefix = "[ModLaunchManager]";

	private static Harmony? _harmony;

	public static void Initialize()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		try
		{
			if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.Ordinal))
			{
				GD.Print("[ModLaunchManager] headless startup: skipping the interactive launcher; continuing the game's normal mod loading.");
				return;
			}
			_harmony = new Harmony("ModLaunchManager.Bootstrap");
			LoadOrderGuard.Install(_harmony);
			int num = ModManager.Mods.Count((Mod mod) => (int)mod.state == 1 && !string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.Ordinal));
			if (num > 0)
			{
				InGameManager.Install(_harmony);
				MainMenuModWarningPatch.Install(_harmony);
				BaseLibSourceAliases.Install(_harmony);
				ModTranslations.Install(_harmony);
				LoadOrderGuard.SchedulePromotionForNextRestart(num);
				GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("guard.loadedLate", ("loaded", num)));
				ManagerReturn.ManagerUnavailable();
				return;
			}
			InGameManager.Install(_harmony);
			MainMenuModWarningPatch.Install(_harmony);
			BaseLibSourceAliases.Install(_harmony);
			ModTranslations.Install(_harmony);
			if (!CollectibleModAssemblyLoader.Install(_harmony))
			{
				GD.PushWarning("[ModLaunchManager] runtime DLL rollback is unavailable; load errors will remain paused instead of claiming a successful unload.");
			}
			ModLoadFailures.Install(_harmony);
			if (!SaveProfileSwitcher.Install(_harmony))
			{
				GD.PushWarning("[ModLaunchManager] game save-profile switching is unavailable; continuing with the game's default profile.");
			}
			AutoUnlockAll.Install(_harmony);
			if (!StartupGate.Install(_harmony))
			{
				GD.PushWarning("[ModLaunchManager] could not install the startup gate; continuing normally.");
				ReportUnavailable(MlmLocManager.Get("notice.reason.startupGate"));
			}
			else if (!DeferredModLoader.Install(_harmony))
			{
				StartupGate.FailOpen("the mod-load interception could not be installed");
				ReportUnavailable(MlmLocManager.Get("notice.reason.loadInterception"));
			}
			else
			{
				VanillaLaunch.Install(_harmony);
				GD.Print("[ModLaunchManager] armed before the remaining mod-loading calls.");
			}
		}
		catch (Exception ex)
		{
			DeferredModLoader.DisableDeferral();
			StartupGate.FailOpen("initializer failed: " + ex.GetType().Name + " " + ex.Message);
			GD.PushWarning($"{"[ModLaunchManager]"} initialization failed; continuing normally: {ex}");
			ReportUnavailable(MlmLocManager.Get("notice.reason.initializerFailed"));
		}
	}

	internal static void ReportUnavailable(string reason)
	{
		ManagerReturn.ManagerUnavailable();
		try
		{
			MainMenuNotice.Queue(MlmLocManager.Get("notice.launcherUnavailable.title"), MlmLocManager.Get("notice.launcherUnavailable.body", ("reason", reason)));
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} launcher unavailable ({reason}); the on-screen notice could not be raised either: {ex.Message}");
		}
	}
}
