using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(MonsterModel), "CreateVisuals")]
internal static class MonsterVisualResultPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(MonsterModel __instance)
	{
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name);
		if (skinGroup != null)
		{
			CharacterAppearanceRuntime.AddVisibleCombatRuntimeGroup(skinGroup.Id);
		}
	}

	[HarmonyPriority(0)]
	private static void Postfix(MonsterModel __instance, ref NCreatureVisuals __result)
	{
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name);
		if (skinGroup == null || MultiplayerSkinSync.GetScopedSelection(skinGroup.Id) == null)
		{
			ContextualSkinControls.ReplaceCreatedVisuals(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name, ContextualSkinControls.GetMonsterVisualsPath(__instance), ref __result);
			ContextualSkinControls.ApplySelectedProviderVisualPostfix(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name, __instance, ref __result);
			ContextualSkinControls.MarkAndApplyMonsterScale(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name, __result);
		}
	}
}
