using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class ManagedSkinModLoadPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(ModManager), "TryLoadMod", (Type[])null, (Type[])null);
	}

	private static bool Prefix(Mod mod)
	{
		return !ManagedSkinModLoader.TryManage(mod);
	}

	private static void Postfix(Mod mod)
	{
		if (FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod.manifest?.id))
		{
			FrameworkCompatibilityLayer.TryBindOriginalFramework();
		}
	}
}
