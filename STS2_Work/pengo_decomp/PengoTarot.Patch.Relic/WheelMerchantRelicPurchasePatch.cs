using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patch.Relic;

[HarmonyPatch(typeof(MerchantRelicEntry), "OnTryPurchase")]
public static class WheelMerchantRelicPurchasePatch
{
	private static void Prefix(MerchantRelicEntry __instance, ref RelicModel? __state)
	{
		__state = __instance.Model;
	}

	private static void Postfix(MerchantRelicEntry __instance, RelicModel? __state, ref Task<(bool, int)> __result)
	{
		__result = Wrap(__result, __instance, __state);
	}

	private static async Task<(bool, int)> Wrap(Task<(bool, int)> task, MerchantRelicEntry entry, RelicModel? model)
	{
		(bool, int) result = await task;
		int copies = WheelOfFortuneRelicCopyState.TakeMerchantCopies(entry);
		if (result.Item1 && model != null && TarotImmediateEffects.CanWheelOfFortuneCopy(model))
		{
			Player player = Traverse.Create((object)entry).Field("_player").GetValue<Player>();
			for (int i = 0; i < copies; i++)
			{
				RelicModel clone = ModelDb.GetById<RelicModel>(((AbstractModel)model).Id).ToMutable();
				await RelicCmd.Obtain(clone, player, -1);
				RunManager.Instance.RewardSynchronizer.SyncLocalObtainedRelic(clone);
			}
		}
		return result;
	}
}
