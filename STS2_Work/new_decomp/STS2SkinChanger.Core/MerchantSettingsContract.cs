using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Core;

internal sealed record MerchantSettingsContract(Type CommandType, MethodInfo Save, MethodInfo ApplyHand, MethodInfo ApplyLegs, MethodInfo UseFoot, MethodInfo? SettingsPostfix, IReadOnlyList<MethodInfo> WorldApplyMethods)
{
	private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	internal static MerchantSettingsContract? TryCreate(Assembly assembly)
	{
		Type[] types = assembly.GetTypes();
		foreach (Type item in types.Where((Type type) => !type.IsAbstract && typeof(AbstractConsoleCmd).IsAssignableFrom(type)))
		{
			MethodInfo method2 = item.GetMethod("ApplyToExistingHands", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes);
			MethodInfo method3 = item.GetMethod("UpdateLegVisibility", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(bool) });
			if (method2?.ReturnType != typeof(int) || method3?.ReturnType != typeof(void) || !IsAdapted(method2) || !IsAdapted(method3))
			{
				continue;
			}
			MethodInfo method4 = item.GetMethod("TryApplyToHand", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(NMerchantHand) });
			MethodInfo method5 = item.GetMethod("UpdateLegVisibilityStatic", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[2]
			{
				typeof(Node),
				typeof(bool)
			});
			MethodInfo method6 = item.GetMethod("ProcessPointCommand", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(string[]) });
			MethodInfo save = ((method6 == null) ? null : Calls(method6).SingleOrDefault((MethodInfo method) => method.Name == "Save" && method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.Module.Assembly == assembly));
			MethodInfo methodInfo = ((item.TypeInitializer == null) ? null : Calls(item.TypeInitializer).SingleOrDefault((MethodInfo method) => method.Name == "get_UseFootLike" && method.ReturnType == typeof(bool) && method.GetParameters().Length == 0 && method.Module.Assembly == assembly));
			if (!(method4?.ReturnType != typeof(bool)) && !(method5?.ReturnType != typeof(void)) && !(save == null) && !(methodInfo == null))
			{
				MethodInfo settingsPostfix = (from type in types
					where type.Namespace == save.DeclaringType?.Namespace + ".Patches"
					select type.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[2]
					{
						typeof(NModInfoContainer),
						typeof(Mod)
					})).SingleOrDefault((MethodInfo method) => method?.DeclaringType?.Name == "ModdingScreenUiPatch");
				return new MerchantSettingsContract(item, save, method4, method5, methodInfo, settingsPostfix, new global::_003C_003Ez__ReadOnlyArray<MethodInfo>(new MethodInfo[2] { method2, method3 }));
			}
		}
		return null;
	}

	private static bool IsAdapted(MethodInfo method)
	{
		return Calls(method).Any((MethodInfo call) => call.DeclaringType == typeof(ProviderSettingsApi) && call.Name == "Refresh");
	}

	private static IEnumerable<MethodInfo> Calls(MethodBase method)
	{
		return (from instruction in PatchProcessor.GetOriginalInstructions(method, (ILGenerator)null)
			select instruction.operand).OfType<MethodInfo>().Distinct();
	}

	[CompilerGenerated]
	private MerchantSettingsContract(MerchantSettingsContract original)
	{
		CommandType = original.CommandType;
		Save = original.Save;
		ApplyHand = original.ApplyHand;
		ApplyLegs = original.ApplyLegs;
		UseFoot = original.UseFoot;
		SettingsPostfix = original.SettingsPostfix;
		WorldApplyMethods = original.WorldApplyMethods;
	}
}
