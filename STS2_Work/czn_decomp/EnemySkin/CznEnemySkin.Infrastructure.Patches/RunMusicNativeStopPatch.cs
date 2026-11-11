using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NRunMusicController), "StopMusic")]
internal static class RunMusicNativeStopPatch
{
	private static void Postfix()
	{
		Entry.RunMusic.OnNativeStopRequested();
	}
}
