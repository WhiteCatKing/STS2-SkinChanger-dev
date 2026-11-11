using System;
using CznCore.Presentation;
using CznCore.Standalone;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyDefensePresentation
{
	private const string SettingsGroup = "cznenemy_defense_settings";

	private static SharedDefensePresentationRuntime? _runtime;

	internal static bool Accepts(Creature target, bool effectsEnabled)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (effectsEnabled && target.IsMonster)
		{
			return (int)target.Side == 2;
		}
		return false;
	}

	internal static void Initialize()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		if (_runtime == null)
		{
			PresentationTransports.Ensure((PresentationTransport)4);
			SharedSupportPresentation.RegisterMonsters("CznEnemySkin", (Func<Creature, bool>)((Creature target) => Accepts(target, Entry.PresentationSettings.ActionEffectsEnabled)), (Func<Dictionary>)(() => new Dictionary
			{
				[Variant.op_Implicit("settings_group")] = Variant.op_Implicit("cznenemy_defense_settings"),
				[Variant.op_Implicit("effects_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
				[Variant.op_Implicit("particles_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
				[Variant.op_Implicit("sfx_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
				[Variant.op_Implicit("effect_brightness")] = Variant.op_Implicit(Entry.PresentationSettings.EffectBrightness),
				[Variant.op_Implicit("effect_opacity")] = Variant.op_Implicit(1f),
				[Variant.op_Implicit("effect_volume")] = Variant.op_Implicit(1f)
			}));
			_runtime = new SharedDefensePresentationRuntime("CznEnemySkin", (Func<Creature, bool>)((Creature target) => Accepts(target, Entry.PresentationSettings.ActionEffectsEnabled)), (Action<Node2D>)Apply, "cznenemy_defense_settings", (Action<string>)delegate(string message)
			{
				Entry.Log.Warn(message, 1);
			});
			_runtime.ConfigureBreakAudio((Func<Creature, float>)((Creature _) => 1f));
			Entry.PresentationSettings.ActionEffectsEnabledChanged += delegate
			{
				Refresh();
			};
			Entry.PresentationSettings.EffectBrightnessChanged += delegate
			{
				Refresh();
			};
		}
	}

	private static void Apply(Node2D effect)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)effect).Call(StringName.op_Implicit("apply_presentation_settings"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(new Dictionary
		{
			[Variant.op_Implicit("effects_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
			[Variant.op_Implicit("sfx_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
			[Variant.op_Implicit("particles_enabled")] = Variant.op_Implicit(Entry.PresentationSettings.ActionEffectsEnabled),
			[Variant.op_Implicit("effect_brightness")] = Variant.op_Implicit(Entry.PresentationSettings.EffectBrightness),
			[Variant.op_Implicit("effect_opacity")] = Variant.op_Implicit(1f)
		}) });
	}

	private static void Refresh()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		foreach (Node item in val.GetNodesInGroup(StringName.op_Implicit("cznenemy_defense_settings")))
		{
			Node2D val2 = (Node2D)(object)((item is Node2D) ? item : null);
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				Apply(val2);
			}
		}
	}
}
