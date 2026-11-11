using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class FrameworkNativeUiPatch
{
	public static void Install(Assembly assembly, Harmony harmony)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		Type type = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinSelectorInjector", throwOnError: true);
		harmony.Patch((MethodBase)AccessTools.Method(type, "Postfix", (Type[])null, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(FrameworkNativeUiPatch), "RemoveDebugLookup", (Type[])null), (HarmonyMethod)null);
		Type type2 = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.Patches.EntryAnimationAdd", throwOnError: true);
		harmony.Patch((MethodBase)AccessTools.Method(type2, "AddEntry", (Type[])null, (Type[])null), new HarmonyMethod(typeof(FrameworkNativeUiPatch), "OwnsEntryAnimation", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static bool OwnsEntryAnimation(CharacterModel __1)
	{
		if (SkinService.TryGetSelectedFrameworkContract(FrameworkSkinRuntime.NormalizeToken(((AbstractModel)__1).Id.Entry), out FrameworkCharacterSkinContract contract))
		{
			return FrameworkRegistryCooperation.UsesNativePresentation(contract);
		}
		return false;
	}

	internal static IEnumerable<CodeInstruction> RemoveDebugLookup(IEnumerable<CodeInstruction> source)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		List<CodeInstruction> list = source.ToList();
		int num = list.FindIndex((CodeInstruction instruction) => instruction.operand is MethodInfo { Name: "AddChildSafely" } methodInfo && methodInfo.DeclaringType?.FullName == "MegaCrit.Sts2.Core.Helpers.GodotTreeExtensions");
		if (num >= 0 && num + 2 < list.Count && list[num + 1].opcode == OpCodes.Ldarg_0 && list[num + 2].opcode == OpCodes.Ldstr && object.Equals(list[num + 2].operand, "CharSelectButtons/ButtonContainer/DEFECT_button/PlayerIconContainer") && list.Take(num + 1).All((CodeInstruction instruction) => instruction.blocks.Count == 0))
		{
			return list.Take(num + 1).Append(new CodeInstruction(OpCodes.Ret, (object)null));
		}
		return list;
	}
}
