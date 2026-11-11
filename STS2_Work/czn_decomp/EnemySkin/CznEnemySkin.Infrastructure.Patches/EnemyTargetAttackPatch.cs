using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using CznEnemySkin.Profiles;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class EnemyTargetAttackPatch
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Action<Creature, string> _003C0_003E__PlayOnCreatureCenter;

		public static Action<Creature, string> _003C1_003E__PlayOnCreature;
	}

	internal static EnemyAction? ResolveAction(MonsterModel? monster, string? trigger)
	{
		GremlinMerc val = (GremlinMerc)(object)((monster is GremlinMerc) ? monster : null);
		if (val != null && trigger == "AttackDouble" && GremlinMercAnimatorAdapter.IsGimmeMove(val))
		{
			return EnemyAction.Buff;
		}
		if (monster is CeremonialBeast && trigger == "Attack" && Entry.MoveSemantics.TryResolve(monster, trigger, out var action))
		{
			return action;
		}
		SharedHitRoute sharedHitRoute = GeneratedSharedHitRoutes.FindForMonster(monster, trigger);
		if (sharedHitRoute != null)
		{
			return sharedHitRoute.Action;
		}
		bool flag = ((monster is BruteRubyRaider || monster is BowlbugSilk || monster is ScrollOfBiting || monster is AssassinRubyRaider || monster is AxeRubyRaider || monster is BowlbugEgg || monster is CalcifiedCultist || monster is BowlbugRock || monster is BygoneEffigy) ? true : false);
		if (flag && trigger == "Attack")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is VineShambler && trigger == "Chomp")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is VineShambler && trigger == "SwipePower")
		{
			return EnemyAction.HeavyAttack;
		}
		if (monster is TheInsatiable && trigger == "Thrash")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is TheInsatiable && trigger == "Bite")
		{
			return EnemyAction.HeavyAttack;
		}
		TestSubject val2 = (TestSubject)(object)((monster is TestSubject) ? monster : null);
		EnemyAction action2 = default(EnemyAction);
		flag = val2 != null && TestSubjectAnimatorAdapter.TryMapPresentation(val2, trigger ?? "", out action2);
		if (flag)
		{
			bool flag2 = (uint)(action2 - 4) <= 1u;
			flag = flag2;
		}
		if (flag)
		{
			return action2;
		}
		if (monster is Fabricator && string.Equals(trigger, "Attack", StringComparison.OrdinalIgnoreCase))
		{
			return EnemyAction.BasicAttack;
		}
		SlimedBerserker val3 = (SlimedBerserker)(object)((monster is SlimedBerserker) ? monster : null);
		EnemyAction action3;
		if (val3 != null && trigger == "Attack")
		{
			return (!SlimedBerserkerAnimatorAdapter.TryMapPresentation(val3, trigger, out action3)) ? EnemyAction.BasicAttack : action3;
		}
		if (monster is WaterfallGiant && trigger == "Erupt")
		{
			return EnemyAction.Special;
		}
		if (monster is Aeonglass && trigger == "AttackHeavy")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is Aeonglass && trigger == "AttackDouble")
		{
			return EnemyAction.HeavyAttack;
		}
		if (monster is Mawler && trigger == "Attack")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is MagiKnight && trigger == "RamAttack")
		{
			return EnemyAction.BasicAttack;
		}
		if (monster is InfestedPrism)
		{
			if (trigger == "AttackBlock")
			{
				return EnemyAction.HeavyAttack;
			}
			if (trigger == "AttackDouble")
			{
				return EnemyAction.Charge;
			}
		}
		if (monster is SoulNexus)
		{
			if (trigger == "Cast")
			{
				return EnemyAction.Cast;
			}
			if (trigger == "Attack" && Entry.MoveSemantics.TryResolve(monster, trigger, out var action4) && action4 == EnemyAction.BasicAttack)
			{
				return action4;
			}
		}
		return null;
	}

	private static void Prefix(AttackCommand __instance, string? ____attackerAnimName, float ____attackerAnimDelay, Creature? ____visualAttacker, List<Func<Creature, Node2D?>> ____customHitVfxNodes, bool ____spawnVfxOnCreatureCenter)
	{
		Creature attacker = __instance.Attacker;
		if (((attacker != null) ? attacker.Monster : null) is LagavulinMatriarch && ____attackerAnimName == "AttackDouble")
		{
			return;
		}
		Creature attacker2 = __instance.Attacker;
		SharedHitRoute sharedHitRoute = GeneratedSharedHitRoutes.FindForMonster((attacker2 != null) ? attacker2.Monster : null, ____attackerAnimName);
		int num;
		if (!(sharedHitRoute != null))
		{
			Creature attacker3 = __instance.Attacker;
			num = ((((attacker3 != null) ? attacker3.Monster : null) is CeremonialBeast) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		if (!flag)
		{
			Creature attacker4 = __instance.Attacker;
			MonsterModel val = ((attacker4 != null) ? attacker4.Monster : null);
			bool flag2 = ((val is BruteRubyRaider || val is BowlbugSilk || val is ScrollOfBiting || val is VineShambler || val is AssassinRubyRaider || val is AxeRubyRaider || val is BowlbugEgg || val is CalcifiedCultist || val is BowlbugRock || val is BygoneEffigy) ? true : false);
			flag = flag2;
		}
		bool num2 = flag;
		int num3;
		if (!(sharedHitRoute != null))
		{
			Creature attacker5 = __instance.Attacker;
			if (!(((attacker5 != null) ? attacker5.Monster : null) is CeremonialBeast))
			{
				num3 = 0;
				goto IL_0152;
			}
		}
		Creature attacker6 = __instance.Attacker;
		num3 = (NativeHitCallbackAdapter.IsReviewed((attacker6 != null) ? attacker6.Monster : null, ____attackerAnimName, ____customHitVfxNodes) ? 1 : 0);
		goto IL_0152;
		IL_0152:
		bool flag3 = (byte)num3 != 0;
		if (num2 && ((____customHitVfxNodes.Count != 0 && !flag3) || ____visualAttacker != null))
		{
			return;
		}
		Creature attacker7 = __instance.Attacker;
		EnemyAction? action = ResolveAction((attacker7 != null) ? attacker7.Monster : null, ____attackerAnimName);
		if (!action.HasValue)
		{
			return;
		}
		Creature attacker8 = __instance.Attacker;
		if (!EnemyVisualHostAdapter.TryResolveAppliedProfile((attacker8 != null) ? attacker8.GetCreatureNode() : null, out EnemyReplacementProfile profile) || (sharedHitRoute != null && sharedHitRoute.ProfileId != profile.Id) || !profile.Monster.ActionPresentations.TryGetValue(action.Value, out EnemyActionPresentation value) || !value.Effects.Any((EnemyEffectCue c) => c.TargetHit))
		{
			return;
		}
		if (__instance.Attacker.Monster is SlimedBerserker)
		{
			__instance.WithAttackerAnim(____attackerAnimName, Math.Max(____attackerAnimDelay, 1f), ____visualAttacker);
		}
		if (__instance.Attacker.Monster is WaterfallGiant && action == EnemyAction.Special)
		{
			__instance.WithAttackerAnim(____attackerAnimName, Math.Max(____attackerAnimDelay, 2.25f), ____visualAttacker);
		}
		if (!Entry.PresentationSettings.ActionEffectsEnabled)
		{
			return;
		}
		bool flag4 = __instance.Attacker.Monster is WaterfallGiant && action == EnemyAction.Special;
		bool center = flag4 || (__instance.Attacker.Monster is SoulNexus && action == EnemyAction.BasicAttack);
		bool centerPerHit = value.Effects.Any((EnemyEffectCue c) => c.TargetHit && c.TargetCenter);
		if ((center || centerPerHit) && (!__instance.IsMultiTargeted || __instance.IsRandomlyTargeted))
		{
			return;
		}
		string nativeVfx = __instance.HitVfx;
		string text;
		if (sharedHitRoute != null)
		{
			text = sharedHitRoute.NativeVfx;
		}
		else
		{
			string text2;
			if (__instance.Attacker.Monster is CeremonialBeast)
			{
				text2 = ((action == EnemyAction.ChargeEnd) ? null : "vfx/vfx_attack_slash");
			}
			else
			{
				MonsterModel val = __instance.Attacker.Monster;
				string text3;
				if ((val is BruteRubyRaider || val is BowlbugSilk || val is BowlbugEgg || val is BowlbugRock) ? true : false)
				{
					text3 = "vfx/vfx_attack_blunt";
				}
				else
				{
					string text4;
					if (__instance.Attacker.Monster is ScrollOfBiting)
					{
						text4 = "vfx/vfx_bite";
					}
					else
					{
						string text5;
						if (__instance.Attacker.Monster is VineShambler)
						{
							text5 = ((action == EnemyAction.BasicAttack) ? "vfx/vfx_bite" : "vfx/vfx_scratch");
						}
						else
						{
							string text6;
							if (__instance.Attacker.Monster is TheInsatiable)
							{
								text6 = ((action == EnemyAction.BasicAttack) ? "vfx/vfx_scratch" : "vfx/vfx_bite");
							}
							else
							{
								string text7;
								if (__instance.Attacker.Monster is TestSubject)
								{
									text7 = ((action == EnemyAction.Cast) ? null : "vfx/vfx_attack_blunt");
								}
								else
								{
									string text8;
									if (action == EnemyAction.Cast || flag4 || __instance.Attacker.Monster is SlimedBerserker)
									{
										text8 = null;
									}
									else
									{
										MonsterModel monster = __instance.Attacker.Monster;
										bool flag2 = ((monster is MagiKnight || monster is Aeonglass) ? true : false);
										text8 = (flag2 ? "vfx/vfx_attack_blunt" : "vfx/vfx_attack_slash");
									}
									text7 = text8;
								}
								text6 = text7;
							}
							text5 = text6;
						}
						text4 = text5;
					}
					text3 = text4;
				}
				text2 = text3;
			}
			text = text2;
		}
		string text9 = text;
		if ((sharedHitRoute != null) ? (!sharedHitRoute.AcceptsNativeVfx(nativeVfx)) : (nativeVfx != text9))
		{
			return;
		}
		int generation = Entry.ActionPresentation.EffectsGeneration;
		Action<Creature, string> nativeFallback = ResolveNativeFallback(____spawnVfxOnCreatureCenter);
		if (value.Effects.Any((EnemyEffectCue c) => c.TargetHit && c.TargetAtEntry))
		{
			if (!__instance.IsMultiTargeted || __instance.IsRandomlyTargeted)
			{
				return;
			}
			bool entryPlayed = false;
			__instance.WithAttackerFx((Func<Node2D>)delegate
			{
				if (entryPlayed)
				{
					return (Node2D?)null;
				}
				entryPlayed = true;
				if (!Entry.Replacements.IsEnabled(profile) || !Entry.PresentationSettings.ActionEffectsEnabled || generation != Entry.ActionPresentation.EffectsGeneration)
				{
					return (Node2D?)null;
				}
				foreach (Creature item in __instance.Attacker.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive))
				{
					TryPlay(item, profile, action.Value, generation, atEntry: true, null);
				}
				return (Node2D?)null;
			});
			if (__instance.Attacker.Monster is InfestedPrism && action == EnemyAction.Charge)
			{
				__instance.WithAttackerAnim(____attackerAnimName, Math.Max(____attackerAnimDelay, 0.7f), ____visualAttacker);
			}
		}
		if (!value.Effects.Any((EnemyEffectCue c) => c.TargetHit && !c.TargetAtEntry))
		{
			return;
		}
		bool centerAttempted = false;
		bool centerPlayed = false;
		if (center)
		{
			__instance.WithAttackerAnim(____attackerAnimName, Math.Max(____attackerAnimDelay, flag4 ? 2.25f : value.Effects.Where((EnemyEffectCue c) => c.TargetHit).Min((EnemyEffectCue c) => c.DelaySeconds)), ____visualAttacker);
			__instance.WithAttackerFx((Func<Node2D>)delegate
			{
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_011d: Unknown result type (might be due to invalid IL or missing references)
				if (!Entry.Replacements.IsEnabled(profile) || !Entry.PresentationSettings.ActionEffectsEnabled || generation != Entry.ActionPresentation.EffectsGeneration)
				{
					return (Node2D?)null;
				}
				if (centerAttempted)
				{
					return (Node2D?)null;
				}
				centerAttempted = true;
				Creature[] array = __instance.Attacker.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToArray();
				NCreature[] array2 = array.Select((Creature c) => c.GetCreatureNode()).ToArray();
				if (array2.Length == 0 || array2.Any((NCreature n) => n == null || !GodotObject.IsInstanceValid((GodotObject)(object)n)))
				{
					return (Node2D?)null;
				}
				Vector2 globalPosition = EnemyTargetPosition.ResolveSideFloor(array2.Select((NCreature n) => n));
				Creature val2 = array[0];
				Control vfxContainer = val2.GetVfxContainer();
				if (vfxContainer != null)
				{
					centerPlayed = Entry.ActionPresentation.TryPlayTargetEffects((Node)(object)vfxContainer, globalPosition, profile, action.Value, generation, (Node?)(object)val2.GetBackVfxContainer(), null, atEntry: false, centerOnly: false, null, screenSpace: false, null, EnemyPresentationStance.Normal, null);
				}
				return (Node2D?)null;
			});
		}
		Func<Creature, bool> playTarget = delegate(Creature target)
		{
			if (Entry.Replacements.IsEnabled(profile) && Entry.PresentationSettings.ActionEffectsEnabled && generation == Entry.ActionPresentation.EffectsGeneration)
			{
				EnemyReplacementProfile profile2 = profile;
				EnemyAction value2 = action.Value;
				int generation2 = generation;
				int? nativeHitIndex = __instance.Results.Count();
				MonsterModel monster2 = __instance.Attacker.Monster;
				CeremonialBeast val3 = (CeremonialBeast)(object)((monster2 is CeremonialBeast) ? monster2 : null);
				return TryPlay(target, profile2, value2, generation2, atEntry: false, nativeHitIndex, (val3 != null && val3.IsInSecondPhase) ? EnemyPresentationStance.Transformed : EnemyPresentationStance.Normal);
			}
			return false;
		};
		if (flag3)
		{
			playTarget = NativeHitCallbackAdapter.Wrap(____customHitVfxNodes, () => __instance.Results.Count(), playTarget);
		}
		int lastCenterHit = -1;
		__instance.WithHitVfxNode((Func<Creature, Node2D>)delegate(Creature target)
		{
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			bool flag5 = Entry.Replacements.IsEnabled(profile) && Entry.PresentationSettings.ActionEffectsEnabled && generation == Entry.ActionPresentation.EffectsGeneration;
			if (flag5 && centerPerHit && TryBeginCenterHit(__instance, ref lastCenterHit))
			{
				NCreature[] array3 = (from c in __instance.Attacker.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToArray()
					select c.GetCreatureNode()).ToArray();
				if (array3.Length != 0 && array3.All((NCreature n) => n != null && GodotObject.IsInstanceValid((GodotObject)(object)n) && !((GodotObject)n).IsQueuedForDeletion()))
				{
					Control vfxContainer2 = target.GetVfxContainer();
					Vector2 globalPosition2 = EnemyTargetPosition.ResolveSideFloor(array3.Select((NCreature n) => n));
					if (vfxContainer2 != null)
					{
						Entry.ActionPresentation.TryPlayTargetEffects((Node)(object)vfxContainer2, globalPosition2, profile, action.Value, generation, (Node?)(object)target.GetBackVfxContainer(), null, atEntry: false, centerOnly: true, (__instance.Attacker.Monster is TestSubject) ? array3.Select((NCreature n) => n).ToArray() : null, screenSpace: false, null, EnemyPresentationStance.Normal, null);
					}
				}
			}
			if (!(center ? (flag5 && centerPlayed) : playTarget(target)) && nativeVfx != null)
			{
				nativeFallback(target, nativeVfx);
			}
			return (Node2D?)null;
		});
		__instance.WithHitFx((string)null, __instance.HitSfx, __instance.TmpHitSfx);
	}

	internal static Action<Creature, string> ResolveNativeFallback(bool atCenter)
	{
		Action<Creature, string> action;
		if (!atCenter)
		{
			action = _003C_003EO._003C1_003E__PlayOnCreature;
			if (action == null)
			{
				return _003C_003EO._003C1_003E__PlayOnCreature = VfxCmd.PlayOnCreature;
			}
		}
		else
		{
			action = VfxCmd.PlayOnCreatureCenter;
		}
		return action;
	}

	internal static bool TryBeginCenterHit(AttackCommand command, ref int previousHit)
	{
		int num = command.Results.Count();
		if (num == previousHit)
		{
			return false;
		}
		previousHit = num;
		return true;
	}

	internal static bool TryPlay(Creature target, EnemyReplacementProfile profile, EnemyAction action, int generation, bool atEntry = false, int? nativeHitIndex = null, EnemyPresentationStance stance = EnemyPresentationStance.Normal)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		NCreature creatureNode = target.GetCreatureNode();
		Control vfxContainer = target.GetVfxContainer();
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || ((GodotObject)creatureNode).IsQueuedForDeletion() || vfxContainer == null)
		{
			return false;
		}
		bool flag = profile.Target.MonsterId == "MAGI_KNIGHT";
		bool flag2 = GeneratedSharedHitRoutes.Follows(profile.Id, action) || (profile.Target.MonsterId == "BYGONE_EFFIGY" && action == EnemyAction.BasicAttack) || (profile.Target.MonsterId == "AEONGLASS" && action == EnemyAction.HeavyAttack) || flag || (profile.Target.MonsterId == "SOUL_NEXUS" && action == EnemyAction.Cast);
		return Entry.ActionPresentation.TryPlayTargetEffects((Node)(object)vfxContainer, EnemyTargetPosition.ResolveFloor(creatureNode), profile, action, generation, (Node?)(object)target.GetBackVfxContainer(), (CanvasItem?)(object)(flag2 ? creatureNode : null), atEntry, centerOnly: false, null, screenSpace: false, nativeHitIndex, stance, EnemyTargetPosition.Resolve(creatureNode, EnemyTargetOrigin.Body));
	}
}
