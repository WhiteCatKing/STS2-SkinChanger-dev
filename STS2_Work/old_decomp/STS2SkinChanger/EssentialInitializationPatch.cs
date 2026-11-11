using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

[HarmonyPatch(typeof(OneTimeInitialization), "ExecuteEssential")]
internal static class EssentialInitializationPatch
{
	private static void Prefix()
	{
		VisualPatchGuard.RemoveProviderVisualPatches(ManagedSkinModLoader.ProviderRoots, ManagedSkinModLoader.GetPreservedRuntimeRoots(ModManager.Mods));
		SkinService.InitializeBeforeAssets();
	}

	private static void Postfix()
	{
		VisualPatchGuard.RemoveProviderVisualPatches(ManagedSkinModLoader.ProviderRoots, ManagedSkinModLoader.GetPreservedRuntimeRoots(ModManager.Mods));
		SkinService.InitializeCardGroupsAfterModels();
		SkinService.PrepareSelectedCharacterPreviews();
		ExternalCardVisualBridge.WarmUp();
		SkinWorkshopService.StartCommunityRefresh();
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception)
	{
		QuitCacheCleanup.MarkInitializationCompleted();
		return __exception;
	}
}
