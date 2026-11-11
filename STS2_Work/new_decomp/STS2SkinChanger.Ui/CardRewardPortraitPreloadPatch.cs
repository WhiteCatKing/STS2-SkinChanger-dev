using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CardRewardPortraitPreloadPatch
{
	internal static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCardRewardSelectionScreen), "RefreshOptions", (Type[])null, (Type[])null);
	}

	[HarmonyPriority(800)]
	private static void Prefix(IReadOnlyList<CardCreationResult> options)
	{
		SkinService.PreloadCardPortraits(options.Select((CardCreationResult option) => option.Card));
	}
}
