using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(Reward), "FromSerializable")]
public static class RewardFromSerializableTarotPatch
{
	[HarmonyPrefix]
	private static bool Prefix(SerializableReward save, Player player, ref Reward __result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TarotReward.TryGetFlagFromRewardType(save.RewardType, out var flagIndex))
		{
			__result = (Reward)(object)new TarotReward(player, flagIndex);
			return false;
		}
		return true;
	}
}
