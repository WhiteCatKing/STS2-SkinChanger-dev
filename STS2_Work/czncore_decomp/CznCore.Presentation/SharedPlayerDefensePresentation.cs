using CznCore.Standalone;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal static class SharedPlayerDefensePresentation
{
	private static SharedDefensePresentationRuntime? _runtime;

	public static void Ensure()
	{
		if (_runtime == null)
		{
			PresentationTransports.Ensure(PresentationTransport.Defense);
			_runtime = new SharedDefensePresentationRuntime("CznCore.Players", Accepts, Apply, "czn_player_defense_settings", delegate(string message)
			{
				SkinLog.Warn(message);
			})
			{
				PlayPartialBlockFeedback = false
			};
			_runtime.ConfigureBreakAudio(BreakVolume);
		}
	}

	internal static bool Accepts(Creature target)
	{
		ICharacterSkinAudioSettings characterSkinAudioSettings = SharedSupportPresentation.ResolvePlayerSettings(target);
		if (characterSkinAudioSettings != null && characterSkinAudioSettings.EffectiveEffectsEnabled)
		{
			return Allows(target, characterSkinAudioSettings, CombatPresentationChannel.Effects);
		}
		return false;
	}

	internal static float BreakVolume(Creature target)
	{
		ICharacterSkinAudioSettings characterSkinAudioSettings = SharedSupportPresentation.ResolvePlayerSettings(target);
		if (characterSkinAudioSettings == null || !characterSkinAudioSettings.EffectiveCombatSfxEnabled || !Allows(target, characterSkinAudioSettings, CombatPresentationChannel.CombatSfx))
		{
			return 0f;
		}
		return characterSkinAudioSettings.EffectVolume;
	}

	private static bool Allows(Creature target, ICharacterSkinAudioSettings settings, CombatPresentationChannel channel)
	{
		return new CombatPresentationAudiencePolicy(settings, (Player? player) => player == target.Player).Allows(target.Player, channel);
	}

	private static void Apply(Node2D effect, Creature target)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		ICharacterSkinAudioSettings characterSkinAudioSettings = SharedSupportPresentation.ResolvePlayerSettings(target);
		if (characterSkinAudioSettings != null)
		{
			((GodotObject)effect).Call(StringName.op_Implicit("apply_presentation_settings"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(new Dictionary
			{
				[Variant.op_Implicit("effects_enabled")] = Variant.op_Implicit(characterSkinAudioSettings.EffectiveEffectsEnabled),
				[Variant.op_Implicit("particles_enabled")] = Variant.op_Implicit(characterSkinAudioSettings.EffectiveParticlesEnabled),
				[Variant.op_Implicit("effect_brightness")] = Variant.op_Implicit(characterSkinAudioSettings.EffectBrightness),
				[Variant.op_Implicit("effect_opacity")] = Variant.op_Implicit(characterSkinAudioSettings.EffectOpacity)
			}) });
		}
	}
}
