using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace ModLaunchManager;

internal static class AutoUnlockAll
{
	private static int? _requestedProfileId;

	internal static bool Available { get; private set; }

	internal static void CancelRequest()
	{
		_requestedProfileId = null;
	}

	internal static bool Install(Harmony harmony)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b3: Expected O, but got Unknown
		Available = false;
		_requestedProfileId = null;
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(ProgressSaveManager), "LoadProgress", Array.Empty<Type>(), (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(AutoUnlockAll), "BeforeProgressLoaded", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(AutoUnlockAll), "AfterProgressLoaded", (Type[])null, (Type[])null);
			if (methodInfo == null || methodInfo.ReturnType != typeof(ReadSaveResult<SerializableProgress>) || methodInfo2 == null || methodInfo3 == null)
			{
				throw new MissingMemberException("ProgressSaveManager.LoadProgress or auto-unlock hook is missing");
			}
			harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(methodInfo2), new HarmonyMethod(methodInfo3)
			{
				priority = 0
			}, (HarmonyMethod)null, (HarmonyMethod)null);
			Available = true;
			return true;
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} auto-unlock hook failed: {value}");
			if (MlmLocManager.AutoUnlockAll)
			{
				ReportFailure("settings.saves.autoUnlock.unavailable");
			}
			return false;
		}
	}

	internal static void RequestForLaunch(int profileId)
	{
		_requestedProfileId = null;
		if (Available && MlmLocManager.AutoUnlockAll)
		{
			if ((profileId < 1 || profileId > 3) ? true : false)
			{
				throw new ArgumentOutOfRangeException("profileId");
			}
			_requestedProfileId = profileId;
		}
	}

	private static void BeforeProgressLoaded(ProgressSaveManager __instance, out int? __state)
	{
		__state = null;
		if (!_requestedProfileId.HasValue)
		{
			return;
		}
		try
		{
			SaveManager instance = SaveManager.Instance;
			if (instance.IsProfileInitialized && __instance.Progress == instance.Progress)
			{
				int value = _requestedProfileId.Value;
				_requestedProfileId = null;
				if (instance.CurrentProfileId != value)
				{
					GD.PushWarning($"{"[ModLaunchManager]"} skipped auto-unlock: requested profile {value}, but startup loaded profile {instance.CurrentProfileId}.");
					ReportFailure("settings.saves.autoUnlock.profileChanged");
				}
				else if (MlmLocManager.AutoUnlockAll)
				{
					__state = value;
				}
			}
		}
		catch (Exception value2)
		{
			_requestedProfileId = null;
			GD.PushWarning($"{"[ModLaunchManager]"} could not prepare auto-unlock: {value2}");
			ReportFailure("settings.saves.autoUnlock.failed");
		}
	}

	private static void AfterProgressLoaded(ProgressSaveManager __instance, ReadSaveResult<SerializableProgress> __result, int? __state)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!__state.HasValue)
		{
			return;
		}
		try
		{
			if (!MlmLocManager.AutoUnlockAll)
			{
				return;
			}
			SaveManager instance = SaveManager.Instance;
			if (!instance.IsProfileInitialized || instance.CurrentProfileId != __state.Value || __instance.Progress != instance.Progress)
			{
				ReportFailure("settings.saves.autoUnlock.profileChanged");
			}
			else if (!__result.Success && (int)__result.Status != 2)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} skipped auto-unlock for profile {instance.CurrentProfileId}: {__result.Status} ({__result.ErrorMessage}).");
				ReportFailure("settings.saves.autoUnlock.loadFailed");
			}
			else
			{
				CmdResult val = ((AbstractConsoleCmd)new UnlockConsoleCmd()).Process((Player)null, new string[1] { "all" });
				if (!val.success)
				{
					throw new InvalidOperationException("unlock all failed: " + val.msg);
				}
				GD.Print($"{"[ModLaunchManager]"} ran unlock all for save profile {instance.CurrentProfileId}.");
			}
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} auto-unlock failed: {value}");
			ReportFailure("settings.saves.autoUnlock.failed");
		}
	}

	private static void ReportFailure(string key)
	{
		try
		{
			MainMenuNotice.Queue(MlmLocManager.Get("settings.saves.autoUnlock.failureTitle"), MlmLocManager.Get(key));
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not show the auto-unlock notice: {value}");
		}
	}
}
