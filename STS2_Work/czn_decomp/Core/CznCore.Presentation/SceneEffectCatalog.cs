using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CznCore.Presentation;

internal sealed class SceneEffectCatalog : ICombatEffectCatalog
{
	private readonly Lazy<Dictionary<string, IReadOnlyList<CombatPresentationEffectCue>>> _effects = new Lazy<Dictionary<string, IReadOnlyList<CombatPresentationEffectCue>>>(() => scenePaths.Select((string path) => (Path: path, Group: Path.GetFileNameWithoutExtension(path))).DistinctBy<(string, string), string>(((string Path, string Group) scene) => scene.Group, StringComparer.Ordinal).ToDictionary<(string, string), string, IReadOnlyList<CombatPresentationEffectCue>>(((string Path, string Group) scene) => scene.Group, ((string Path, string Group) scene) => new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationEffectCue>(Cue(scene.Path, scene.Group)), StringComparer.Ordinal));

	private readonly Lazy<CombatPresentationSfxCue[]> _sfx = new Lazy<CombatPresentationSfxCue[]>(() => (from path in sfxPaths.Distinct<string>(StringComparer.Ordinal)
		select new CombatPresentationSfxCue(path)).ToArray());

	public SceneEffectCatalog(IEnumerable<string> scenePaths, IEnumerable<string> sfxPaths)
	{
	}

	public static CombatPresentationEffectCue Cue(string scenePath, string group)
	{
		return new CombatPresentationEffectCue(scenePath, 0f, 0f, ClearExisting: false, CombatPresentationEffectPalette.Idle, CombatPresentationEffectAnchor.Owner, UseNeutralPmaMaterial: false, UseAttackBoost: false, group, 1f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, AllowSynchronousSceneLoad: true, 0, PreserveSceneRoot: true, CombatPresentationEffectImportance.Normal, 1f, null, NeutralizeAnchorScale: false, CombatPresentationEffectWarmupTier.Starter);
	}

	public IEnumerable<CombatPresentationEffectCue> EnumerateTransientEffectCues()
	{
		return _effects.Value.Values.SelectMany((IReadOnlyList<CombatPresentationEffectCue> cues) => cues);
	}

	public IEnumerable<CombatPresentationSfxCue> EnumerateSfxCues()
	{
		return _sfx.Value;
	}

	public bool TryGetTransientEffects(string group, out IReadOnlyList<CombatPresentationEffectCue> effects)
	{
		return _effects.Value.TryGetValue(group, out effects);
	}

	public bool TryGetSfxCues(string group, out IReadOnlyList<CombatPresentationSfxCue> sfxCues)
	{
		sfxCues = Array.Empty<CombatPresentationSfxCue>();
		return false;
	}
}
