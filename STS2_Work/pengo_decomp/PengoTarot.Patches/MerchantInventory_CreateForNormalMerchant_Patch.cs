using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using PengoTarot.ConfigFW;
using PengoTarot.Data;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(MerchantInventory), "CreateForNormalMerchant")]
public static class MerchantInventory_CreateForNormalMerchant_Patch
{
	private static void Postfix(Player player, ref MerchantInventory __result)
	{
		PortableTarotRewardContextRegistry.RegisterMerchant(player, __result);
		if (ConfigFloatingWindowRunData.GetTarFlag(0))
		{
			MerchantTarotEntry entry = new MerchantTarotEntry(player);
			TarotEntryHolder.SetEntry(__result, entry);
		}
	}
}
