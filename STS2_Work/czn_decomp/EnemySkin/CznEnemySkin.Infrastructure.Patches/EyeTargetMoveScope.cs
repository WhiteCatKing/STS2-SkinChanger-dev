using System.Threading;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(EyeWithTeeth), "DistractMove")]
internal static class EyeTargetMoveScope
{
	internal sealed record State(EyeWithTeeth Monster, int Generation);

	internal static readonly AsyncLocal<State?> Current = new AsyncLocal<State>();

	private static void Prefix(EyeWithTeeth __instance, out State? __state)
	{
		__state = Current.Value;
		Current.Value = new State(__instance, Entry.ActionPresentation.EffectsGeneration);
	}

	private static void Postfix(State? __state)
	{
		Current.Value = __state;
	}
}
