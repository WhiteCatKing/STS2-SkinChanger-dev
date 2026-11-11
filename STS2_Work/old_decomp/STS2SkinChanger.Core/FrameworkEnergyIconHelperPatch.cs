using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(EnergyIconHelper), "GetPath", new Type[] { typeof(string) })]
internal static class FrameworkEnergyIconHelperPatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(string prefix, ref string __result)
	{
		if (!SkinService.TryGetSelectedFrameworkContract(FrameworkSkinRuntime.NormalizeToken(prefix), out FrameworkCharacterSkinContract contract) || !FrameworkSkinRuntime.UsesDeclarativePresentation(contract) || !contract.CharacterResources.TryGetValue("EnergyIcon", out string value))
		{
			return true;
		}
		__result = value;
		return false;
	}
}
