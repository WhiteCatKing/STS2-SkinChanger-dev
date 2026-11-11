using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class CharacterSkinBundleNewRunBindingPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new MethodInfo[2]
		{
			AccessTools.Method(typeof(RunManager), "SetUpNewSingleplayer", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(RunManager), "SetUpNewMultiplayer", (Type[])null, (Type[])null)
		};
	}

	private static void Postfix(RunManager __instance, RunState state, bool shouldSave, MethodBase __originalMethod)
	{
		try
		{
			SkinService.BindNewCharacterSkinBundleRun(__instance, state, __originalMethod.Name == "SetUpNewMultiplayer", shouldSave);
		}
		catch (Exception ex)
		{
			ModLog.Error("保存新对局的皮肤包记录失败，不影响游戏开局：" + ex);
		}
	}
}
