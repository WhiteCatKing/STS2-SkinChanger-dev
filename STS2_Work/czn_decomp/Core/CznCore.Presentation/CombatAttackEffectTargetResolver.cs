using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CznCore.Presentation;

internal sealed class CombatAttackEffectTargetResolver
{
	[CompilerGenerated]
	private sealed class _003CGetRunStateCreatures_003Ed__11 : IEnumerable<Creature>, IEnumerable, IEnumerator<Creature>, IEnumerator, IDisposable
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
		public _003CGetRunStateCreatures_003Ed__11(int _003C_003E1__state)
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
				if (target != null)
				{
					_003C_003E2__current = target;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0046;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0046;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0046:
				if (dealer != null)
				{
					_003C_003E2__current = dealer;
					_003C_003E1__state = 2;
					return true;
				}
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
			_003CGetRunStateCreatures_003Ed__11 _003CGetRunStateCreatures_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetRunStateCreatures_003Ed__ = this;
			}
			else
			{
				_003CGetRunStateCreatures_003Ed__ = new _003CGetRunStateCreatures_003Ed__11(0);
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

	private readonly FieldInfo? _attackerAnimDelayField = typeof(AttackCommand).GetField("_attackerAnimDelay", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly FieldInfo? _damagePerHitField = typeof(AttackCommand).GetField("_damagePerHit", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly FieldInfo? _calculatedDamageVarField = typeof(AttackCommand).GetField("_calculatedDamageVar", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly FieldInfo? _singleTargetField = typeof(AttackCommand).GetField("_singleTarget", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly MethodInfo? _getPossibleTargetsMethod = typeof(AttackCommand).GetMethod("GetPossibleTargets", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private readonly CombatOpponentTargetResolver _opponentTargets = new CombatOpponentTargetResolver();

	public float GetAttackAnimDelaySeconds(AttackCommand? attackCommand, Player? player, Action<string>? warn = null)
	{
		try
		{
			if (attackCommand != null && _attackerAnimDelayField?.GetValue(attackCommand) is float num && num > 0f)
			{
				return num;
			}
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat attack anim delay reflection failed: " + SkinLog.FormatException(ex));
		}
		float? obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			CharacterModel character = player.Character;
			obj = ((character != null) ? new float?(character.AttackAnimDelay) : ((float?)null));
		}
		float? num2 = obj;
		return num2.GetValueOrDefault();
	}

	public decimal GetDamagePerHit(AttackCommand? attackCommand, Creature? target = null, Action<string>? warn = null)
	{
		return GetDamagePerHitDetails(attackCommand, target, warn).Amount;
	}

	public CombatAttackDamagePerHit GetDamagePerHitDetails(AttackCommand? attackCommand, Creature? target = null, Action<string>? warn = null)
	{
		try
		{
			if (attackCommand == null)
			{
				return new CombatAttackDamagePerHit(0m, Array.Empty<AbstractModel>());
			}
			AbstractModel modelSource = attackCommand.ModelSource;
			CardModel cardSource = (CardModel)(object)((modelSource is CardModel) ? modelSource : null);
			Creature val = ResolveDamageTarget(attackCommand, target, cardSource);
			decimal? num = null;
			object? obj = _calculatedDamageVarField?.GetValue(attackCommand);
			CalculatedDamageVar val2 = (CalculatedDamageVar)((obj is CalculatedDamageVar) ? obj : null);
			if (val2 != null)
			{
				num = ((CalculatedVar)val2).Calculate(val);
			}
			else if (_damagePerHitField?.GetValue(attackCommand) is decimal value)
			{
				num = value;
			}
			if (num.HasValue)
			{
				return ModifyDamagePerHit(attackCommand, val, cardSource, num.Value);
			}
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat attack damage-per-hit reflection failed: " + SkinLog.FormatException(ex));
		}
		return new CombatAttackDamagePerHit(0m, Array.Empty<AbstractModel>());
	}

	private Creature? ResolveDamageTarget(AttackCommand attackCommand, Creature? target, CardModel? cardSource)
	{
		object obj = target;
		if (obj == null)
		{
			object? obj2 = _singleTargetField?.GetValue(attackCommand);
			obj = ((obj2 is Creature) ? obj2 : null);
			if (obj == null)
			{
				if (cardSource == null)
				{
					return null;
				}
				obj = cardSource.CurrentTarget;
			}
		}
		return (Creature?)obj;
	}

	private static CombatAttackDamagePerHit ModifyDamagePerHit(AttackCommand attackCommand, Creature? target, CardModel? cardSource, decimal damagePerHit)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Creature attacker = attackCommand.Attacker;
		if (target == null && attacker == null)
		{
			return new CombatAttackDamagePerHit(Math.Max(0m, damagePerHit), Array.Empty<AbstractModel>());
		}
		IEnumerable<AbstractModel> modifiers;
		decimal val = HookCompat.ModifyDamage(IRunState.GetFrom(GetRunStateCreatures(target, attacker)), ((target != null) ? target.CombatState : null) ?? ((attacker != null) ? attacker.CombatState : null) ?? ((cardSource != null) ? cardSource.CombatState : null), target, attacker, damagePerHit, attackCommand.DamageProps, cardSource, (ModifyDamageHookType)14, (CardPreviewMode)0, out modifiers);
		return new CombatAttackDamagePerHit(Math.Max(0m, val), (modifiers as IReadOnlyList<AbstractModel>) ?? modifiers.ToArray());
	}

	[IteratorStateMachine(typeof(_003CGetRunStateCreatures_003Ed__11))]
	private static IEnumerable<Creature> GetRunStateCreatures(Creature? target, Creature? dealer)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetRunStateCreatures_003Ed__11(-2)
		{
			_003C_003E3__target = target,
			_003C_003E3__dealer = dealer
		};
	}

	public IReadOnlyList<Creature>? ResolveTargets(CardModel? card, AttackCommand? attackCommand, object? combatState, Action<string>? warn = null)
	{
		try
		{
			if (attackCommand != null && combatState != null && attackCommand.Attacker != null && !attackCommand.IsRandomlyTargeted)
			{
				if (attackCommand.IsMultiTargeted)
				{
					return _opponentTargets.ResolveAliveOpponents(combatState, attackCommand.Attacker, warn);
				}
				if (attackCommand.IsSingleTargeted)
				{
					object? obj = _singleTargetField?.GetValue(attackCommand);
					Creature val = NormalizeAttackTarget((Creature?)((obj is Creature) ? obj : null)) ?? NormalizeAttackTarget((card != null) ? card.CurrentTarget : null);
					if (val != null)
					{
						return new _003C_003Ez__ReadOnlySingleElementList<Creature>(val);
					}
					if (_getPossibleTargetsMethod?.Invoke(attackCommand, null) is IReadOnlyList<Creature> { Count: 1 } readOnlyList && readOnlyList[0].IsAlive)
					{
						return new _003C_003Ez__ReadOnlySingleElementList<Creature>(readOnlyList[0]);
					}
				}
			}
			Creature val2 = NormalizeAttackTarget((card != null) ? card.CurrentTarget : null);
			if (val2 != null)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<Creature>(val2);
			}
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat attack effect target resolution failed: " + SkinLog.FormatException(ex));
		}
		return null;
	}

	private static Creature? NormalizeAttackTarget(Creature? target)
	{
		if (target == null || !target.IsAlive)
		{
			return null;
		}
		return target;
	}
}
