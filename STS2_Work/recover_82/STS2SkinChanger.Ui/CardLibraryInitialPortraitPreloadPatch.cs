using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CardLibraryInitialPortraitPreloadPatch
{
	private static readonly MethodInfo ColumnsGetter = AccessTools.PropertyGetter(typeof(NCardGrid), "Columns");

	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCardGrid), "CalculateRowsNeeded", (Type[])null, (Type[])null);
	}

	[HarmonyPriority(800)]
	private static void Postfix(NCardGrid __instance, int __result)
	{
		NCardLibraryGrid val = (NCardLibraryGrid)(object)((__instance is NCardLibraryGrid) ? __instance : null);
		if (val != null && __result > 0)
		{
			int num = (int)(ColumnsGetter.Invoke(__instance, null) ?? ((object)0));
			if (num > 0)
			{
				SkinService.PreloadCardPortraits(val.VisibleCards.Take(__result * num));
			}
		}
	}
}
