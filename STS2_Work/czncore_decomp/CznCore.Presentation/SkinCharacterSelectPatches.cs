using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;

namespace CznCore.Presentation;

internal static class SkinCharacterSelectPatches
{
	[HarmonyPatch(typeof(RunManager), "InitializeShared")]
	private static class AppearanceRunIdentityPatch
	{
		private static readonly FieldRef<RunManager, RunState?> State = AccessTools.FieldRefAccess<RunManager, RunState>("<State>k__BackingField");

		private static void Postfix(RunManager __instance, long startTime, int numReloads)
		{
			RunState obj = State.Invoke(__instance);
			CharacterAppearanceCatalog.BeginRun(((obj != null) ? obj.Rng.StringSeed : null) ?? startTime.ToString(CultureInfo.InvariantCulture), numReloads == 0);
		}
	}

	[HarmonyPatch(typeof(RunManager), "CleanUp")]
	private static class AppearanceRunEndPatch
	{
		private static void Postfix()
		{
			CharacterAppearanceCatalog.EndRun();
			SkinEventHub.Publish(default(RunEndedEvent));
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectTitle")]
	private static class CharacterSelectTitlePatch
	{
		private static void Postfix(CharacterModel __instance, ref string __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile != null)
			{
				CharacterSkinModBootstrap.ApplyCharacterSelectTitle(characterSkinProfile, __instance, ref __result);
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectDesc")]
	private static class CharacterSelectDescriptionPatch
	{
		private static void Postfix(CharacterModel __instance, ref string __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile != null)
			{
				CharacterSkinModBootstrap.ApplyCharacterSelectDescription(characterSkinProfile, __instance, ref __result);
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_Title")]
	private static class CharacterTitlePatch
	{
		private static void Postfix(CharacterModel __instance, ref LocString __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile != null)
			{
				CharacterSkinModBootstrap.ApplyCharacterTitle(characterSkinProfile, __instance, ref __result);
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_TitleObject")]
	private static class CharacterTitleObjectPatch
	{
		private static void Postfix(CharacterModel __instance, ref LocString __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile != null)
			{
				CharacterSkinModBootstrap.ApplyCharacterTitleObject(characterSkinProfile, __instance, ref __result);
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_MapMarker")]
	private static class CharacterMapMarkerPatch
	{
		private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile != null)
			{
				CharacterSkinModBootstrap.ApplyCharacterMapMarker(characterSkinProfile, __instance, ref __result);
			}
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectButton), "Init")]
	private static class CharacterSelectIconInitializedPatch
	{
		private static void Postfix(NCharacterSelectButton __instance)
		{
			CharacterSelectIconBackground.Apply(__instance, CharacterSkinModBootstrap.FindProfile(__instance.Character)?.CharacterSelectPresentation);
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectIcon")]
	private static class CharacterSelectIconPatch
	{
		private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile == null)
			{
				return;
			}
			string characterSelectIconPath = characterSkinProfile.CharacterSelectPresentation.CharacterSelectIconPath;
			if (!string.IsNullOrEmpty(characterSelectIconPath))
			{
				CompressedTexture2D val = ResourceLoader.Load<CompressedTexture2D>(characterSelectIconPath, (string)null, (CacheMode)1);
				if (val != null)
				{
					__result = val;
				}
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_IconTexture")]
	private static class CharacterIconPatch
	{
		private static void Postfix(CharacterModel __instance, ref Texture2D __result)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(__instance);
			if ((object)characterSkinProfile == null)
			{
				return;
			}
			string characterIconPath = characterSkinProfile.CharacterSelectPresentation.CharacterIconPath;
			if (!string.IsNullOrEmpty(characterIconPath))
			{
				Texture2D val = ResourceLoader.Load<Texture2D>(characterIconPath, (string)null, (CacheMode)1);
				if (val != null)
				{
					__result = val;
				}
			}
		}
	}

	[HarmonyPatch(typeof(NMultiplayerLoadGameScreen), "AfterMultiplayerStarted")]
	private static class MultiplayerLoadScreenStartedPatch
	{
		private static void Postfix(NMultiplayerLoadGameScreen __instance)
		{
			SkinEventHub.Publish(new MultiplayerLoadScreenStartedEvent(__instance));
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
	private static class CharacterSelectedPatch
	{
		[HarmonyPriority(800)]
		private static void Prefix(NCharacterSelectScreen __instance)
		{
			CharacterSelectSettingsMenu.ReleaseBeforeSelection((Control)(object)__instance);
			CharacterAppearanceCatalog.EnterSelectionScreen();
		}

		private static void Postfix(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton, CharacterModel characterModel)
		{
			RunStartPreloadRuntime.ForCharacter(characterModel)?.ScheduleCharacterSelectEntryWarmup((Node)(object)__instance, characterModel, "Character-select");
			SkinEventHub.Publish(new CharacterSelectedEvent(__instance, charSelectButton, characterModel));
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), "OnLocalCharacterChangedForRandom")]
	private static class RandomCharacterResolvedPatch
	{
		[HarmonyPriority(800)]
		private static void Prefix(NCharacterSelectScreen __instance)
		{
			CharacterSelectSettingsMenu.ReleaseBeforeSelection((Control)(object)__instance);
		}

		private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
		{
			RunStartPreloadRuntime.ForCharacter(characterModel)?.ScheduleCharacterSelectEntryWarmup((Node)(object)__instance, characterModel, "Random character-select");
			SkinEventHub.Publish(new RandomCharacterResolvedEvent(__instance, characterModel));
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectButton), "Select")]
	private static class CharacterSelectButtonSelectedPatch
	{
		private static void Postfix(NCharacterSelectButton __instance)
		{
			RunStartPreloadRuntime.ForCharacter(__instance.Character)?.ScheduleCharacterSelectEntryWarmup((Node)(object)__instance, __instance.Character, "Character-select button");
			SkinEventHub.Publish(new CharacterSelectButtonSelectedEvent(__instance));
		}
	}

	public static IReadOnlyList<Type> PatchTypes { get; } = new _003C_003Ez__ReadOnlyArray<Type>(new Type[14]
	{
		typeof(AppearanceRunIdentityPatch),
		typeof(AppearanceRunEndPatch),
		typeof(CharacterSelectTitlePatch),
		typeof(CharacterSelectDescriptionPatch),
		typeof(CharacterTitlePatch),
		typeof(CharacterTitleObjectPatch),
		typeof(CharacterMapMarkerPatch),
		typeof(CharacterSelectIconPatch),
		typeof(CharacterSelectIconInitializedPatch),
		typeof(CharacterIconPatch),
		typeof(MultiplayerLoadScreenStartedPatch),
		typeof(CharacterSelectedPatch),
		typeof(RandomCharacterResolvedPatch),
		typeof(CharacterSelectButtonSelectedPatch)
	});
}
