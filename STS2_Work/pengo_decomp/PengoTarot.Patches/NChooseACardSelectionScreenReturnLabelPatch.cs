using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Cards;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NChooseACardSelectionScreen), "_Ready")]
public static class NChooseACardSelectionScreenReturnLabelPatch
{
	[HarmonyPostfix]
	private static void Postfix(NChooseACardSelectionScreen __instance)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<CardModel> value = Traverse.Create((object)__instance).Field("_cards").GetValue<IReadOnlyList<CardModel>>();
		if (value != null && value.Count != 0 && value.All((CardModel card) => (card is TarCard || card is PlanetCard) ? true : false))
		{
			Traverse.Create((object)((Node)__instance).GetNode<NChoiceSelectionSkipButton>(NodePath.op_Implicit("SkipButton"))).Field("_label").GetValue<MegaLabel>()
				.SetTextAutoSize(new LocString("gameplay_ui", "PENGO_TAROT_RETURN_BUTTON").GetFormattedText());
		}
	}
}
