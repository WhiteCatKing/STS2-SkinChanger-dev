using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(EncounterModel), "CreateBackground")]
internal static class FakeMerchantCombatArtPatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(EncounterModel __instance)
	{
		if (((AbstractModel)__instance).Id.Entry.Equals("fake_merchant_event_encounter", StringComparison.OrdinalIgnoreCase))
		{
			SkinService.MountFakeMerchantEventArt();
		}
	}
}
