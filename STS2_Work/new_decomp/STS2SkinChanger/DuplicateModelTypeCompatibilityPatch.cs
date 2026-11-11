using System;
using System.Linq;
using HarmonyLib;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class DuplicateModelTypeCompatibilityPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(ref Type[] __result)
	{
		__result = (from type in ModelTypeCompatibility.Filter(__result)
			where type != typeof(SkinChangerNetMessage)
			select type).ToArray();
	}
}
