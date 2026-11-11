using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record CardSkinOption(string Id, string Name, IReadOnlyDictionary<string, string> NormalPortraits, IReadOnlyDictionary<string, AncientCardPortrait> AncientPortraits, IReadOnlyDictionary<string, ResourceAsset>? PckAssets = null, string? ProviderRootPath = null, string? ProviderId = null, IReadOnlyDictionary<string, CardPresentationDefinition>? Presentations = null)
{
	public StatefulCardArtContract? StatefulArt { get; init; }

	public IReadOnlyDictionary<string, CardSurfaceDefinition> CardSurfaces { get; init; } = new Dictionary<string, CardSurfaceDefinition>(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyDictionary<string, ResourceAsset> Assets { get; init; } = PckAssets ?? new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyDictionary<string, CardPresentationDefinition> CardPresentations { get; init; } = Presentations ?? new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyDictionary<string, string> CardNames { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public string GetNameForCard(string cardType)
	{
		return CardNames.GetValueOrDefault(cardType) ?? Name;
	}

	public CardSkinOption Merge(CardSkinOption other)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(NormalPortraits, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> normalPortrait in other.NormalPortraits)
		{
			dictionary[normalPortrait.Key] = normalPortrait.Value;
		}
		Dictionary<string, AncientCardPortrait> dictionary2 = new Dictionary<string, AncientCardPortrait>(AncientPortraits, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, AncientCardPortrait> ancientPortrait in other.AncientPortraits)
		{
			dictionary2[ancientPortrait.Key] = ancientPortrait.Value;
		}
		Dictionary<string, ResourceAsset> dictionary3 = new Dictionary<string, ResourceAsset>(Assets, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, ResourceAsset> asset in other.Assets)
		{
			dictionary3[asset.Key] = asset.Value;
		}
		Dictionary<string, CardPresentationDefinition> dictionary4 = new Dictionary<string, CardPresentationDefinition>(CardPresentations, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, CardPresentationDefinition> cardPresentation in other.CardPresentations)
		{
			dictionary4[cardPresentation.Key] = cardPresentation.Value;
		}
		Dictionary<string, string> dictionary5 = new Dictionary<string, string>(CardNames, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, CardSurfaceDefinition> dictionary6 = new Dictionary<string, CardSurfaceDefinition>(CardSurfaces, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, CardSurfaceDefinition> cardSurface in other.CardSurfaces)
		{
			dictionary6[cardSurface.Key] = cardSurface.Value;
		}
		foreach (KeyValuePair<string, string> cardName in other.CardNames)
		{
			dictionary5[cardName.Key] = cardName.Value;
		}
		return this with
		{
			NormalPortraits = dictionary,
			AncientPortraits = dictionary2,
			Assets = dictionary3,
			CardPresentations = dictionary4,
			CardNames = dictionary5,
			CardSurfaces = dictionary6,
			ProviderRootPath = (ProviderRootPath ?? other.ProviderRootPath),
			ProviderId = (ProviderId ?? other.ProviderId),
			StatefulArt = (StatefulArt ?? other.StatefulArt)
		};
	}

	public string? GetPortraitPath(string cardType, bool useAncientStyle)
	{
		if (AncientPortraits.TryGetValue(cardType, out AncientCardPortrait value))
		{
			string text = (useAncientStyle ? value.AncientPortrait : value.NormalPortrait);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return NormalPortraits.GetValueOrDefault(cardType);
	}
}
