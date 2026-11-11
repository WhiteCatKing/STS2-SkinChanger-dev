using System.Collections.Generic;

namespace CznCore.Presentation;

internal interface ICombatEffectCatalog
{
	IEnumerable<CombatPresentationEffectCue> EnumerateTransientEffectCues();

	IEnumerable<CombatPresentationSfxCue> EnumerateSfxCues();

	bool TryGetTransientEffects(string group, out IReadOnlyList<CombatPresentationEffectCue> effects);

	bool TryGetSfxCues(string group, out IReadOnlyList<CombatPresentationSfxCue> sfxCues);
}
