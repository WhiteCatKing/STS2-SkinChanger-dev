using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(Hook), "ModifyGeneratedMapLate")]
public static class DivinationMarkerPatch
{
	public static void Postfix(IRunState runState, ActMap map, int actIndex)
	{
		TarotMarkerSystem.ApplyMarkers(runState, map, actIndex);
	}
}
