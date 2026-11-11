using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rewards;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patch.Relic;

[HarmonyPatch(typeof(RewardsSet), "Offer")]
public static class WheelRewardsSetPatch
{
	private static void Prefix(RewardsSet __instance)
	{
		WheelOfFortuneRelicCopyState.RegisterRewards(__instance);
	}

	private static void Postfix(RewardsSet __instance, ref Task __result)
	{
		__result = Wrap(__result, __instance);
	}

	private static async Task Wrap(Task task, RewardsSet set)
	{
		try
		{
			await task;
		}
		finally
		{
			WheelOfFortuneRelicCopyState.UnregisterRewards(set);
		}
	}
}
