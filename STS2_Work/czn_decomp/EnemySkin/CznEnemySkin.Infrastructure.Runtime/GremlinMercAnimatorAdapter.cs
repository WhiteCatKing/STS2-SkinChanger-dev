using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class GremlinMercAnimatorAdapter
{
	private const string GimmeMoveId = "GIMME_MOVE";

	public static CreatureAnimator Create(GremlinMerc merc, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.HeavyAttack);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		val2.NextState = val;
		val3.NextState = val;
		val4.NextState = val;
		val5.NextState = val;
		CreatureAnimator val7 = new CreatureAnimator(val, controller);
		val7.AddAnyState("Idle", val, (Func<bool>)null);
		val7.AddAnyState("Attack", val2, (Func<bool>)null);
		val7.AddAnyState("AttackDouble", val4, (Func<bool>)(() => IsGimmeMove(merc)));
		val7.AddAnyState("AttackDouble", val3, (Func<bool>)(() => !IsGimmeMove(merc)));
		val7.AddAnyState("Dead", val6, (Func<bool>)null);
		val7.AddAnyState("Hit", val5, (Func<bool>)null);
		return val7;
	}

	internal static bool IsGimmeMove(GremlinMerc merc)
	{
		if (((MonsterModel)merc).IsPerformingMove)
		{
			return ((MonsterModel)merc).NextMove.StateId == "GIMME_MOVE";
		}
		return false;
	}
}
