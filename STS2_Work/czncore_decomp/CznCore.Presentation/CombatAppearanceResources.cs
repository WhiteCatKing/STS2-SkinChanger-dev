using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal static class CombatAppearanceResources
{
	private static readonly ConditionalWeakTable<CharacterSkinProfile, ConditionalWeakTable<CombatPresentationEffectCue, CombatPresentationEffectCue>> Cache = new ConditionalWeakTable<CharacterSkinProfile, ConditionalWeakTable<CombatPresentationEffectCue, CombatPresentationEffectCue>>();

	internal static CombatPresentationEffectCue ResolveEffect(CharacterSkinProfile profile, Player? owner, CombatPresentationEffectCue cue)
	{
		if (owner != null)
		{
			return ResolveEffect(profile.For(owner), cue);
		}
		return cue;
	}

	internal static CombatPresentationEffectCue ResolveEffect(CharacterSkinProfile selected, CombatPresentationEffectCue cue)
	{
		string scenePath = cue.Source.ScenePath;
		if (scenePath == null)
		{
			return cue;
		}
		if (!selected.CombatEffectOverrides.TryGetValue(new CombatAppearanceEffectKey(scenePath, cue.ZIndex), out string value))
		{
			return cue;
		}
		ConditionalWeakTable<CombatPresentationEffectCue, CombatPresentationEffectCue> orCreateValue = Cache.GetOrCreateValue(selected);
		if (!orCreateValue.TryGetValue(cue, out var value2))
		{
			return AddCue(orCreateValue, cue, value);
		}
		return value2;
	}

	private static CombatPresentationEffectCue AddCue(ConditionalWeakTable<CombatPresentationEffectCue, CombatPresentationEffectCue> cues, CombatPresentationEffectCue cue, string replacement)
	{
		return cues.GetValue(cue, (CombatPresentationEffectCue original) => original with
		{
			Source = CombatPresentationEffectSource.Scene(replacement)
		});
	}
}
