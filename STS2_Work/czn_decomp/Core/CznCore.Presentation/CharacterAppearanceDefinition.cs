using System.Collections.Generic;

namespace CznCore.Presentation;

internal sealed record CharacterAppearanceDefinition(string ProviderId, string DisplayName, IReadOnlyList<CharacterAppearanceVariant> Variants)
{
	public ILocalizedText? LocalizedDisplayName { get; init; }
}
