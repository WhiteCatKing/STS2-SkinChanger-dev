using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal sealed class ModOptionCatalog
{
	private sealed record CacheSlot(DateTime Stamp, long Length, ModOptionCatalog Catalog);

	internal const string DeclarationKey = "mlm_options";

	private static readonly HashSet<string> ReservedKeys = new HashSet<string>(StringComparer.Ordinal)
	{
		"id", "name", "author", "description", "version", "has_pck", "has_dll", "dependencies", "affects_gameplay", "min_game_version",
		"mlm_options", "loadPriority", "load_priority", "incompatibilities"
	};

	private static readonly Dictionary<string, CacheSlot> Cache = new Dictionary<string, CacheSlot>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

	internal static ModOptionCatalog Empty { get; } = new ModOptionCatalog(string.Empty, Array.Empty<ModOptionSpec>(), new Dictionary<string, JsonNode>(StringComparer.Ordinal));

	internal string ManifestFile { get; }

	internal IReadOnlyList<ModOptionSpec> Specs { get; }

	internal IReadOnlyDictionary<string, JsonNode?> Values { get; }

	internal bool IsEmpty => Specs.Count == 0;

	private ModOptionCatalog(string manifestFile, IReadOnlyList<ModOptionSpec> specs, IReadOnlyDictionary<string, JsonNode?> values)
	{
		ManifestFile = manifestFile;
		Specs = specs;
		Values = values;
	}

	internal ModOptionSpec? Find(string key)
	{
		return Specs.FirstOrDefault((ModOptionSpec spec) => string.Equals(spec.Key, key, StringComparison.Ordinal));
	}

	internal static ModOptionCatalog For(Mod mod)
	{
		try
		{
			string text = ModManifestReader.FindManifestFile(mod);
			if (text == null)
			{
				return Empty;
			}
			FileInfo fileInfo = new FileInfo(text);
			if (Cache.TryGetValue(text, out CacheSlot value) && value.Stamp == fileInfo.LastWriteTimeUtc && value.Length == fileInfo.Length)
			{
				return value.Catalog;
			}
			ModOptionCatalog modOptionCatalog = Parse(text);
			Cache[text] = new CacheSlot(fileInfo.LastWriteTimeUtc, fileInfo.Length, modOptionCatalog);
			return modOptionCatalog;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not read launch options for {mod.manifest?.id ?? "<unknown>"}: {ex.Message}");
			return Empty;
		}
	}

	internal static void Invalidate(string manifestFile)
	{
		Cache.Remove(manifestFile);
	}

	private static ModOptionCatalog Parse(string file)
	{
		if (!(JsonNode.Parse(File.ReadAllText(file, Encoding.UTF8), null) is JsonObject jsonObject))
		{
			return Empty;
		}
		Dictionary<string, JsonNode> dictionary = new Dictionary<string, JsonNode>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, JsonNode> item in jsonObject)
		{
			item.Deconstruct(out var key, out var value);
			string key2 = key;
			dictionary[key2] = value?.DeepClone();
		}
		List<ModOptionSpec> list = new List<ModOptionSpec>();
		if (jsonObject["mlm_options"] is JsonArray jsonArray)
		{
			foreach (JsonNode item2 in jsonArray)
			{
				if (item2 is JsonObject declaration)
				{
					ModOptionSpec spec = ParseDeclaration(declaration);
					if (spec != null && list.All((ModOptionSpec existing) => !string.Equals(existing.Key, spec.Key, StringComparison.Ordinal)))
					{
						list.Add(spec);
					}
				}
			}
		}
		Discover(dictionary, list);
		return new ModOptionCatalog(file, list, dictionary);
	}

	private static ModOptionSpec? ParseDeclaration(JsonObject declaration)
	{
		string text = TryReadString(declaration, "key")?.Trim();
		if (string.IsNullOrWhiteSpace(text) || ReservedKeys.Contains(text))
		{
			return null;
		}
		List<string> list = new List<string>();
		if (declaration["values"] is JsonArray jsonArray)
		{
			foreach (JsonNode item in jsonArray)
			{
				if (item != null)
				{
					string text2 = ModOptionValues.Display(item);
					if (text2 != null && text2.Length > 0 && !list.Contains<string>(text2, StringComparer.Ordinal))
					{
						list.Add(text2);
					}
				}
			}
		}
		JsonNode jsonNode = declaration["default"]?.DeepClone();
		if (!TryReadType(TryReadString(declaration, "type"), jsonNode, list.Count > 0, out var type))
		{
			return null;
		}
		ModOptionSpec obj = new ModOptionSpec
		{
			Key = text,
			Type = type,
			Name = TryReadString(declaration, "name"),
			Description = TryReadString(declaration, "description"),
			Section = TryReadString(declaration, "section"),
			DeclaredDefault = jsonNode,
			Minimum = TryReadNumber(declaration, "min"),
			Maximum = TryReadNumber(declaration, "max"),
			Choices = list
		};
		JsonNode? jsonNode2 = declaration["advanced"];
		obj.Advanced = jsonNode2 != null && jsonNode2.GetValueKind() == JsonValueKind.True;
		obj.Declared = true;
		return obj;
	}

	private static void Discover(Dictionary<string, JsonNode?> values, List<ModOptionSpec> specs)
	{
		foreach (KeyValuePair<string, JsonNode> value in values)
		{
			var (key, node) = (KeyValuePair<string, JsonNode>)(ref value);
			if (!ReservedKeys.Contains(key) && !specs.Any((ModOptionSpec spec) => string.Equals(spec.Key, key, StringComparison.Ordinal)) && TryInferType(node, out var type) && type == ModOptionType.Bool)
			{
				specs.Add(new ModOptionSpec
				{
					Key = key,
					Type = type,
					Advanced = true,
					Declared = false
				});
			}
		}
	}

	private static bool TryReadType(string? declared, JsonNode? fallback, bool hasChoices, out ModOptionType type)
	{
		switch (declared?.Trim().ToLowerInvariant())
		{
		case "bool":
		case "boolean":
			type = ModOptionType.Bool;
			return true;
		case "integer":
		case "int":
			type = ModOptionType.Int;
			return true;
		case "number":
		case "double":
		case "float":
			type = ModOptionType.Float;
			return true;
		case "text":
		case "string":
			type = ModOptionType.String;
			return true;
		case "enum":
		case "choice":
			type = ModOptionType.Enum;
			return true;
		default:
			if (hasChoices)
			{
				type = ModOptionType.Enum;
				return true;
			}
			return TryInferType(fallback, out type);
		}
	}

	private static bool TryInferType(JsonNode? node, out ModOptionType type)
	{
		type = ModOptionType.String;
		if (!(node is JsonValue jsonValue))
		{
			return false;
		}
		switch (node.GetValueKind())
		{
		case JsonValueKind.True:
		case JsonValueKind.False:
			type = ModOptionType.Bool;
			return true;
		case JsonValueKind.Number:
		{
			type = (jsonValue.TryGetValue<long>(out var _) ? ModOptionType.Int : ModOptionType.Float);
			return true;
		}
		case JsonValueKind.String:
			type = ModOptionType.String;
			return true;
		default:
			return false;
		}
	}

	private static string? TryReadString(JsonObject declaration, string key)
	{
		JsonNode? jsonNode = declaration[key];
		if (jsonNode == null || jsonNode.GetValueKind() != JsonValueKind.String)
		{
			return null;
		}
		return declaration[key].GetValue<string>();
	}

	private static double? TryReadNumber(JsonObject declaration, string key)
	{
		if (!(declaration[key] is JsonValue jsonValue) || !jsonValue.TryGetValue<double>(out var value))
		{
			return null;
		}
		return value;
	}
}
