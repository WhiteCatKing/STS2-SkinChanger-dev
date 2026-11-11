using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NDebugAudioManager), "Play", new Type[]
{
	typeof(string),
	typeof(float),
	typeof(PitchVariance)
})]
internal static class EnemyNativeIdentityDebugSfxPatch
{
	private static bool Prefix(string streamName, ref int __result)
	{
		if (!Entry.NativePresentation.ShouldSuppressActiveAudio(streamName))
		{
			return true;
		}
		__result = -1;
		return false;
	}
}
