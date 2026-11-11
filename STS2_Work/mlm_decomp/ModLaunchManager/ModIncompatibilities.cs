using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModIncompatibilities
{
	internal const string ManifestKey = "incompatibilities";

	internal static IReadOnlyList<ModIncompatibility> Parse(JsonNode? node, string? ownerId)
	{
		if (!(node is JsonArray jsonArray))
		{
			return Array.Empty<ModIncompatibility>();
		}
		List<ModIncompatibility> list = new List<ModIncompatibility>();
		foreach (JsonNode item in jsonArray)
		{
			ModIncompatibility modIncompatibility = ((item is JsonObject declaration) ? FromObject(declaration) : ((!(item is JsonValue jsonValue) || jsonValue.GetValueKind() != JsonValueKind.String) ? null : FromId(jsonValue.GetValue<string>())));
			ModIncompatibility modIncompatibility2 = modIncompatibility;
			if (modIncompatibility2 != null && !string.Equals(modIncompatibility2.Id, ownerId, StringComparison.Ordinal) && !list.Contains(modIncompatibility2))
			{
				list.Add(modIncompatibility2);
			}
		}
		return list;
	}

	internal static string PairKey(string first, string second)
	{
		if (string.CompareOrdinal(first, second) > 0)
		{
			return second + "\n" + first;
		}
		return first + "\n" + second;
	}

	internal static int? CompareVersions(string? left, string? right)
	{
		if (!TryParseVersion(left, out int[] numbers, out string[] prerelease) || !TryParseVersion(right, out int[] numbers2, out string[] prerelease2))
		{
			return null;
		}
		for (int i = 0; i < Math.Max(numbers.Length, numbers2.Length); i++)
		{
			int num = NumberAt(numbers, i).CompareTo(NumberAt(numbers2, i));
			if (num != 0)
			{
				return num;
			}
		}
		if (prerelease.Length == 0 || prerelease2.Length == 0)
		{
			return prerelease2.Length.CompareTo(prerelease.Length);
		}
		for (int j = 0; j < Math.Min(prerelease.Length, prerelease2.Length); j++)
		{
			int num2 = ComparePrereleaseIdentifier(prerelease[j], prerelease2[j]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return prerelease.Length.CompareTo(prerelease2.Length);
	}

	internal static bool IsReadableVersion(string? text)
	{
		int[] numbers;
		string[] prerelease;
		return TryParseVersion(text, out numbers, out prerelease);
	}

	private static ModIncompatibility? FromId(string id)
	{
		if (!string.IsNullOrWhiteSpace(id))
		{
			return new ModIncompatibility(id.Trim(), null, null, null);
		}
		return null;
	}

	private static ModIncompatibility? FromObject(JsonObject declaration)
	{
		string text = ReadText(declaration, "id");
		if (text == null)
		{
			return null;
		}
		return new ModIncompatibility(text, ReadText(declaration, "min_version"), ReadText(declaration, "max_version"), ReadText(declaration, "reason"));
	}

	private static string? ReadText(JsonObject declaration, string key)
	{
		if (declaration[key] is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String)
		{
			string text = jsonValue.GetValue<string>().Trim();
			if (text != null && text.Length > 0)
			{
				return text;
			}
		}
		return null;
	}

	private static bool TryParseVersion(string? text, out int[] numbers, out string[] prerelease)
	{
		numbers = Array.Empty<int>();
		prerelease = Array.Empty<string>();
		string text2 = text?.Trim() ?? string.Empty;
		if (text2.StartsWith('v') || text2.StartsWith('V'))
		{
			text2 = text2.Substring(1);
		}
		int num = text2.IndexOf('+');
		if (num >= 0)
		{
			text2 = text2.Substring(0, num);
		}
		int num2 = text2.IndexOf('-');
		if (num2 >= 0)
		{
			prerelease = text2.Substring(num2 + 1).Split('.');
			text2 = text2.Substring(0, num2);
			if (prerelease.Any(string.IsNullOrEmpty))
			{
				return false;
			}
		}
		string[] array = text2.Split('.');
		if (array.Length > 4)
		{
			return false;
		}
		numbers = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (!int.TryParse(array[i], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static int NumberAt(int[] numbers, int index)
	{
		if (index >= numbers.Length)
		{
			return 0;
		}
		return numbers[index];
	}

	private static int ComparePrereleaseIdentifier(string left, string right)
	{
		int result;
		bool num = int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out result);
		int result2;
		bool flag = int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out result2);
		if (num)
		{
			if (flag)
			{
				return result.CompareTo(result2);
			}
			return -1;
		}
		if (flag)
		{
			return 1;
		}
		return string.CompareOrdinal(left, right);
	}

	internal static IReadOnlyList<ModIncompatibility> Declared(Mod mod)
	{
		try
		{
			foreach (var (a, node) in ModOptionCatalog.For(mod).Values)
			{
				if (!string.Equals(a, "incompatibilities", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				IReadOnlyList<ModIncompatibility> readOnlyList = Parse(node, mod.manifest?.id);
				foreach (ModIncompatibility item in readOnlyList.Where((ModIncompatibility rule) => rule.HasUnreadableBound))
				{
					GD.PushWarning($"{"[ModLaunchManager]"} {mod.manifest?.id} declares an incompatibility with {item.Id} for versions \"{item.RangeText}\"; a bound " + "there is not a readable version, so it is ignored and the rule covers more versions.");
				}
				return readOnlyList;
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not read the incompatibilities of {mod.manifest?.id ?? "<unknown>"}: {ex.Message}");
		}
		return Array.Empty<ModIncompatibility>();
	}
}
