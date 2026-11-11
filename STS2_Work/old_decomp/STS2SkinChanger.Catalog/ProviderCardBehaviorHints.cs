using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ProviderCardBehaviorHints(IReadOnlyDictionary<string, AncientCardPortrait> AncientPortraits, IReadOnlyDictionary<string, CardPresentationDefinition> Presentations);
