using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NHandCardHolder), "get_ShouldGlowRed")]
public static class NHandCardHolder_ShouldGlowRed_DeathPatch
{
	private static void Postfix(NHandCardHolder __instance, ref bool __result)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if (!__result)
		{
			NCard cardNode = ((NCardHolder)__instance).CardNode;
			CardModel val = ((cardNode != null) ? cardNode.Model : null);
			if (val != null && (int)val.Type == 3 && val.Owner.Creature.GetPower<TarDeathReversedPower>() != null && val.CanPlay())
			{
				__result = true;
			}
		}
	}
}
