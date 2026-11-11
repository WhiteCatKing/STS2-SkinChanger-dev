using System;
using Godot;

namespace CznCore.Presentation;

internal sealed record CharacterEffectEngineOptions(string OwnerId, ICombatEffectCatalog Catalog, CombatWarmupPolicyProfile Policy, ISkinTimingProbe Timing, Func<bool> EffectsEnabled, Action<Node2D, CombatPresentationEffectCue> Start)
{
	public Func<bool> SfxEnabled { get; init; } = () => false;

	public Func<string, bool> PrewarmSfx { get; init; } = (string _) => true;

	public Func<int> CountEffectTargets { get; init; } = () => 1;

	public Func<CombatPresentationEffectCue, bool> ShouldWarmupCue { get; init; } = (CombatPresentationEffectCue _) => true;

	public Func<bool> ShouldDeferWarmup { get; init; } = () => false;

	public Func<Node2D, Node, int> AttachLayers { get; init; } = (Node2D _, Node _) => 0;

	public Action<Node, CombatPresentationEffectCue> ApplyMaterial { get; init; } = delegate
	{
	};

	public Func<string, PackedScene?>? BuildScene { get; init; }

	public string? MetaKeyPrefix { get; init; }

	public Func<CombatPresentationEffectCue, int>? PoolCap { get; init; }

	public Func<CombatPresentationEffectCue, CombatPresentationEffectCue> ResolveWarmupCue { get; init; } = (CombatPresentationEffectCue cue) => cue;

	public bool SeedEffectsDuringLimitedPreload { get; init; } = true;

	public Func<float>? EntryDurationSeconds { get; init; }

	public Func<bool> CombatWarmupEnabled { get; init; } = () => true;

	public Action? AfterEntryWarmups { get; init; }
}
