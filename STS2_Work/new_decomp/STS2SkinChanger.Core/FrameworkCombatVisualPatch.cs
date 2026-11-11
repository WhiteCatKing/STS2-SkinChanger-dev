using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(CharacterModel), "CreateVisuals")]
internal static class FrameworkCombatVisualPatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(CharacterModel __instance, ref NCreatureVisuals __result)
	{
		string scenePath = "res://scenes/creature_visuals/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant() + ".tscn";
		if (FrameworkSkinRuntime.TryGetCharacterContract(__instance, out string groupId, out FrameworkCharacterSkinContract contract) && contract.CharacterResources.ContainsKey("CombatVisual"))
		{
			__result = MarkManagedCombatScene(SkinService.InstantiateManagedCharacterCreatureVisuals(groupId, scenePath), groupId);
			return false;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name);
		if (skinGroup == null || !SkinService.SelectionServesCombatScene(skinGroup.Id, scenePath))
		{
			return true;
		}
		__result = MarkManagedCombatScene(SkinService.InstantiateManagedCharacterCreatureVisuals(skinGroup.Id, scenePath), skinGroup.Id);
		return false;
	}

	private static NCreatureVisuals MarkManagedCombatScene(NCreatureVisuals visuals, string groupId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)visuals).SetMeta(StringName.op_Implicit("sts2_skin_managed_combat_scene"), Variant.op_Implicit(groupId));
		return visuals;
	}
}
