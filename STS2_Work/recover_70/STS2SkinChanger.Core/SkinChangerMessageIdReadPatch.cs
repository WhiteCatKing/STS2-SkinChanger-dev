using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(MessageTypes), "TryGetMessageType")]
internal static class SkinChangerMessageIdReadPatch
{
	private static bool Prefix(int id, ref Type? type, ref bool __result)
	{
		if (id != 254)
		{
			return true;
		}
		type = typeof(SkinChangerNetMessage);
		__result = true;
		return false;
	}
}
