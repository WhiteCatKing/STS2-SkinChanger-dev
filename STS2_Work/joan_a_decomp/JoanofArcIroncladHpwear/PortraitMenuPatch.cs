using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NCharacterSelectScreen), "_Ready")]
internal static class PortraitMenuPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCharacterSelectScreen __instance)
	{
		long next = 0L;
		int last = -1;
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.Menu(__instance, ref next, ref last);
		});
	}
}
