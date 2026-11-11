using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Platform;

namespace ModLaunchManager;

internal static class ModErrorText
{
	internal const string SteamBranchUnsupportedKey = "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED";

	private const string AssemblyLoadKey = "MOD_ERROR.ASSEMBLY_LOAD";

	private const string ExceptionKey = "MOD_ERROR.EXCEPTION";

	internal static bool IsSteamBranchMismatch(LocString error)
	{
		return string.Equals(error.LocEntryKey, "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED", StringComparison.Ordinal);
	}

	internal static bool IsGenericFailure(LocString error)
	{
		string locEntryKey = error.LocEntryKey;
		if (locEntryKey == "MOD_ERROR.ASSEMBLY_LOAD" || locEntryKey == "MOD_ERROR.EXCEPTION")
		{
			return true;
		}
		return false;
	}

	internal static bool HasInitializerFailure(IEnumerable<LocString>? errors)
	{
		return errors?.Any((LocString error) => error.LocEntryKey == "MOD_ERROR.ASSEMBLY_LOAD") ?? false;
	}

	internal static string? ExceptionType(IEnumerable<LocString>? errors)
	{
		object value;
		return (from error in errors?.Where((LocString error) => error.LocEntryKey == "MOD_ERROR.EXCEPTION")
			select (!error.Variables.TryGetValue("exceptionType", out value)) ? null : value?.ToString()).FirstOrDefault((string type) => !string.IsNullOrEmpty(type));
	}

	internal static void ClearSteamBranchMismatch(Mod mod)
	{
		mod.errors?.RemoveAll(IsSteamBranchMismatch);
	}

	internal static void AddSteamBranchMismatch(Mod mod, string? supportedBranch)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (mod.errors == null)
		{
			mod.errors = new List<LocString>();
		}
		LocString val = new LocString("main_menu_ui", "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED");
		val.Add("id", mod.manifest?.id ?? "<null>");
		val.Add("currentBranch", CurrentBranchName());
		val.Add("supportedBranch", supportedBranch ?? "<null>");
		mod.errors.Add(val);
	}

	internal static string CurrentBranchName()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			return PlatformBranchExtensions.ToName(PlatformUtil.GetPlatformBranch());
		}
		catch
		{
			return MlmLocManager.Get("common.value.unknown");
		}
	}

	internal static string Describe(LocString error)
	{
		string text = error.LocEntryKey ?? string.Empty;
		string item = Variable(error, "id");
		return text switch
		{
			"MOD_ERROR.STEAM_BRANCH_UNSUPPORTED" => MlmLocManager.Get("modError.steamBranchUnsupported", ("id", item), ("supportedBranch", Variable(error, "supportedBranch")), ("currentBranch", Variable(error, "currentBranch"))), 
			"MOD_ERROR.ASSEMBLY_LOAD" => MlmLocManager.Get("modError.assemblyLoad", ("id", item)), 
			"MOD_ERROR.CIRCULAR_DEPENDENCY" => MlmLocManager.Get("modError.circularDependency", ("id", item), ("dependencyChain", Variable(error, "dependencyChain"))), 
			"MOD_ERROR.DEPENDENCY_MIN_VERSION_INVALID" => MlmLocManager.Get("modError.dependencyMinVersionInvalid", ("id", item), ("dependency", Variable(error, "dependency")), ("minVersion", Variable(error, "minVersion"))), 
			"MOD_ERROR.DEPENDENCY_VERSION_INVALID" => MlmLocManager.Get("modError.dependencyVersionInvalid", ("id", item), ("dependency", Variable(error, "dependency")), ("minVersion", Variable(error, "minVersion")), ("version", Variable(error, "version"))), 
			"MOD_ERROR.DEPENDENCY_VERSION_MISSING" => MlmLocManager.Get("modError.dependencyVersionMissing", ("id", item), ("dependency", Variable(error, "dependency")), ("minVersion", Variable(error, "minVersion"))), 
			"MOD_ERROR.DEPENDENCY_VERSION_UNSUPPORTED" => MlmLocManager.Get("modError.dependencyVersionUnsupported", ("id", item), ("dependency", Variable(error, "dependency")), ("minVersion", Variable(error, "minVersion")), ("version", Variable(error, "version"))), 
			"MOD_ERROR.DUPLICATE_ID" => MlmLocManager.Get("modError.duplicateId", ("id", item)), 
			"MOD_ERROR.EXCEPTION" => MlmLocManager.Get("modError.exception", ("id", item), ("exceptionType", Variable(error, "exceptionType"))), 
			"MOD_ERROR.GAME_VERSION_INVALID" => MlmLocManager.Get("modError.gameVersionInvalid", ("id", item), ("minGameVersion", Variable(error, "minGameVersion"))), 
			"MOD_ERROR.GAME_VERSION_UNSUPPORTED" => MlmLocManager.Get("modError.gameVersionUnsupported", ("id", item), ("minGameVersion", Variable(error, "minGameVersion")), ("gameVersion", Variable(error, "gameVersion"))), 
			"MOD_ERROR.MIGRATION_REQUIRED" => MlmLocManager.Get("modError.migrationRequired", ("id", item)), 
			"MOD_ERROR.MISSING_DEPENDENCY" => MlmLocManager.Get("modError.missingDependency", ("id", item), ("missingCount", Variable(error, "missingCount")), ("missingDependencies", Variable(error, "missingDependencies"))), 
			_ => Fallback(error, text), 
		};
	}

	private static string Fallback(LocString error, string key)
	{
		if (error.Variables.Count == 0)
		{
			return key;
		}
		string item = string.Join(MlmLocManager.Get("common.listSeparator"), error.Variables.Select((KeyValuePair<string, object> pair) => $"{pair.Key}={pair.Value}"));
		return MlmLocManager.Get("modError.fallback.withVariables", ("key", key), ("variables", item));
	}

	private static string Variable(LocString error, string name)
	{
		if (!error.Variables.TryGetValue(name, out var value))
		{
			return "?";
		}
		return value?.ToString() ?? "?";
	}
}
