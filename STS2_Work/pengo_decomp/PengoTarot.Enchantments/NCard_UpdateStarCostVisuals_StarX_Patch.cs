using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace PengoTarot.Enchantments;

[HarmonyPatch(typeof(NCard), "UpdateStarCostVisuals")]
public static class NCard_UpdateStarCostVisuals_StarX_Patch
{
	private static void Postfix(NCard __instance)
	{
		CardModel model = __instance.Model;
		if (((model != null) ? model.Enchantment : null) is TarStarReversedEnchantment { IsSwappedToEnergyX: not false })
		{
			TextureRect value = Traverse.Create((object)__instance).Field<TextureRect>("_starIcon").Value;
			MegaLabel value2 = Traverse.Create((object)__instance).Field<MegaLabel>("_starLabel").Value;
			if (value != null)
			{
				((CanvasItem)value).Visible = false;
			}
			if (value2 != null)
			{
				((Label)value2).Text = "";
			}
		}
	}
}
