using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Cards;

namespace PengoTarot.Patch.Card;

public static class NCard_UnknownEnergyPatch
{
	[HarmonyPatch(typeof(NCard), "UpdateEnergyCostVisuals")]
	public static class UpdateEnergyCostVisuals
	{
		[HarmonyPostfix]
		internal static void Postfix(NCard __instance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)__instance.Visibility == 1)
			{
				return;
			}
			CardModel model = __instance.Model;
			bool flag = ((model is TarCard || model is PlanetCard) ? true : false);
			if (flag && model.EnergyCost.Canonical < 0)
			{
				TextureRect value = Traverse.Create((object)__instance).Field("_energyIcon").GetValue<TextureRect>();
				if (value != null)
				{
					((CanvasItem)value).Visible = false;
				}
				MegaLabel value2 = Traverse.Create((object)__instance).Field("_energyLabel").GetValue<MegaLabel>();
				if (value2 != null)
				{
					value2.SetTextAutoSize(string.Empty);
				}
			}
		}
	}
}
