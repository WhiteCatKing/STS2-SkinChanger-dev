using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CharacterAppearancePauseMenuPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new global::_003C_003Ez__ReadOnlyArray<MethodBase>(new MethodBase[2]
		{
			AccessTools.Method(typeof(NPauseMenu), "_Ready", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(NPauseMenu), "OnSubmenuOpened", (Type[])null, (Type[])null)
		});
	}

	[HarmonyPriority(0)]
	private static void Postfix(NPauseMenu __instance)
	{
		CharacterAppearancePauseMenu.Attach(__instance);
	}
}
