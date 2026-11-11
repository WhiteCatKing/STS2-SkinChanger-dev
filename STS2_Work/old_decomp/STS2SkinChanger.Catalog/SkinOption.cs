using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record SkinOption(string Id, string Name, IReadOnlyDictionary<string, ResourceAsset> Assets, bool IsRuntimeProvider = false, bool IsDirectCharacterRuntimeProvider = false, string? RuntimeImagePath = null, ResourceAsset? ManagedMonsterScene = null, RuntimeMonsterVisualMode? RuntimeMonsterVisualMode = null, string? ProviderId = null, bool IsManagedMonsterRuntimeProfile = false, FrameworkCharacterSkinContract? FrameworkContract = null, bool IsCharacterIconOnly = false, bool IsBaselinePartOverride = false)
{
	public string EffectiveProviderId => ProviderId ?? RuntimeMonsterVisualMode?.ProviderId ?? Id;

	public IReadOnlyList<string> CompositionSourceOptionIds { get; init; } = Array.Empty<string>();

	public IReadOnlyList<string> CompositionSourceProviderIds { get; init; } = Array.Empty<string>();

	public bool IsSessionComposition { get; init; }

	public ManualCharacterVariant? ManualCharacterVariant { get; init; }

	public bool IsComposition => CompositionSourceOptionIds.Count > 0;

	public string? ProviderRootPath { get; init; }
}
