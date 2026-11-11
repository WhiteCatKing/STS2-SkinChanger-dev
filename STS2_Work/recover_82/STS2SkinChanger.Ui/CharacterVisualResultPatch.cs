using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(CharacterModel), "CreateVisuals")]
internal static class CharacterVisualResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(CharacterModel __instance, ref NCreatureVisuals __result)
	{
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name);
		if (skinGroup != null && MultiplayerSkinSync.GetScopedSelection(skinGroup.Id) != null)
		{
			return;
		}
		string text = ContextualSkinControls.ResolveVariantCombatPath(__instance);
		if (text != null)
		{
			PackedScene val = GD.Load<PackedScene>(text);
			if (val != null)
			{
				__result = val.Instantiate<NCreatureVisuals>((GenEditState)0);
				ModLog.Info("已应用十字军变体场景：" + text);
				return;
			}
		}
		string text2 = ContextualSkinControls.CanonicalScenePath("creature_visuals/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant());
		if (!ContextualSkinControls.HasCustomVisualsPath(__instance, text2))
		{
			ContextualSkinControls.ReplaceCreatedVisuals(((AbstractModel)__instance).Id.Entry, text2, ref __result);
			ContextualSkinControls.ApplySelectedProviderVisualPostfix(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name, __instance, ref __result);
		}
	}
}
