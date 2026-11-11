using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class GodotScriptPathRegistrationCompatibilityPatch
{
	private static bool Prepare()
	{
		return FindTarget() != null;
	}

	private static MethodBase TargetMethod()
	{
		return FindTarget() ?? throw new MissingMethodException("Godot.Bridge.ScriptManagerBridge.PathScriptTypeBiMap.Add");
	}

	private static bool Prefix(object __instance, string scriptPath, Type scriptType)
	{
		if (!(__instance.GetType().GetField("_pathTypeMap", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance) is IDictionary<string, Type> dictionary) || !dictionary.TryGetValue(scriptPath, out var value))
		{
			return true;
		}
		return !GodotScriptPathRegistrationPolicy.IsIdempotentRepeat(value, scriptType);
	}

	private static MethodBase? FindTarget()
	{
		return (typeof(GodotObject).Assembly.GetType("Godot.Bridge.ScriptManagerBridge")?.GetNestedType("PathScriptTypeBiMap", BindingFlags.NonPublic))?.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(string),
			typeof(Type)
		}, null);
	}
}
