using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NAudioManager), "PlayOneShot", new Type[]
{
	typeof(string),
	typeof(Dictionary<string, float>),
	typeof(float)
})]
internal static class EnemyNativeIdentityOneShotSfxPatch
{
	private static bool Prefix(string path)
	{
		return !Entry.NativePresentation.ShouldSuppressAudio(path);
	}
}
