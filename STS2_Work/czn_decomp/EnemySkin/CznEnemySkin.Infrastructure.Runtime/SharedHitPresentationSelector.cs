using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class SharedHitPresentationSelector
{
	internal static bool Matches(MonsterModel monster, string trigger, EnemyAction expected)
	{
		if ((monster is LivingShield || monster is SoulNexus || monster is OwlMagistrate || monster is Crusher || monster is Rocket || monster is Queen || monster is Vantom) ? true : false)
		{
			if (Entry.MoveSemantics.TryResolve(monster, trigger, out var action))
			{
				return action == expected;
			}
			return false;
		}
		LagavulinMatriarch val = (LagavulinMatriarch)(object)((monster is LagavulinMatriarch) ? monster : null);
		if (val != null)
		{
			if (LagavulinMatriarchAnimatorAdapter.TryMapPresentation(val, trigger, out var action2))
			{
				return action2 == expected;
			}
			return false;
		}
		TestSubject val2 = (TestSubject)(object)((monster is TestSubject) ? monster : null);
		if (val2 != null)
		{
			if (TestSubjectAnimatorAdapter.TryMapPresentation(val2, trigger, out var action3))
			{
				return action3 == expected;
			}
			return false;
		}
		SpinyToad val3 = (SpinyToad)(object)((monster is SpinyToad) ? monster : null);
		if (val3 != null)
		{
			if (SpinyToadAnimatorAdapter.TryMapPresentation(val3, trigger, out var action4))
			{
				return action4 == expected;
			}
			if (trigger == "Attack")
			{
				return expected == EnemyAction.BasicAttack;
			}
			return false;
		}
		BowlbugNectar val4 = (BowlbugNectar)(object)((monster is BowlbugNectar) ? monster : null);
		if (val4 != null && BowlbugNectarAnimatorAdapter.TryMapPresentation(val4, trigger, out var action5))
		{
			return action5 == expected;
		}
		return false;
	}
}
