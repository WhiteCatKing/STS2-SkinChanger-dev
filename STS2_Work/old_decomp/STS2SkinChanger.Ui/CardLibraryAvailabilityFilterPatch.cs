using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CardLibraryAvailabilityFilterPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCardLibraryGrid), "FilterCards", new Type[2]
		{
			typeof(Func<CardModel, bool>),
			typeof(List<SortingOrders>)
		}, (Type[])null);
	}

	private static void Prefix(NCardLibraryGrid __instance, ref Func<CardModel, bool> filter)
	{
		CardSkinControls.ApplyAvailabilityFilter(__instance, ref filter);
	}
}
