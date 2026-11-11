using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using CznCore.Standalone;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Ui;
using MegaCrit.Sts2.Core.TestSupport;

namespace CznCore.Presentation;

internal static class SharedSupportPresentation
{
	private sealed record Owner(Func<Creature, bool> Accepts, Func<ICharacterSkinAudioSettings>? Settings, Func<Dictionary>? MonsterSettings = null);

	private sealed class Scope(Creature target, string route, ICharacterSkinAudioSettings? settings, Func<Dictionary>? monsterSettings = null)
	{
		private bool _visualConsumed;

		private bool _soundConsumed;

		private readonly CombatPresentationAudiencePolicy? _audience = ((settings == null) ? null : new CombatPresentationAudiencePolicy(settings, (Player? p) => p == target.Player));

		public bool Matches(string expectedRoute, Creature? receiver = null)
		{
			if (route == expectedRoute)
			{
				if (receiver != null)
				{
					return target == receiver;
				}
				return true;
			}
			return false;
		}

		public bool TryPlay(bool audio)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Unknown result type (might be due to invalid IL or missing references)
			//IL_032b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Unknown result type (might be due to invalid IL or missing references)
			//IL_0339: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0347: Unknown result type (might be due to invalid IL or missing references)
			//IL_034c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_035e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0362: Unknown result type (might be due to invalid IL or missing references)
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_036e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_0377: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_038a: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
			if (NonInteractiveMode.IsActive || TestMode.IsOn || CombatManager.Instance.IsEnding)
			{
				return false;
			}
			if (audio ? _soundConsumed : _visualConsumed)
			{
				return false;
			}
			object obj = monsterSettings?.Invoke();
			Variant val7;
			if (obj == null)
			{
				Dictionary val6 = new Dictionary
				{
					[Variant.op_Implicit("effects_enabled")] = Variant.op_Implicit(settings.EffectiveEffectsEnabled),
					[Variant.op_Implicit("particles_enabled")] = Variant.op_Implicit(settings.EffectiveParticlesEnabled),
					[Variant.op_Implicit("sfx_enabled")] = Variant.op_Implicit(settings.EffectiveCombatSfxEnabled),
					[Variant.op_Implicit("effect_volume")] = Variant.op_Implicit(settings.EffectVolume),
					[Variant.op_Implicit("effect_brightness")] = Variant.op_Implicit(settings.EffectBrightness)
				};
				val7 = Variant.op_Implicit("effect_opacity");
				val6[val7] = Variant.op_Implicit(settings.EffectOpacity);
				obj = (object)val6;
			}
			Dictionary val8 = (Dictionary)obj;
			val7 = val8[Variant.op_Implicit(audio ? "sfx_enabled" : "effects_enabled")];
			if (!((Variant)(ref val7)).AsBool())
			{
				return false;
			}
			if (_audience != null && !_audience.Allows(target.Player, (!audio) ? CombatPresentationChannel.Effects : CombatPresentationChannel.CombatSfx))
			{
				return false;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val9 = ((instance != null) ? instance.GetCreatureNode(target) : null);
			if (val9 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val9) || !((Node)val9).IsInsideTree())
			{
				return false;
			}
			try
			{
				Node2D val10 = new CombatCreatureSpineResolver().Find(val9);
				Node2D val11 = (target.IsMonster ? MonsterPresentationSurface.FindVisibleBody(val10) : null);
				if (monsterSettings != null && val11 == null)
				{
					return false;
				}
				Node2D val12 = val11 ?? CombatTargetSurfaceAnchorRuntime.Resolve(val9, "CznSupportTarget", "Support anchor: ", null, null, CombatTargetSurfaceAnchorMode.VfxSpawnPosition);
				if (val12 == null)
				{
					return false;
				}
				Node2D orCreateHost = GetOrCreateHost(val12, (Node2D)(val11 ?? ((object)val10) ?? ((object)val9.Visuals)), val11 != null, val8);
				if (orCreateHost == null)
				{
					return false;
				}
				val7 = ((GodotObject)orCreateHost).Call(StringName.op_Implicit("is_runtime_ready"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(route) });
				if (!((Variant)(ref val7)).AsBool())
				{
					return false;
				}
				StringName obj2 = StringName.op_Implicit("support_body");
				val7 = default(Variant);
				val7 = ((GodotObject)orCreateHost).GetMeta(obj2, val7);
				GodotObject obj3 = ((Variant)(ref val7)).AsGodotObject();
				Node2D val13 = (Node2D)(object)((obj3 is Node2D) ? obj3 : null);
				if (val13 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val13))
				{
					return false;
				}
				if (val11 == null)
				{
					((CanvasItem)val12).ZAsRelative = false;
					((CanvasItem)val12).ZIndex = CombatPresentationEffectCueProjection.ResolveOrdinaryTargetDepth(val13) ?? ((CanvasItem)val13).ZIndex;
				}
				((GodotObject)orCreateHost).Call(StringName.op_Implicit("apply_presentation_settings"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(val8) });
				bool num;
				if (!audio)
				{
					StringName obj4 = StringName.op_Implicit("play");
					Variant[] obj5 = new Variant[4]
					{
						Variant.op_Implicit(route),
						Variant.op_Implicit((GodotObject)(object)val12),
						Variant.op_Implicit(val12.GlobalPosition),
						default(Variant)
					};
					Vector2 val16;
					if (val11 == null)
					{
						Vector2 val14 = val13.GlobalScale;
						Vector2 val15 = ((Vector2)(ref val14)).Abs();
						val14 = val12.GlobalScale;
						val14 = ((Vector2)(ref val14)).Abs();
						val16 = val15 / ((Vector2)(ref val14)).Max(new Vector2(0.0001f, 0.0001f));
					}
					else
					{
						val16 = Vector2.One;
					}
					obj5[3] = Variant.op_Implicit(val16);
					val7 = ((GodotObject)orCreateHost).Call(obj4, (Variant[])(object)obj5);
					num = ((Variant)(ref val7)).AsBool();
				}
				else
				{
					val7 = ((GodotObject)orCreateHost).Call(StringName.op_Implicit("sound"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(route) });
					num = ((Variant)(ref val7)).AsBool();
				}
				bool flag = num;
				if (audio)
				{
					_soundConsumed = flag;
				}
				else
				{
					_visualConsumed = flag;
				}
				if (flag)
				{
					SkinLog.InfoOnce($"support:{settings?.CharacterId ?? "enemy"}:{route}:{audio}", $"Shared support dispatched character={settings?.CharacterId ?? "enemy"} route={route} channel={(audio ? "sfx" : "vfx")} depth={((CanvasItem)val12).ZIndex}");
				}
				return flag;
			}
			catch (Exception ex)
			{
				SkinLog.Warn("Shared support feedback retained native fallback: " + ex.Message);
				return false;
			}
		}
	}

	[HarmonyPatch(typeof(CreatureCmd), "Heal")]
	private static class HealScope
	{
		private static void Prefix(Creature creature, decimal amount, bool playAnim, out Scope? __state)
		{
			__state = Current.Value;
			Current.Value = ((playAnim && amount > 0m && creature.CurrentHp < creature.MaxHp) ? Begin(creature, "system_eff_heal") : null);
		}

		private static void Postfix(Scope? __state)
		{
			Current.Value = __state;
		}

		private static void Finalizer(Scope? __state)
		{
			Current.Value = __state;
		}
	}

	[HarmonyPatch(typeof(NCreature), "OnPowerIncreased")]
	private static class PowerScope
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			List<CodeInstruction> list = instructions.Select((Func<CodeInstruction, CodeInstruction>)((CodeInstruction i) => new CodeInstruction(i))).ToList();
			MethodInfo methodInfo = AccessTools.Method(typeof(SfxCmd), "Play", new Type[2]
			{
				typeof(string),
				typeof(float)
			}, (Type[])null);
			int num = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (CodeInstructionExtensions.Calls(list[j], methodInfo))
				{
					CodeInstruction val = list[j];
					CodeInstruction val2 = new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					val2.labels.AddRange(val.labels);
					val.labels.Clear();
					val2.blocks.AddRange(val.blocks);
					val.blocks.Clear();
					list.InsertRange(j, new _003C_003Ez__ReadOnlyArray<CodeInstruction>((CodeInstruction[])(object)new CodeInstruction[2]
					{
						val2,
						new CodeInstruction(OpCodes.Ldarg_2, (object)null)
					}));
					val.operand = AccessTools.Method(typeof(SharedSupportPresentation), "PlayPowerSound", (Type[])null, (Type[])null);
					j += 2;
					num++;
				}
			}
			if (num != 1)
			{
				throw new InvalidOperationException($"Expected one native power sound call, found {num}.");
			}
			return list;
		}

		private static void Prefix(PowerModel power, int amount, bool silent, out Scope? __state)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			__state = Current.Value;
			string text = ResolvePowerRoute(power.GetTypeForAmount((decimal)power.Amount), power.Owner.IsMonster, amount);
			Current.Value = ((!silent && power.ShouldPlayVfx && text != null) ? Begin(power.Owner, text) : null);
		}

		private static void Postfix(Scope? __state)
		{
			Current.Value = __state;
		}

		private static void Finalizer(Scope? __state)
		{
			Current.Value = __state;
		}
	}

	[HarmonyPatch(typeof(VfxCmd), "PlayOnCreatureCenter")]
	private static class HealVisual
	{
		private static bool Prefix(Creature target, string path)
		{
			if (!(path != "vfx/vfx_cross_heal"))
			{
				Scope? value = Current.Value;
				if (value != null && value.Matches("system_eff_heal", target))
				{
					return !Current.Value.TryPlay(audio: false);
				}
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(NPowerAppliedBuffVfx), "Create")]
	private static class BuffVisual
	{
		private static bool Prefix(ref NPowerAppliedBuffVfx? __result)
		{
			Scope? value = Current.Value;
			if (value == null || !value.Matches("system_eff_buff") || !Current.Value.TryPlay(audio: false))
			{
				return true;
			}
			__result = null;
			return false;
		}
	}

	[HarmonyPatch(typeof(NPowerAppliedDebuffVfx), "Create")]
	private static class DebuffVisual
	{
		private static bool Prefix(ref NPowerAppliedDebuffVfx? __result)
		{
			Scope? value = Current.Value;
			if (value == null || !value.Matches("system_eff_debuff") || !Current.Value.TryPlay(audio: false))
			{
				return true;
			}
			__result = null;
			return false;
		}
	}

	[HarmonyPatch(typeof(SfxCmd), "Play", new Type[]
	{
		typeof(string),
		typeof(float)
	})]
	private static class Sound
	{
		private static bool Prefix(string sfx)
		{
			if (!(sfx != "event:/sfx/heal"))
			{
				Scope? value = Current.Value;
				if (value != null && value.Matches("system_eff_heal"))
				{
					return !Current.Value.TryPlay(audio: true);
				}
			}
			return true;
		}
	}

	private static readonly Dictionary<string, Owner> Owners = new Dictionary<string, Owner>(StringComparer.Ordinal);

	private static readonly AsyncLocal<Scope?> Current = new AsyncLocal<Scope>();

	private const string HostName = "CznSharedSupport";

	private const string ScriptPath = "res://CznCore/scripts/common_presentation.gd";

	private const string BuffRoute = "system_eff_buff";

	private const string HealRoute = "system_eff_heal";

	private const string DebuffRoute = "system_eff_debuff";

	private static readonly Type[] Patches = new Type[6]
	{
		typeof(HealScope),
		typeof(PowerScope),
		typeof(HealVisual),
		typeof(BuffVisual),
		typeof(DebuffVisual),
		typeof(Sound)
	};

	public static void Register(string owner, Func<Creature, bool> accepts, Func<ICharacterSkinAudioSettings> settings)
	{
		PresentationTransports.EnsureTypes(Patches);
		SharedPlayerDefensePresentation.Ensure();
		Owners[owner] = new Owner(accepts, settings);
		CharacterSkinModBootstrap.TrackInitializationRollback(delegate
		{
			Owners.Remove(owner);
		});
	}

	internal static ICharacterSkinAudioSettings? ResolvePlayerSettings(Creature target)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		if (target.IsMonster || target.Player == null || (int)target.Side != 1)
		{
			return null;
		}
		foreach (Owner value in Owners.Values)
		{
			if (value.Settings != null && value.Accepts(target))
			{
				return value.Settings();
			}
		}
		return null;
	}

	public static void RegisterMonsters(string owner, Func<Creature, bool> accepts, Func<Dictionary> settings)
	{
		PresentationTransports.EnsureTypes(Patches);
		Owners[owner] = new Owner((Creature c) => c.IsMonster && accepts(c), null, settings);
		CharacterSkinModBootstrap.TrackInitializationRollback(delegate
		{
			Owners.Remove(owner);
		});
	}

	private static Scope? Begin(Creature target, string route)
	{
		if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || target.IsDead)
		{
			return null;
		}
		foreach (Owner value in Owners.Values)
		{
			if (value.Accepts(target))
			{
				return new Scope(target, route, value.Settings?.Invoke(), value.MonsterSettings);
			}
		}
		return null;
	}

	internal static Node2D? GetOrCreateHost(Node2D anchor, Node2D body, bool modelSurface, Dictionary presentation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = ((Node)anchor).GetNodeOrNull<Node2D>(NodePath.op_Implicit("CznSharedSupport"));
		if (val == null)
		{
			if (!ResourceLoader.Exists("res://CznCore/scripts/common_presentation.gd", "") || !ResourceLoader.Exists("res://CznCore/presentation/support.json", ""))
			{
				return null;
			}
			val = new Node2D
			{
				Name = StringName.op_Implicit("CznSharedSupport")
			};
			((GodotObject)val).SetScript(Variant.op_Implicit((GodotObject)(object)ResourceLoader.Load<Script>("res://CznCore/scripts/common_presentation.gd", (string)null, (CacheMode)1)));
			((GodotObject)val).Set(StringName.op_Implicit("catalog_path"), Variant.op_Implicit("res://CznCore/presentation/support.json"));
			((GodotObject)val).Set(StringName.op_Implicit("max_concurrent"), Variant.op_Implicit(8));
			if (presentation.ContainsKey(Variant.op_Implicit("settings_group")))
			{
				((GodotObject)val).Set(StringName.op_Implicit("settings_group"), presentation[Variant.op_Implicit("settings_group")]);
			}
			((GodotObject)val).SetMeta(StringName.op_Implicit("support_body"), Variant.op_Implicit((GodotObject)(object)body));
			if (modelSurface)
			{
				Dictionary val2 = new Dictionary();
				string[] array = new string[2] { "root", "target" };
				foreach (string text in array)
				{
					Node2D val3 = MonsterPresentationSurface.ResolveBone(body, text);
					if (val3 == null)
					{
						((GodotObject)val).Free();
						return null;
					}
					val2[Variant.op_Implicit(text)] = Variant.op_Implicit((GodotObject)(object)val3);
				}
				((GodotObject)val).SetMeta(StringName.op_Implicit("support_model_bones"), Variant.op_Implicit(val2));
			}
			((Node)anchor).AddChild((Node)(object)val, false, (InternalMode)0);
		}
		return val;
	}

	internal static string? ResolvePowerRoute(PowerType type, bool monster, int amount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		if ((int)type != 1 || amount <= 0)
		{
			if (!((int)type == 2 && monster) || amount == 0)
			{
				return null;
			}
			return "system_eff_debuff";
		}
		return "system_eff_buff";
	}

	private static void PlayPowerSound(string sfx, float volume, PowerModel power, int amount)
	{
		if (sfx == "event:/sfx/buff" && amount > 0)
		{
			Scope? scope = Begin(power.Owner, "system_eff_buff");
			if (scope != null && scope.TryPlay(audio: true))
			{
				return;
			}
		}
		SfxCmd.Play(sfx, volume);
	}
}
