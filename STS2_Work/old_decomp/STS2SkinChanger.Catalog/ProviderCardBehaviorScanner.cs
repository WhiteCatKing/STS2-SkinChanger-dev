using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Catalog;

internal static class ProviderCardBehaviorScanner
{
	private enum ProviderCardBehavior
	{
		None,
		AncientPortrait,
		FullFrameArt
	}

	private sealed record FrameOverlayGeometry(float? OffsetTop, float? OffsetBottom, float? OffsetLeft, float? OffsetRight, float? ScaleX, float? ScaleY);

	public static ProviderCardBehaviorHints Scan(string? providerRoot, IReadOnlyCollection<ResourceAsset> assets)
	{
		Dictionary<string, AncientCardPortrait> dictionary = new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, CardPresentationDefinition> dictionary2 = new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot) || assets.Count == 0)
		{
			return new ProviderCardBehaviorHints(dictionary, dictionary2);
		}
		Dictionary<string, ResourceAsset> dictionary3 = assets.Where((ResourceAsset asset) => IsImage(asset.SourcePath) && HasDirectoryToken(asset.SourcePath, "cards")).GroupBy<ResourceAsset, string>((ResourceAsset asset) => NormalizeToken(FileStem(asset.SourcePath)), StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, ResourceAsset>, string, ResourceAsset>((IGrouping<string, ResourceAsset> group) => group.Key, (IGrouping<string, ResourceAsset> group) => group.OrderBy((ResourceAsset asset) => asset.SourcePath.Length).First(), StringComparer.OrdinalIgnoreCase);
		if (dictionary3.Count == 0)
		{
			return new ProviderCardBehaviorHints(dictionary, dictionary2);
		}
		ResourceAsset[] source = assets.Where((ResourceAsset asset) => IsImage(asset.SourcePath) && HasDirectoryToken(asset.SourcePath, "border")).ToArray();
		foreach (string item in Directory.EnumerateFiles(providerRoot, "*.json", SearchOption.TopDirectoryOnly))
		{
			try
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(item).TrimStart('\ufeff'));
				if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
				{
					continue;
				}
				FrameOverlayGeometry frameOverlayGeometry = ReadFrameOverlayGeometry(jsonDocument.RootElement);
				foreach (JsonProperty item2 in jsonDocument.RootElement.EnumerateObject())
				{
					if (item2.Value.ValueKind != JsonValueKind.Object)
					{
						continue;
					}
					ProviderCardBehavior providerCardBehavior = Classify(item2.Name);
					if (providerCardBehavior == ProviderCardBehavior.None)
					{
						continue;
					}
					foreach (JsonProperty item3 in item2.Value.EnumerateObject())
					{
						if (!IsEnabled(item3.Value) || !dictionary3.TryGetValue(NormalizeToken(item3.Name), out var value))
						{
							continue;
						}
						string name = item3.Name;
						switch (providerCardBehavior)
						{
						case ProviderCardBehavior.AncientPortrait:
							dictionary[name] = new AncientCardPortrait(null, value.SourcePath);
							break;
						case ProviderCardBehavior.FullFrameArt:
						{
							string category = GetCategory(value.SourcePath, "cards");
							ResourceAsset resourceAsset = source.FirstOrDefault((ResourceAsset asset) => GetCategory(asset.SourcePath, "border").Equals(category, StringComparison.OrdinalIgnoreCase));
							string sourcePath = value.SourcePath;
							string frameOverlay = resourceAsset?.SourcePath;
							bool? frameVisible = true;
							bool? textBackgroundVisible = true;
							bool? portraitVisible = false;
							bool? portraitBorderVisible = false;
							float? offsetTop = frameOverlayGeometry.OffsetTop;
							float? offsetBottom = frameOverlayGeometry.OffsetBottom;
							float? offsetLeft = frameOverlayGeometry.OffsetLeft;
							float? offsetRight = frameOverlayGeometry.OffsetRight;
							float? scaleX = frameOverlayGeometry.ScaleX;
							float? scaleY = frameOverlayGeometry.ScaleY;
							dictionary2[name] = new CardPresentationDefinition(UseAncientLayout: false, sourcePath, null, null, null, null, null, null, null, null, null, null, frameVisible, null, textBackgroundVisible, portraitBorderVisible, null, null, null, null, null, null, UseFullFrameArt: true, UseExpandedPortraitLayout: false, frameOverlay, portraitVisible, offsetTop, offsetBottom, offsetLeft, offsetRight, scaleX, scaleY, null);
							break;
						}
						}
					}
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
			{
			}
		}
		return new ProviderCardBehaviorHints(dictionary, dictionary2);
	}

	private static ProviderCardBehavior Classify(string name)
	{
		string text = NormalizeToken(name);
		if (!text.Contains("card", StringComparison.OrdinalIgnoreCase))
		{
			return ProviderCardBehavior.None;
		}
		if (text.Contains("ancient", StringComparison.OrdinalIgnoreCase) && (text.Contains("portrait", StringComparison.OrdinalIgnoreCase) || text.Contains("image", StringComparison.OrdinalIgnoreCase)))
		{
			return ProviderCardBehavior.AncientPortrait;
		}
		if (!text.Contains("border", StringComparison.OrdinalIgnoreCase) || (!text.Contains("enabled", StringComparison.OrdinalIgnoreCase) && !text.Contains("replace", StringComparison.OrdinalIgnoreCase) && !text.Contains("custom", StringComparison.OrdinalIgnoreCase)))
		{
			return ProviderCardBehavior.None;
		}
		return ProviderCardBehavior.FullFrameArt;
	}

	private static bool IsEnabled(JsonElement value)
	{
		bool result;
		return value.ValueKind switch
		{
			JsonValueKind.True => true, 
			JsonValueKind.String => bool.TryParse(value.GetString(), out result) && result, 
			_ => false, 
		};
	}

	private static FrameOverlayGeometry ReadFrameOverlayGeometry(JsonElement root)
	{
		return new FrameOverlayGeometry(ReadNumber(root, "customBorderOffsetTop"), ReadNumber(root, "customBorderOffsetBottom"), ReadNumber(root, "customBorderOffsetLeft"), ReadNumber(root, "customBorderOffsetRight"), ReadNumber(root, "customBorderScaleX"), ReadNumber(root, "customBorderScaleY"));
	}

	private static float? ReadNumber(JsonElement root, string name)
	{
		foreach (JsonProperty item in root.EnumerateObject())
		{
			if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				if (item.Value.ValueKind == JsonValueKind.Number && item.Value.TryGetSingle(out var value))
				{
					return value;
				}
				if (item.Value.ValueKind == JsonValueKind.String && float.TryParse(item.Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				{
					return value;
				}
			}
		}
		return null;
	}

	private static string GetCategory(string path, string marker)
	{
		string[] array = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i + 1 < array.Length; i++)
		{
			if (array[i].Equals(marker, StringComparison.OrdinalIgnoreCase))
			{
				return array[i + 1];
			}
		}
		return string.Empty;
	}

	private static bool HasDirectoryToken(string path, string token)
	{
		return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any((string part) => part.Equals(token, StringComparison.OrdinalIgnoreCase));
	}

	private static string FileStem(string path)
	{
		int num = path.LastIndexOf('/') + 1;
		string text = path.Substring(num, path.Length - num);
		int num2 = text.LastIndexOf('.');
		if (num2 >= 0)
		{
			return text.Substring(0, num2);
		}
		return text;
	}

	private static bool IsImage(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".tres", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".res", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}
}
