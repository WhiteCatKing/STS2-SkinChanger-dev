using System.Collections.Generic;

namespace CznCore.Presentation;

internal sealed class PresentationEffectCatalog(ICombatPresentationProfile profile) : ICombatEffectCatalog
{
	public IEnumerable<CombatPresentationEffectCue> EnumerateTransientEffectCues()
	{
		return profile.EnumerateTransientEffectCues();
	}

	public IEnumerable<CombatPresentationSfxCue> EnumerateSfxCues()
	{
		return profile.EnumerateSfxCues();
	}

	public bool TryGetTransientEffects(string group, out IReadOnlyList<CombatPresentationEffectCue> effects)
	{
		return profile.TryGetTransientEffects(group, out effects);
	}

	public bool TryGetSfxCues(string group, out IReadOnlyList<CombatPresentationSfxCue> sfxCues)
	{
		return profile.TryGetSfxCues(group, out sfxCues);
	}
}
