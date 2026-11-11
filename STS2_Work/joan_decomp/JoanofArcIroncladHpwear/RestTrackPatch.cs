using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class RestTrackPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRestSiteCharacter __instance)
	{
		try
		{
			HpWear.TrackRest(__instance);
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("rest track failed: " + ex.Message, 1);
		}
	}
}
