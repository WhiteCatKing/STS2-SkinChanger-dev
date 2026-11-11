using System;
using System.Collections.Generic;
using System.Reflection;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class CeremonialBeastAnimatorAdapter
{
	private const string StampMoveId = "STAMP_MOVE";

	private static readonly FieldInfo? IsStunnedField = AccessTools.Field(typeof(CeremonialBeast), "_isStunnedByPlowRemoval");

	private static readonly FieldInfo? InMidChargeField = AccessTools.Field(typeof(CeremonialBeast), "_inMidCharge");

	public static CreatureAnimator Create(CeremonialBeast beast, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected O, but got Unknown
		if (IsStunnedField == null || InMidChargeField == null)
		{
			throw new MissingFieldException("STS2 CeremonialBeast phase fields are unavailable.");
		}
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Cast);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.BasicAttack);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Charge);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.ChargeEnd);
		AnimState val10 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.ChargeRecovery);
		AnimState val11 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.ChargeDeath);
		AnimState val12 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.StunTransition);
		AnimState val13 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.StunLoop);
		AnimState val14 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.WakeUp);
		AnimState val15 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val16 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val17 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val18 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		val3.NextState = val;
		val4.NextState = val2;
		val5.NextState = val;
		val6.NextState = val2;
		val7.NextState = val;
		val15.NextState = val;
		val16.NextState = val2;
		val8.NextState = val9;
		val9.NextState = val10;
		val10.NextState = val;
		val12.NextState = val13;
		val14.NextState = val2;
		val.AddBranch("Plow", val8, (Func<bool>)null);
		AddHitBranches(new _003C_003Ez__ReadOnlyArray<AnimState>((AnimState[])(object)new AnimState[12]
		{
			val, val2, val3, val4, val15, val16, val5, val7, val6, val12,
			val13, val14
		}), beast, val15, val16, val12);
		CreatureAnimator val19 = new CreatureAnimator(beast.IsInSecondPhase ? val2 : val, controller);
		val19.AddAnyState("Unstun", val14, (Func<bool>)null);
		val19.AddAnyState("Dead", val17, (Func<bool>)(() => !InMidCharge(beast) && !beast.IsInSecondPhase));
		val19.AddAnyState("Dead", val11, (Func<bool>)(() => InMidCharge(beast)));
		val19.AddAnyState("Dead", val18, (Func<bool>)(() => !InMidCharge(beast) && beast.IsInSecondPhase));
		val19.AddAnyState("Stun", val12, (Func<bool>)null);
		val19.AddAnyState("Plow", val8, (Func<bool>)null);
		val19.AddAnyState("Attack", val7, (Func<bool>)(() => IsStampMove(beast)));
		val19.AddAnyState("Attack", val6, (Func<bool>)(() => !IsStampMove(beast) && beast.IsInSecondPhase));
		val19.AddAnyState("Attack", val5, (Func<bool>)(() => !IsStampMove(beast) && !beast.IsInSecondPhase));
		val19.AddAnyState("Cast", val4, (Func<bool>)(() => beast.IsInSecondPhase));
		val19.AddAnyState("Cast", val3, (Func<bool>)(() => !beast.IsInSecondPhase));
		val19.AddAnyState("PlowHit", val15, (Func<bool>)null);
		return val19;
	}

	internal static bool IsStampMove(CeremonialBeast beast)
	{
		if (((MonsterModel)beast).IsPerformingMove)
		{
			return ((MonsterModel)beast).NextMove.StateId == "STAMP_MOVE";
		}
		return false;
	}

	private static void AddHitBranches(IEnumerable<AnimState> states, CeremonialBeast beast, AnimState normalHurt, AnimState transformedHurt, AnimState transition)
	{
		foreach (AnimState state in states)
		{
			state.AddBranch("Hit", normalHurt, (Func<bool>)(() => ShouldPlayRegularHurt(beast) && !beast.IsInSecondPhase));
			state.AddBranch("Hit", transformedHurt, (Func<bool>)(() => ShouldPlayRegularHurt(beast) && beast.IsInSecondPhase));
			state.AddBranch("Hit", transition, (Func<bool>)(() => IsStunnedByPlowRemoval(beast)));
		}
	}

	private static bool ShouldPlayRegularHurt(CeremonialBeast beast)
	{
		if (!IsStunnedByPlowRemoval(beast))
		{
			return !InMidCharge(beast);
		}
		return false;
	}

	private static bool IsStunnedByPlowRemoval(CeremonialBeast beast)
	{
		return ReadFlag(IsStunnedField, beast);
	}

	private static bool InMidCharge(CeremonialBeast beast)
	{
		return ReadFlag(InMidChargeField, beast);
	}

	private static bool ReadFlag(FieldInfo field, CeremonialBeast beast)
	{
		object value = field.GetValue(beast);
		if (value is bool)
		{
			return (bool)value;
		}
		return false;
	}
}
