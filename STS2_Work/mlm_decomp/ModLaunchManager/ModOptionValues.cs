using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModLaunchManager;

internal static class ModOptionValues
{
	internal static JsonNode? Effective(ModOptionSpec spec, ModOptionCatalog catalog, ModOptionEntry? stored)
	{
		if (stored != null && stored.Values.TryGetValue(spec.Key, out JsonNode value))
		{
			return value;
		}
		if (stored != null && stored.Baseline.TryGetValue(spec.Key, out JsonNode value2))
		{
			return value2 ?? spec.DeclaredDefault;
		}
		if (!catalog.Values.TryGetValue(spec.Key, out JsonNode value3) || value3 == null)
		{
			return spec.DeclaredDefault;
		}
		return value3;
	}

	internal static JsonNode? Fallback(ModOptionSpec spec, ModOptionCatalog catalog, ModOptionEntry? stored)
	{
		if (stored != null && stored.Baseline.TryGetValue(spec.Key, out JsonNode value))
		{
			return value ?? spec.DeclaredDefault;
		}
		if (!catalog.Values.TryGetValue(spec.Key, out JsonNode value2) || value2 == null)
		{
			return spec.DeclaredDefault;
		}
		return value2;
	}

	internal static bool IsOverridden(ModOptionSpec spec, ModOptionEntry? stored)
	{
		return stored?.Values.ContainsKey(spec.Key) ?? false;
	}

	internal static string Display(JsonNode? node)
	{
		if (node == null)
		{
			return string.Empty;
		}
		return node.GetValueKind() switch
		{
			JsonValueKind.String => node.GetValue<string>(), 
			JsonValueKind.True => "true", 
			JsonValueKind.False => "false", 
			_ => node.ToJsonString(), 
		};
	}

	internal static bool AsBool(JsonNode? node)
	{
		if (node == null)
		{
			return false;
		}
		return node.GetValueKind() == JsonValueKind.True;
	}

	internal static JsonNode? FromText(ModOptionSpec spec, string text, out string? error)
	{
		error = null;
		string text2 = text.Trim();
		switch (spec.Type)
		{
		case ModOptionType.Int:
		{
			if (!long.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
			{
				error = text2;
				return null;
			}
			return JsonValue.Create((long)Clamp(spec, result2), (JsonNodeOptions?)null);
		}
		case ModOptionType.Float:
		{
			if (!double.TryParse(text2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				error = text2;
				return null;
			}
			return JsonValue.Create(Clamp(spec, result), (JsonNodeOptions?)null);
		}
		default:
			return JsonValue.Create(text, (JsonNodeOptions?)null);
		}
	}

	private static double Clamp(ModOptionSpec spec, double value)
	{
		double? minimum = spec.Minimum;
		if (minimum.HasValue)
		{
			double valueOrDefault = minimum.GetValueOrDefault();
			value = Math.Max(valueOrDefault, value);
		}
		minimum = spec.Maximum;
		if (minimum.HasValue)
		{
			double valueOrDefault2 = minimum.GetValueOrDefault();
			value = Math.Min(valueOrDefault2, value);
		}
		return value;
	}

	internal static string RangeText(ModOptionSpec spec)
	{
		double? minimum = spec.Minimum;
		double? maximum = spec.Maximum;
		if (minimum.HasValue)
		{
			double valueOrDefault = minimum.GetValueOrDefault();
			if (maximum.HasValue)
			{
				double valueOrDefault2 = maximum.GetValueOrDefault();
				return Number(valueOrDefault) + " – " + Number(valueOrDefault2);
			}
			double value = valueOrDefault;
			return "≥ " + Number(value);
		}
		if (maximum.HasValue)
		{
			double valueOrDefault2 = maximum.GetValueOrDefault();
			double value2 = valueOrDefault2;
			return "≤ " + Number(value2);
		}
		return string.Empty;
	}

	private static string Number(double value)
	{
		if (value != Math.Floor(value) || !(Math.Abs(value) < 1000000000000000.0))
		{
			return value.ToString("0.###", CultureInfo.InvariantCulture);
		}
		return ((long)value).ToString(CultureInfo.InvariantCulture);
	}
}
