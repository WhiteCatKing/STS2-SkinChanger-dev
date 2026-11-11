using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(ModelDb), "get_AllCardPools")]
public static class ModelDb_AllCardPools_Patch
{
	private static void Postfix(ref IEnumerable<CardPoolModel> __result)
	{
		TarotPool tarotPool = ModelDb.CardPool<TarotPool>();
		PlanetPool planetPool = ModelDb.CardPool<PlanetPool>();
		__result = __result.Concat((IEnumerable<CardPoolModel>)(object)new CardPoolModel[2] { tarotPool, planetPool });
	}
}
