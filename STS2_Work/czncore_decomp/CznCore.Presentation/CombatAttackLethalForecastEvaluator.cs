using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznCore.Presentation;

internal static class CombatAttackLethalForecastEvaluator
{
	[CompilerGenerated]
	private sealed class _003CGetRunStateCreatures_003Ed__4 : IEnumerable<Creature>, IEnumerable, IEnumerator<Creature>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Creature _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Creature target;

		public Creature _003C_003E3__target;

		private Creature dealer;

		public Creature _003C_003E3__dealer;

		Creature IEnumerator<Creature>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetRunStateCreatures_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = target;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				if (target != dealer)
				{
					_003C_003E2__current = dealer;
					_003C_003E1__state = 2;
					return true;
				}
				break;
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Creature> IEnumerable<Creature>.GetEnumerator()
		{
			_003CGetRunStateCreatures_003Ed__4 _003CGetRunStateCreatures_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetRunStateCreatures_003Ed__ = this;
			}
			else
			{
				_003CGetRunStateCreatures_003Ed__ = new _003CGetRunStateCreatures_003Ed__4(0);
			}
			_003CGetRunStateCreatures_003Ed__.target = _003C_003E3__target;
			_003CGetRunStateCreatures_003Ed__.dealer = _003C_003E3__dealer;
			return _003CGetRunStateCreatures_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Creature>)this).GetEnumerator();
		}
	}

	private const int MaxForecastHitCount = 100;

	private static readonly ConcurrentDictionary<(Type ModelType, string MethodName), bool> HookOverrideCache = new ConcurrentDictionary<(Type, string), bool>();

	public static CombatAttackLethalForecastResult Evaluate(CardModel? card, AttackCommand? attackCommand, object? combatState, int hitCount, CombatAttackEffectTargetResolver targetResolver, Action<string>? warn = null)
	{
		try
		{
			if (attackCommand == null || attackCommand.Attacker == null)
			{
				return Unknown("missing_attack_command");
			}
			if (attackCommand.IsRandomlyTargeted)
			{
				return Unknown("random_target");
			}
			if (hitCount <= 0 || hitCount > 100)
			{
				return Unknown("unsupported_hit_count");
			}
			IReadOnlyList<Creature> readOnlyList = targetResolver.ResolveTargets(card, attackCommand, combatState, warn);
			if (readOnlyList == null || readOnlyList.Count == 0)
			{
				return Unknown("target_unresolved");
			}
			bool flag = false;
			foreach (Creature item in readOnlyList)
			{
				CombatAttackLethalForecastResult result = EvaluateTarget(card, attackCommand, item, hitCount, targetResolver, warn);
				if (result.Forecast == CombatAttackLethalForecast.Lethal)
				{
					return result;
				}
				flag |= result.Forecast == CombatAttackLethalForecast.Unknown;
			}
			return flag ? Unknown("one_or_more_targets_unknown") : new CombatAttackLethalForecastResult(CombatAttackLethalForecast.NotLethal, "direct_damage_below_current_hp");
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat attack lethal forecast failed: " + SkinLog.FormatException(ex));
			return Unknown("forecast_exception");
		}
	}

	private static CombatAttackLethalForecastResult EvaluateTarget(CardModel? card, AttackCommand attackCommand, Creature target, int hitCount, CombatAttackEffectTargetResolver targetResolver, Action<string>? warn)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		if (!target.IsAlive || target.CurrentHp <= 0)
		{
			return Unknown("target_not_alive");
		}
		Creature attacker = attackCommand.Attacker;
		CombatAttackDamagePerHit damagePerHitDetails = targetResolver.GetDamagePerHitDetails(attackCommand, target, warn);
		decimal amount = damagePerHitDetails.Amount;
		if (amount <= 0m)
		{
			return new CombatAttackLethalForecastResult(CombatAttackLethalForecast.NotLethal, "no_direct_damage");
		}
		Player petOwner = target.PetOwner;
		decimal val = (((petOwner != null) ? petOwner.Creature : null) ?? target).Block;
		int num = target.CurrentHp;
		ICombatState val2 = target.CombatState ?? attacker.CombatState;
		IRunState from = IRunState.GetFrom(GetRunStateCreatures(target, attacker));
		ValueProp damageProps = attackCommand.DamageProps;
		if (HasHookOverride(target, "BeforeDamageReceived"))
		{
			return Unknown("before_damage_reaction");
		}
		if (hitCount > 1 && (damagePerHitDetails.Modifiers.Any((AbstractModel modifier) => OverridesHook(modifier, "AfterModifyingDamageAmount")) || HasInterHitReaction(target)))
		{
			return Unknown("stateful_multi_hit_reaction");
		}
		IEnumerable<AbstractModel> source = default(IEnumerable<AbstractModel>);
		IEnumerable<AbstractModel> source2 = default(IEnumerable<AbstractModel>);
		AbstractModel val4 = default(AbstractModel);
		for (int i = 0; i < hitCount; i++)
		{
			decimal num2 = (((Enum)damageProps).HasFlag((Enum)(object)(ValueProp)2) ? 0m : Math.Min(val, amount));
			val -= (decimal)(int)num2;
			decimal num3 = Math.Max(amount - num2, 0m);
			if (num3 <= 0m)
			{
				continue;
			}
			decimal num4 = Hook.ModifyHpLost(from, val2, target, num3, damageProps, attacker, card, (HpLossHookPhase)1, ref source);
			if (num4 != num3 || source.Any())
			{
				return Unknown("hp_loss_modifier");
			}
			Creature val3 = (Creature)((val2 == null) ? ((object)target) : ((object)Hook.ModifyUnblockedDamageTarget(val2, target, num4, damageProps, attacker)));
			if (val3 != target)
			{
				return Unknown("damage_redirected");
			}
			decimal num5 = Hook.ModifyHpLost(from, val2, val3, num4, damageProps, attacker, card, (HpLossHookPhase)2, ref source2);
			if (num5 != num4 || source2.Any())
			{
				return Unknown("hp_loss_modifier");
			}
			if (num > 0 && num5 >= (decimal)num)
			{
				if (!Hook.ShouldDie(from, val2, target, ref val4))
				{
					return Unknown("death_prevented");
				}
				if (val2 != null && !Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(val2, target))
				{
					return Unknown("death_not_final");
				}
				return new CombatAttackLethalForecastResult(CombatAttackLethalForecast.Lethal, "direct_damage_reaches_current_hp");
			}
			int num6 = (int)Math.Clamp(num5, 0m, 999999999m);
			num = Math.Max(num - num6, 0);
		}
		return new CombatAttackLethalForecastResult(CombatAttackLethalForecast.NotLethal, "direct_damage_below_current_hp");
	}

	[IteratorStateMachine(typeof(_003CGetRunStateCreatures_003Ed__4))]
	private static IEnumerable<Creature> GetRunStateCreatures(Creature target, Creature dealer)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetRunStateCreatures_003Ed__4(-2)
		{
			_003C_003E3__target = target,
			_003C_003E3__dealer = dealer
		};
	}

	private static bool HasInterHitReaction(Creature target)
	{
		if (!HasHookOverride(target, "AfterDamageReceived") && !HasHookOverride(target, "AfterBlockBroken"))
		{
			return HasHookOverride(target, "AfterCurrentHpChanged");
		}
		return true;
	}

	private static bool HasHookOverride(Creature target, string methodName)
	{
		if (target.Monster != null && OverridesHook((AbstractModel)(object)target.Monster, methodName))
		{
			return true;
		}
		return target.Powers.Any((PowerModel power) => OverridesHook((AbstractModel)(object)power, methodName));
	}

	private static bool OverridesHook(AbstractModel model, string methodName)
	{
		return HookOverrideCache.GetOrAdd((((object)model).GetType(), methodName), delegate((Type ModelType, string MethodName) key)
		{
			MethodInfo method = key.ModelType.GetMethod(key.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return method != null && method.DeclaringType != typeof(AbstractModel);
		});
	}

	private static CombatAttackLethalForecastResult Unknown(string reason)
	{
		return new CombatAttackLethalForecastResult(CombatAttackLethalForecast.Unknown, reason);
	}
}
