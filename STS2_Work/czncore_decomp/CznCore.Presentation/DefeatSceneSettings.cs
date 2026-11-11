using CznCore.Settings;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal static class DefeatSceneSettings
{
	internal static void Apply(Node node, SkinCharacterSettings settings)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)node).Call(StringName.op_Implicit("apply_presentation_settings"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(new Dictionary
		{
			[Variant.op_Implicit("effects_enabled")] = Variant.op_Implicit(settings.EffectiveEffectsEnabled),
			[Variant.op_Implicit("particles_enabled")] = Variant.op_Implicit(settings.EffectiveParticlesEnabled),
			[Variant.op_Implicit("sfx_enabled")] = Variant.op_Implicit(settings.EffectiveCombatSfxEnabled),
			[Variant.op_Implicit("effect_volume")] = Variant.op_Implicit(settings.EffectVolume),
			[Variant.op_Implicit("effect_opacity")] = Variant.op_Implicit(settings.EffectOpacity),
			[Variant.op_Implicit("effect_brightness")] = Variant.op_Implicit(settings.EffectBrightness)
		}) });
	}
}
