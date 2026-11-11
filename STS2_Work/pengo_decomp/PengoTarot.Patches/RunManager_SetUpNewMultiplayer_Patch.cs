using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Relics;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(RunManager), "SetUpNewMultiplayer")]
public static class RunManager_SetUpNewMultiplayer_Patch
{
	private static void Postfix(RunState state)
	{
		if (!ConfigFloatingWindowRunData.PlanetEnabled)
		{
			return;
		}
		foreach (Player player in state.Players)
		{
			if (player.GetRelic<StargazerKit>() == null)
			{
				RelicModel val = ((RelicModel)ModelDb.Relic<StargazerKit>()).ToMutable();
				player.AddRelicInternal(val, -1, false);
			}
		}
	}
}
