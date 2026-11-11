using System;

namespace CznCore.Presentation;

internal sealed record CharacterAppearanceVariant(string Id, string DisplayName, Func<CharacterSkinProfile, CharacterSkinProfile> Resolve)
{
	public ILocalizedText? LocalizedDisplayName { get; init; }
}
