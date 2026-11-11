using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Catalog;

internal static class CardLayoutVariantPolicy
{
	private sealed record Request(string ProviderNamespace, string CardType, string PortraitPath);

	private static readonly Request[] Requests = new Request[1]
	{
		new Request("SilentCardAnimerRework", "EscapePlan", "res://generated/assets/card_art/MegaCrit.Sts2.Core.Models.Cards.EscapePlan_card_art.png")
	};

	public static IReadOnlyList<CardSkinOption> Expand(CardSkinOption source, string providerNamespace)
	{
		CardSkinOption cardSkinOption = source;
		List<CardSkinOption> list = new List<CardSkinOption>();
		Request[] requests = Requests;
		foreach (Request request in requests)
		{
			if (request.ProviderNamespace.Equals(providerNamespace, StringComparison.OrdinalIgnoreCase) && !source.CardPresentations.ContainsKey(request.CardType) && !source.AncientPortraits.ContainsKey(request.CardType) && source.NormalPortraits.TryGetValue(request.CardType, out string portrait) && request.PortraitPath.Equals(portrait, StringComparison.OrdinalIgnoreCase))
			{
				Dictionary<string, CardPresentationDefinition> cardPresentations = new Dictionary<string, CardPresentationDefinition>(cardSkinOption.CardPresentations, StringComparer.OrdinalIgnoreCase) { [request.CardType] = new CardPresentationDefinition(UseAncientLayout: false, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: true, null, null, null, null, null, null, null, null, null) };
				Dictionary<string, string> cardNames = new Dictionary<string, string>(cardSkinOption.CardNames, StringComparer.OrdinalIgnoreCase) { [request.CardType] = CardSkinOptionNamingPolicy.Build(source.Name, null, 1, 2) };
				cardSkinOption = cardSkinOption with
				{
					CardPresentations = cardPresentations,
					CardNames = cardNames
				};
				list.Add(new CardSkinOption(source.Id + "::card-layout:" + request.CardType.ToLowerInvariant() + ":ancient", CardSkinOptionNamingPolicy.Build(source.Name, null, 2, 2), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [request.CardType] = portrait }, new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase), source.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => pair.Key.Equals(portrait, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, ResourceAsset>, string, ResourceAsset>((KeyValuePair<string, ResourceAsset> pair) => pair.Key, (KeyValuePair<string, ResourceAsset> pair) => pair.Value, StringComparer.OrdinalIgnoreCase), source.ProviderRootPath, source.ProviderId, new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase) { [request.CardType] = new CardPresentationDefinition(UseAncientLayout: true, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: false, null, null, null, null, null, null, null, null, null) }));
			}
		}
		list.Insert(0, cardSkinOption);
		return list;
	}
}
