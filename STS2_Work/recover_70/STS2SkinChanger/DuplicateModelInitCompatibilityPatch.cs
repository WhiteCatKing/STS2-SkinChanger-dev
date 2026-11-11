using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

[HarmonyPatch]
internal static class DuplicateModelInitCompatibilityPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(ModelDb), "Init", (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(ModelDb).FullName, "Init");
	}

	[HarmonyPriority(0)]
	private static void Prefix(object[] __args)
	{
		int value = ModelTypeCompatibility.RemoveExistingCanonicalConflicts();
		Type[] array2;
		if (__args != null && __args.Length == 1)
		{
			object obj = __args[0];
			if (obj != null && obj is Type[] array)
			{
				array2 = array;
				goto IL_0028;
			}
		}
		array2 = ModelDb.AllAbstractModelSubtypes;
		goto IL_0028;
		IL_0028:
		int num = array2.Length;
		Type[] array3 = ModelTypeCompatibility.Filter(array2, includeAlreadyRegisteredModels: true);
		int value2 = num - array3.Length;
		if (__args.Length != 0)
		{
			__args[0] = array3;
		}
		ModLog.Info($"ModelDb.Init 兼容补丁已执行：模型 {num} 个，移除列表重复 {value2} 个，移除已注入冲突 {value} 个。");
	}

	[HarmonyPostfix]
	private static void Postfix()
	{
		ProviderModelDiscovery.InstantiatePublishedAfterInit();
	}
}
