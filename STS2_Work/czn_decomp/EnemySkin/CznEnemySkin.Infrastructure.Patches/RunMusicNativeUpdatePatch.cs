using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NRunMusicController), "UpdateMusic")]
internal static class RunMusicNativeUpdatePatch
{
	private static void Postfix(NRunMusicController __instance)
	{
		Entry.RunMusic.OnNativeMusicUpdated(__instance);
	}
}
