using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CharacterSkinFeatureSupportProfile(bool CombatBodyScalePatch, bool CombatPositionOffsetPatch, bool CombatVoiceHooks, bool CombatEntryAnimation, bool CombatIdleEffects, bool BattleReadyOverlay, bool DeathCgOverlay, bool MeleeApproachMovement = true, bool OfficialAttackTiming = false, bool SuppressNativeAttackCommandPresentation = false)
{
	public static CharacterSkinFeatureSupportProfile Minimal { get; } = new CharacterSkinFeatureSupportProfile(CombatBodyScalePatch: false, CombatPositionOffsetPatch: false, CombatVoiceHooks: false, CombatEntryAnimation: false, CombatIdleEffects: false, BattleReadyOverlay: false, DeathCgOverlay: false, MeleeApproachMovement: false);

	public static CharacterSkinFeatureSupportProfile FullCombatPresentation { get; } = new CharacterSkinFeatureSupportProfile(CombatBodyScalePatch: true, CombatPositionOffsetPatch: false, CombatVoiceHooks: true, CombatEntryAnimation: true, CombatIdleEffects: false, BattleReadyOverlay: true, DeathCgOverlay: true, MeleeApproachMovement: false, OfficialAttackTiming: true);

	[CompilerGenerated]
	private CharacterSkinFeatureSupportProfile(CharacterSkinFeatureSupportProfile original)
	{
		CombatBodyScalePatch = original.CombatBodyScalePatch;
		CombatPositionOffsetPatch = original.CombatPositionOffsetPatch;
		CombatVoiceHooks = original.CombatVoiceHooks;
		CombatEntryAnimation = original.CombatEntryAnimation;
		CombatIdleEffects = original.CombatIdleEffects;
		BattleReadyOverlay = original.BattleReadyOverlay;
		DeathCgOverlay = original.DeathCgOverlay;
		MeleeApproachMovement = original.MeleeApproachMovement;
		OfficialAttackTiming = original.OfficialAttackTiming;
		SuppressNativeAttackCommandPresentation = original.SuppressNativeAttackCommandPresentation;
	}
}
