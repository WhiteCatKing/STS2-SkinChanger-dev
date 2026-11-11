using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NAudioManager), "SetBgmVol")]
internal static class CombatBgmChannelVolumePatch
{
	private static void Postfix(float volume)
	{
		Entry.RunMusic.UpdateBgmVolume(volume);
	}
}
