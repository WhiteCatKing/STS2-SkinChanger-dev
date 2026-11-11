using System;
using System.Collections.Generic;
using System.Text.Json;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModLoadPriorities
{
	internal const string ManifestKey = "loadPriority";

	internal const string ManifestKeyAlias = "load_priority";

	internal static IReadOnlyList<ModLoadPriority> Tiers { get; } = new global::_003C_003Ez__ReadOnlyArray<ModLoadPriority>(new ModLoadPriority[5]
	{
		ModLoadPriority.First,
		ModLoadPriority.High,
		ModLoadPriority.Default,
		ModLoadPriority.Low,
		ModLoadPriority.Last
	});

	internal static ModLoadPriority? Declared(Mod mod)
	{
		try
		{
			foreach (var (key, jsonNode2) in ModOptionCatalog.For(mod).Values)
			{
				if (IsPriorityKey(key) && jsonNode2 != null && jsonNode2.GetValueKind() == JsonValueKind.String && TryParse(jsonNode2.GetValue<string>(), out var priority))
				{
					return priority;
				}
			}
		}
		catch
		{
		}
		return null;
	}

	internal static bool IsPriorityKey(string key)
	{
		if (!string.Equals(key, "loadPriority", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(key, "load_priority", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static bool TryParse(string? text, out ModLoadPriority priority)
	{
		switch (text?.Trim().ToLowerInvariant())
		{
		case "first":
			priority = ModLoadPriority.First;
			return true;
		case "high":
			priority = ModLoadPriority.High;
			return true;
		case "default":
		case "normal":
			priority = ModLoadPriority.Default;
			return true;
		case "low":
			priority = ModLoadPriority.Low;
			return true;
		case "last":
			priority = ModLoadPriority.Last;
			return true;
		default:
			priority = ModLoadPriority.Default;
			return false;
		}
	}

	internal static string Label(ModLoadPriority priority)
	{
		return MlmLocManager.Get(priority switch
		{
			ModLoadPriority.First => "priority.tier.first", 
			ModLoadPriority.High => "priority.tier.high", 
			ModLoadPriority.Low => "priority.tier.low", 
			ModLoadPriority.Last => "priority.tier.last", 
			_ => "priority.tier.default", 
		});
	}
}
