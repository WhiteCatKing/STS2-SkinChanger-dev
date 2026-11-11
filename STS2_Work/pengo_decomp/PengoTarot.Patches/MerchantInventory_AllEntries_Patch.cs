using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using PengoTarot.ConfigFW;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(MerchantInventory), "get_AllEntries")]
public static class MerchantInventory_AllEntries_Patch
{
	private static void Postfix(ref IEnumerable<MerchantEntry> __result, MerchantInventory __instance)
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(0))
		{
			MerchantTarotEntry entry = TarotEntryHolder.GetEntry(__instance);
			if (entry != null)
			{
				__result = __result.Concat((IEnumerable<MerchantEntry>)(object)new MerchantTarotEntry[1] { entry });
			}
		}
	}
}
