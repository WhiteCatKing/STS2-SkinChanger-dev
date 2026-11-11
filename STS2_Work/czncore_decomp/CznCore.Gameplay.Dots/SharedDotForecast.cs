using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznCore.Gameplay.Dots;

public static class SharedDotForecast
{
	internal readonly record struct Amounts(int Poison, int Rift, int Pain)
	{
		public int Total => (int)Math.Min(2147483647L, (long)Poison + (long)Rift + Pain);
	}

	public static bool HasDamageOverTime(Creature creature)
	{
		if (!creature.HasPower<PoisonPower>())
		{
			return creature.Powers.Any((PowerModel p) => p is ISharedDotForecast && p.Amount > 0);
		}
		return true;
	}

	public static int Calculate(Creature creature)
	{
		return CalculateAmounts(creature).Total;
	}

	internal static Amounts CalculateAmounts(Creature creature)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		PoisonPower power = creature.GetPower<PoisonPower>();
		int poison = ((power != null) ? power.CalculateTotalDamageNextTurn() : 0);
		if (!creature.IsAlive || !creature.IsEnemy || creature.CombatState == null)
		{
			return new Amounts(poison, 0, 0);
		}
		AbstractModel val = default(AbstractModel);
		decimal val2 = ((!Hook.ShouldClearBlock(creature.CombatState, creature, ref val)) ? creature.Block : 0);
		int num = 0;
		int num2 = 0;
		IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
		foreach (PowerModel power2 in creature.Powers)
		{
			if (!(power2 is ISharedDotForecast sharedDotForecast) || power2.Amount <= 0)
			{
				continue;
			}
			bool flag = sharedDotForecast.ForecastKind == SharedDotKind.Pain;
			ValueProp val3 = (ValueProp)(flag ? 4 : 6);
			int num3 = ((!flag) ? 1 : power2.Amount);
			for (int i = 0; i < num3; i++)
			{
				decimal val4 = Hook.ModifyDamage(creature.CombatState.RunState, creature.CombatState, creature, (Creature)null, flag ? 1m : (1m * (decimal)power2.Amount), val3, (CardModel)null, (CardPlay)null, (ModifyDamageHookType)14, (CardPreviewMode)0, ref enumerable);
				val4 = Math.Max(0m, val4);
				if (flag)
				{
					decimal num4 = Math.Min(val2, val4);
					val2 -= num4;
					val4 -= num4;
				}
				int num5 = (int)Math.Min(2147483647m, val4);
				if (flag)
				{
					num2 = (int)Math.Min(2147483647L, (long)num2 + (long)num5);
				}
				else
				{
					num = (int)Math.Min(2147483647L, (long)num + (long)num5);
				}
			}
		}
		return new Amounts(poison, num, num2);
	}
}
