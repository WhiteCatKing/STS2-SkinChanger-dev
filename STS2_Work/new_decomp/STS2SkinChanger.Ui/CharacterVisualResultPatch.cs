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
		if (skinGroup == null || MultiplayerSkinSync.GetScopedSelection(skinGroup.Id) == null)
		{
			ContextualSkinControls.ReplaceCreatedVisuals(((AbstractModel)__instance).Id.Entry, ContextualSkinControls.CanonicalScenePath("creature_visuals/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant()), ref __result);
			ContextualSkinControls.ApplySelectedProviderVisualPostfix(((AbstractModel)__instance).Id.Entry, ((object)__instance).GetType().Name, __instance, ref __result);
		}
	}
}
