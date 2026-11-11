using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardLibraryGrid), "AssignCardsToRow")]
internal static class CardLibraryRowPortraitPreloadPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCardLibraryGrid __instance, List<NGridCardHolder> row, int startIndex)
	{
		SkinService.PreloadCardPortraits(__instance.VisibleCards.Skip(startIndex).Take(row.Count));
	}
}
