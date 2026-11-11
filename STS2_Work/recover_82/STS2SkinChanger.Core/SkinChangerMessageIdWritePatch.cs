using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(MessageTypes), "ToId")]
internal static class SkinChangerMessageIdWritePatch
{
	private static bool Prefix(INetMessage message, ref int __result)
	{
		if (!(message is SkinChangerNetMessage))
		{
			return true;
		}
		__result = 254;
		return false;
	}
}
