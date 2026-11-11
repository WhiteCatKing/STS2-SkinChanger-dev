using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Catalog;

internal sealed record FrameworkCharacterSkinContract(string ProviderId, string FrameworkAssemblyName, string TargetGroupId, string OptionId, string SkinId, string DisplayName, string DescriptorTypeName, IReadOnlyDictionary<string, string> CharacterResources, IReadOnlyDictionary<string, IReadOnlyList<string>> CharacterResourceLists, IReadOnlyDictionary<string, string> CharacterValues, IReadOnlyList<FrameworkModelSkinContract> Orbs, IReadOnlyList<FrameworkModelSkinContract> Relics)
{
	public IReadOnlySet<string> ResourcePaths => CharacterResources.Values.Concat(CharacterResourceLists.Values.SelectMany((IReadOnlyList<string> paths) => paths)).Concat(Orbs.SelectMany((FrameworkModelSkinContract orb) => orb.ResourcePaths)).Concat(Relics.SelectMany((FrameworkModelSkinContract relic) => relic.ResourcePaths))
		.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
}
