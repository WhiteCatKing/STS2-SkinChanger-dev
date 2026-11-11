using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class BaseLibSourceAliases
{
	private sealed record NameLookup(Func<Type, Mod?> FindMod, Func<bool> IncludeModId);

	private static Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static Harmony? _harmony;

	private static NameLookup? _lookup;

	private static int _lookupFailureReported;

	internal static void Install(Harmony harmony)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (_harmony != null)
		{
			return;
		}
		_harmony = harmony;
		RefreshAliases(ModOrganizationStore.Load());
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(ModManager), "TryLoadMod", new Type[1] { typeof(Mod) }, (Type[])null) ?? throw new MissingMethodException("ModManager", "TryLoadMod");
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(BaseLibSourceAliases), "AfterModLoaded", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null);
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not watch for BaseLib loading: {value}");
		}
		foreach (Mod mod in ModManager.Mods)
		{
			AfterModLoaded(mod);
		}
	}

	internal static void RefreshAliases(ModOrganizationDocument document)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, ModOrganizationEntry> mod in document.Mods)
		{
			mod.Deconstruct(out var key, out var value);
			string text = key;
			string text2 = value.Alias.Trim();
			if (!string.IsNullOrWhiteSpace(text) && text2.Length > 0)
			{
				dictionary[text.Trim()] = text2;
			}
		}
		Volatile.Write(ref _aliases, dictionary);
	}

	private static void AfterModLoaded(Mod mod)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (_lookup != null || (int)mod.state != 1)
		{
			return;
		}
		List<LocString> errors = mod.errors;
		if ((errors != null && errors.Count > 0) || !string.Equals(mod.manifest?.id, "BaseLib", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		try
		{
			if (!TryConnect((from assembly in CollectibleModAssemblyLoader.GetRecordedAssemblies(mod)
				select assembly.GetType("BaseLib.Utils.WhatMod", throwOnError: false)).FirstOrDefault((Type type) => type != null), _harmony))
			{
				GD.PushWarning("[ModLaunchManager] BaseLib source aliases skipped: the installed BaseLib does not expose the supported name lookup.");
			}
			else
			{
				GD.Print("[ModLaunchManager] BaseLib content sources now use saved mod aliases.");
			}
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} BaseLib source aliases unavailable: {value}");
		}
	}

	private static bool TryConnect(Type? source, Harmony harmony)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		if (_lookup != null)
		{
			return true;
		}
		MethodInfo methodInfo = ((source == null) ? null : AccessTools.DeclaredMethod(source, "FindModName", new Type[1] { typeof(Type) }, (Type[])null));
		MethodInfo methodInfo2 = ((source == null) ? null : AccessTools.DeclaredMethod(source, "FindMod", new Type[1] { typeof(Type) }, (Type[])null));
		Func<bool> func = CreateIncludeIdReader(source?.Assembly.GetType("BaseLib.Config.BaseLibConfig", throwOnError: false));
		if ((object)methodInfo == null || !methodInfo.IsStatic || methodInfo.ContainsGenericParameters || methodInfo.ReturnType != typeof(string) || (object)methodInfo2 == null || !methodInfo2.IsStatic || methodInfo2.ContainsGenericParameters || methodInfo2.ReturnType != typeof(Mod) || func == null)
		{
			return false;
		}
		NameLookup value = new NameLookup(methodInfo2.CreateDelegate<Func<Type, Mod>>(), func);
		harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(BaseLibSourceAliases), "AfterFindModName", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null);
		Volatile.Write(ref _lookup, value);
		return true;
	}

	private static Func<bool>? CreateIncludeIdReader(Type? config)
	{
		if (config == null)
		{
			return null;
		}
		MethodInfo methodInfo = AccessTools.PropertyGetter(config, "ModIdDisplayMode");
		if (methodInfo != null)
		{
			Type modeType = methodInfo.ReturnType;
			if (!methodInfo.IsStatic || methodInfo.GetParameters().Length != 0 || !modeType.IsEnum || !new string[3] { "Id", "Name", "IdAndName" }.All((string value) => Enum.IsDefined(modeType, value)))
			{
				return null;
			}
			return Expression.Lambda<Func<bool>>(Expression.Equal(Expression.Call(methodInfo), Expression.Constant(Enum.Parse(modeType, "IdAndName"), modeType)), Array.Empty<ParameterExpression>()).Compile();
		}
		MethodInfo methodInfo2 = AccessTools.PropertyGetter(config, "IncludeModId");
		if ((object)methodInfo2 == null || !methodInfo2.IsStatic || !(methodInfo2.ReturnType == typeof(bool)) || methodInfo2.GetParameters().Length != 0)
		{
			return null;
		}
		return methodInfo2.CreateDelegate<Func<bool>>();
	}

	private static void AfterFindModName(Type __0, ref string? __result)
	{
		NameLookup nameLookup = Volatile.Read(in _lookup);
		Dictionary<string, string> dictionary = Volatile.Read(in _aliases);
		if (__result == null || nameLookup == null || dictionary.Count == 0)
		{
			return;
		}
		try
		{
			string text = nameLookup.FindMod(__0)?.manifest?.id;
			if (!string.IsNullOrWhiteSpace(text) && dictionary.TryGetValue(text, out var value))
			{
				__result = ((nameLookup.IncludeModId() && !text.Equals(value, StringComparison.OrdinalIgnoreCase)) ? (value + " (" + text + ")") : value);
			}
		}
		catch (Exception value2)
		{
			if (Interlocked.Exchange(ref _lookupFailureReported, 1) == 0)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} BaseLib alias lookup failed; keeping its original source names: {value2}");
			}
		}
	}
}
