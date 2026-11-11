using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class WorkshopRuntimeNoticePatch
{
	private static MethodBase? TargetMethod()
	{
		return AccessTools.Method(typeof(NGame), "OnNewModDetected", (Type[])null, (Type[])null);
	}

	private static bool Prepare()
	{
		return TargetMethod() != null;
	}

	private static bool Prefix(Mod mod)
	{
		return !SkinWorkshopService.DeferNativeNotice(mod);
	}
}
