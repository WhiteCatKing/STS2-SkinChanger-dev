using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CznCore.Presentation;

internal static class RunStartPreloadHarmonyPatches
{
	[HarmonyPatch(typeof(NCharacterSelectScreen), "BeginRun")]
	private static class CharacterSelectRunStartPreloadPatch
	{
		private static bool Prefix(NCharacterSelectScreen __instance, string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
		{
			if (_runStartPreloadBypass || !ShouldInterceptRunStartPreload(__instance))
			{
				return true;
			}
			if (_runStartPreloadActive)
			{
				return false;
			}
			TaskHelper.RunSafely(RunStartPreloadThenBegin(__instance, seed, acts, modifiers));
			return false;
		}
	}

	[HarmonyPatch(typeof(NMainMenu), "OnContinueButtonPressedAsync")]
	private static class MainMenuContinueRunPreloadPatch
	{
		private static bool Prefix(NMainMenu __instance, ref Task __result)
		{
			if (_continueRunPreloadBypass || !ShouldInterceptContinueRunPreload(__instance))
			{
				return true;
			}
			if (_continueRunPreloadActive)
			{
				__result = Task.CompletedTask;
				return false;
			}
			__result = ContinueRunPreloadThenContinue(__instance);
			return false;
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
	private static class CharacterSelectMultiplayerReadyPreloadPatch
	{
		private static bool Prefix(NCharacterSelectScreen __instance)
		{
			if (_multiplayerReadyPreloadBypass || !ShouldInterceptMultiplayerReadyPreload(__instance))
			{
				return true;
			}
			if (_multiplayerReadyPreloadActive)
			{
				return false;
			}
			TaskHelper.RunSafely(MultiplayerReadyPreloadThenEmbark(__instance));
			return false;
		}
	}

	private static readonly FieldInfo? ContinueRunSaveResultField = AccessTools.Field(typeof(NMainMenu), "_readRunSaveResult");

	private static readonly MethodInfo? CharacterSelectEmbarkPressedMethod = AccessTools.Method(typeof(NCharacterSelectScreen), "OnEmbarkPressed", (Type[])null, (Type[])null);

	private static readonly MethodInfo? ContinueButtonPressedAsyncMethod = AccessTools.Method(typeof(NMainMenu), "OnContinueButtonPressedAsync", (Type[])null, (Type[])null);

	private const string MultiplayerReadyPreloadCompletedMetaPrefix = "czn_run_start_preload_multiplayer_ready_completed_";

	private static bool _runStartPreloadBypass;

	private static bool _runStartPreloadActive;

	private static bool _continueRunPreloadBypass;

	private static bool _continueRunPreloadActive;

	private static bool _multiplayerReadyPreloadBypass;

	private static bool _multiplayerReadyPreloadActive;

	public static IReadOnlyList<Type> PatchTypes { get; } = new _003C_003Ez__ReadOnlyArray<Type>(new Type[3]
	{
		typeof(CharacterSelectRunStartPreloadPatch),
		typeof(MainMenuContinueRunPreloadPatch),
		typeof(CharacterSelectMultiplayerReadyPreloadPatch)
	});

	private static bool ShouldInterceptRunStartPreload(NCharacterSelectScreen? screen)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (screen == null || !GodotObject.IsInstanceValid((GodotObject)(object)screen) || !((Node)screen).IsInsideTree())
			{
				return false;
			}
			if ((int)screen.Lobby.NetService.Type != 1)
			{
				return false;
			}
			return RunStartPreloadRuntime.ForCharacter(screen.Lobby.LocalPlayer.character) != null;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Run-start preload guard failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static bool ShouldInterceptMultiplayerReadyPreload(NCharacterSelectScreen? screen)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (screen == null || !GodotObject.IsInstanceValid((GodotObject)(object)screen) || !((Node)screen).IsInsideTree())
			{
				return false;
			}
			if (!NetGameTypeExtensions.IsMultiplayer(screen.Lobby.NetService.Type))
			{
				return false;
			}
			CharacterModel character = screen.Lobby.LocalPlayer.character;
			RunStartPreloadRuntime runStartPreloadRuntime = RunStartPreloadRuntime.ForCharacter(character);
			return runStartPreloadRuntime != null && !HasCompletedMultiplayerReadyPreload(screen, runStartPreloadRuntime, character);
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Multiplayer-ready preload guard failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static bool ShouldInterceptContinueRunPreload(NMainMenu? menu)
	{
		try
		{
			if (menu == null || !GodotObject.IsInstanceValid((GodotObject)(object)menu) || !((Node)menu).IsInsideTree())
			{
				return false;
			}
			ReadSaveResult<SerializableRun> obj = ContinueRunSaveResultField?.GetValue(menu) as ReadSaveResult<SerializableRun>;
			SerializableRun val = obj?.SaveData;
			if (obj == null || !obj.Success || ((val != null) ? val.Players : null) == null || val.Players.Count == 0)
			{
				return false;
			}
			if (val.Players[0].CharacterId == (ModelId)null)
			{
				return false;
			}
			return RunStartPreloadRuntime.ForCharacter(ModelDb.GetById<CharacterModel>(val.Players[0].CharacterId)) != null;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Continue-run preload guard failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static async Task MultiplayerReadyPreloadThenEmbark(NCharacterSelectScreen screen)
	{
		CharacterModel character = TryGetLocalCharacter(screen);
		RunStartPreloadRuntime runStartPreloadRuntime = RunStartPreloadRuntime.ForCharacter(character);
		string completedMetaKey = ((runStartPreloadRuntime == null) ? null : GetMultiplayerReadyPreloadMetaKey(runStartPreloadRuntime));
		string completedToken = ((runStartPreloadRuntime == null) ? null : BuildMultiplayerReadyPreloadToken(runStartPreloadRuntime, character));
		_multiplayerReadyPreloadActive = true;
		try
		{
			if (runStartPreloadRuntime != null)
			{
				await runStartPreloadRuntime.RunPreload((Node)(object)screen, "Multiplayer-ready");
			}
		}
		finally
		{
			_multiplayerReadyPreloadActive = false;
		}
		if (!GodotObject.IsInstanceValid((GodotObject)(object)screen))
		{
			return;
		}
		if (!string.IsNullOrEmpty(completedMetaKey) && completedToken != null)
		{
			((GodotObject)screen).SetMeta(StringName.op_Implicit(completedMetaKey), Variant.From<string>(ref completedToken));
		}
		try
		{
			_multiplayerReadyPreloadBypass = true;
			if (CharacterSelectEmbarkPressedMethod != null)
			{
				CharacterSelectEmbarkPressedMethod.Invoke(screen, new object[1]);
			}
			else
			{
				SkinLog.Warn("Multiplayer-ready native handler missing after preload; ready skipped.");
			}
		}
		finally
		{
			_multiplayerReadyPreloadBypass = false;
		}
	}

	private static bool HasCompletedMultiplayerReadyPreload(NCharacterSelectScreen screen, RunStartPreloadRuntime runtime, CharacterModel? character)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		string multiplayerReadyPreloadMetaKey = GetMultiplayerReadyPreloadMetaKey(runtime);
		if (((GodotObject)screen).HasMeta(StringName.op_Implicit(multiplayerReadyPreloadMetaKey)))
		{
			StringName obj = StringName.op_Implicit(multiplayerReadyPreloadMetaKey);
			Variant val = default(Variant);
			val = ((GodotObject)screen).GetMeta(obj, val);
			return ((Variant)(ref val)).AsString() == BuildMultiplayerReadyPreloadToken(runtime, character);
		}
		return false;
	}

	private static string GetMultiplayerReadyPreloadMetaKey(RunStartPreloadRuntime runtime)
	{
		return "czn_run_start_preload_multiplayer_ready_completed_" + runtime.ProfileId;
	}

	private static string BuildMultiplayerReadyPreloadToken(RunStartPreloadRuntime runtime, CharacterModel? character)
	{
		return runtime.ProfileId + ":" + (((character != null) ? ((AbstractModel)character).Id.Entry : null) ?? string.Empty);
	}

	private static CharacterModel? TryGetLocalCharacter(NCharacterSelectScreen? screen)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			return (screen != null) ? screen.Lobby.LocalPlayer.character : null;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Multiplayer-ready preload character lookup failed: " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private static async Task RunStartPreloadThenBegin(NCharacterSelectScreen screen, string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		_runStartPreloadActive = true;
		try
		{
			RunStartPreloadRuntime runStartPreloadRuntime = RunStartPreloadRuntime.ForCharacter(TryGetLocalCharacter(screen));
			if (runStartPreloadRuntime != null)
			{
				await runStartPreloadRuntime.RunPreload((Node)(object)screen, "Run-start");
			}
		}
		finally
		{
			_runStartPreloadActive = false;
		}
		if (!GodotObject.IsInstanceValid((GodotObject)(object)screen))
		{
			return;
		}
		try
		{
			_runStartPreloadBypass = true;
			screen.BeginRun(seed, acts, modifiers);
		}
		finally
		{
			_runStartPreloadBypass = false;
		}
	}

	private static CharacterModel? TryGetSavedCharacter(NMainMenu menu)
	{
		ReadSaveResult<SerializableRun> obj = ContinueRunSaveResultField?.GetValue(menu) as ReadSaveResult<SerializableRun>;
		SerializableRun val = obj?.SaveData;
		if (obj != null && obj.Success)
		{
			List<SerializablePlayer> list = ((val != null) ? val.Players : null);
			if (list != null && list.Count > 0 && val.Players[0].CharacterId != (ModelId)null)
			{
				return ModelDb.GetById<CharacterModel>(val.Players[0].CharacterId);
			}
		}
		return null;
	}

	private static async Task ContinueRunPreloadThenContinue(NMainMenu menu)
	{
		_continueRunPreloadActive = true;
		try
		{
			RunStartPreloadRuntime runStartPreloadRuntime = RunStartPreloadRuntime.ForCharacter(TryGetSavedCharacter(menu));
			if (runStartPreloadRuntime != null)
			{
				await runStartPreloadRuntime.RunPreload((Node)(object)menu, "Continue-run");
			}
		}
		finally
		{
			_continueRunPreloadActive = false;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)menu))
		{
			Task task = null;
			try
			{
				_continueRunPreloadBypass = true;
				task = ContinueButtonPressedAsyncMethod?.Invoke(menu, Array.Empty<object>()) as Task;
			}
			finally
			{
				_continueRunPreloadBypass = false;
			}
			if (task != null)
			{
				await task;
			}
			else
			{
				SkinLog.Warn("Continue-run native handler missing after preload; load run skipped.");
			}
		}
	}
}
