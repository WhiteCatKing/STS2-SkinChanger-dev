using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace CznCore.Presentation;

internal static class LeftSideCardAnimationResolver
{
	public static CombatPresentationCardRoute Resolve(CardType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)type != 1)
		{
			if (type - 2 <= 1)
			{
				return CombatPresentationCardRoute.Skill;
			}
			return CombatPresentationCardRoute.Skill;
		}
		return CombatPresentationCardRoute.Attack;
	}

	public static bool TryResolve(IReadOnlyDictionary<CombatPresentationCardRoute, string> animations, CombatPresentationCardRoute route, out string animation)
	{
		if (animations.TryGetValue(route, out animation))
		{
			return true;
		}
		if ((uint)(route - 3) <= 3u)
		{
			return animations.TryGetValue(CombatPresentationCardRoute.Skill, out animation);
		}
		return false;
	}
}
