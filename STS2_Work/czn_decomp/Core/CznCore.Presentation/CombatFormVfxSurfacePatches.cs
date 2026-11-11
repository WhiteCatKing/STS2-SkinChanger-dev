using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;

namespace CznCore.Presentation;

internal static class CombatFormVfxSurfacePatches
{
	[HarmonyPatch(typeof(NFormVfx), "Initialize")]
	private static class FormVfxInitializePatch
	{
		private static void Postfix(NFormVfx __instance, Player owner)
		{
			CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(owner.Character);
			if ((object)characterSkinProfile == null || !CombatFormVfxSurfaceRuntime.OwnsSurface(characterSkinProfile, owner))
			{
				return;
			}
			try
			{
				CombatFormVfxSurfaceRuntime.Adapt(characterSkinProfile, __instance, owner);
			}
			catch (Exception ex)
			{
				CombatFormVfxSurfaceRuntime.DisableFailedVisual(__instance);
				SkinLog.Warn("Disabled local " + ((object)__instance).GetType().Name + " after replacement-skeleton adaptation failed; gameplay continues: " + SkinLog.FormatException(ex));
			}
		}

		private static Exception? Finalizer(NFormVfx __instance, Player owner, Exception? __exception)
		{
			if (__exception != null)
			{
				CharacterSkinProfile characterSkinProfile = CharacterSkinModBootstrap.FindProfile(owner.Character);
				if ((object)characterSkinProfile != null && CombatFormVfxSurfaceRuntime.OwnsSurface(characterSkinProfile, owner))
				{
					CombatFormVfxSurfaceRuntime.DisableFailedVisual(__instance);
					SkinLog.Warn("Suppressed local " + ((object)__instance).GetType().Name + " initialization failure so the synchronized Form action can complete: " + SkinLog.FormatException(__exception));
					return null;
				}
			}
			return __exception;
		}
	}

	public static IReadOnlyList<Type> PatchTypes { get; } = new _003C_003Ez__ReadOnlySingleElementList<Type>(typeof(FormVfxInitializePatch));
}
