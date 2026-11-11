using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class CombatCardRouteResolver : ICombatPresentationRouteResolver<CardModel>
{
	private readonly CombatCardPresentationProfile _profile;

	public CombatCardRouteResolver(CombatCardPresentationProfile profile)
	{
		_profile = profile;
	}

	public CombatPresentationCardRoute Resolve(CardModel? card)
	{
		return Resolve(card, null);
	}

	public CombatPresentationCardRoute Resolve(CardModel? card, IReadOnlyList<string>? cardIdentifiers)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		if (card == null)
		{
			return CombatPresentationCardRoute.Unknown;
		}
		IReadOnlyList<string> readOnlyList = cardIdentifiers ?? CombatCardIdentifiers.Collect(card);
		if (_profile.TryGetRouteOverride(readOnlyList, out var route))
		{
			return route;
		}
		if (ShouldUseGeneratedCardRoute(card))
		{
			return CombatPresentationCardRoute.GeneratedCard;
		}
		if (_profile.MatchesExplicitRoute(readOnlyList, CombatPresentationCardRoute.Skill))
		{
			return CombatPresentationCardRoute.Skill;
		}
		if (_profile.MatchesExplicitRoute(readOnlyList, CombatPresentationCardRoute.BuffU3))
		{
			return CombatPresentationCardRoute.BuffU3;
		}
		if (_profile.MatchesExplicitRoute(readOnlyList, CombatPresentationCardRoute.BuffU2))
		{
			return CombatPresentationCardRoute.BuffU2;
		}
		if ((int)card.Type == 1)
		{
			return CombatPresentationCardRoute.Attack;
		}
		if ((int)card.Type == 2)
		{
			return ResolveSkillRoute(card, readOnlyList);
		}
		if ((int)card.Type == 3)
		{
			if (!HasBuffU3DescriptionSignal(GetCardDescriptionText(card)))
			{
				return CombatPresentationCardRoute.BuffU2;
			}
			return CombatPresentationCardRoute.BuffU3;
		}
		return CombatPresentationCardRoute.Unknown;
	}

	private static bool ShouldUseBlockSkillRoute(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)card.Type == 2)
		{
			return card.GainsBlock;
		}
		return false;
	}

	private static bool ShouldUseGeneratedCardRoute(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		CardType type = card.Type;
		if (type - 2 <= 1)
		{
			return CombatGeneratedCardSignalDetector.HasGeneratedCardOnPlaySignal(card);
		}
		return false;
	}

	private CombatPresentationCardRoute ResolveSkillRoute(CardModel card, IReadOnlyList<string> identifiers)
	{
		if (ShouldUseBlockSkillRoute(card))
		{
			return CombatPresentationCardRoute.Skill;
		}
		if (ShouldUseEnemySkillDebuffRoute(card, identifiers, out string description))
		{
			return CombatPresentationCardRoute.EnemySkillDebuff;
		}
		if (description == null)
		{
			description = GetCardDescriptionText(card);
		}
		if (HasBuffU3DescriptionSignal(description))
		{
			return CombatPresentationCardRoute.BuffU3;
		}
		if (!IsBuffRouteSkill(card))
		{
			return CombatPresentationCardRoute.Skill;
		}
		return CombatPresentationCardRoute.BuffU2;
	}

	private bool ShouldUseEnemySkillDebuffRoute(CardModel card, IReadOnlyList<string> identifiers, out string? description)
	{
		description = null;
		if (!IsEnemyTargetedSkill(card))
		{
			return false;
		}
		if (ShouldUseAoeEnemySkillDebuffRoute(card))
		{
			return true;
		}
		if (_profile.MatchesExplicitRoute(identifiers, CombatPresentationCardRoute.EnemySkillDebuff) || CombatGeneratedCardSignalDetector.HasEnemyDebuffOnPlaySignal(card))
		{
			return true;
		}
		description = GetCardDescriptionText(card);
		return HasEnemySkillDebuffDescriptionSignal(description);
	}

	private static bool ShouldUseAoeEnemySkillDebuffRoute(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if ((int)card.Type == 2 && !card.GainsBlock)
		{
			return (int)card.TargetType == 3;
		}
		return false;
	}

	private static bool IsEnemyTargetedSkill(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((int)card.Type != 2)
		{
			return false;
		}
		TargetType targetType = card.TargetType;
		CombatPresentationTargetScope combatPresentationTargetScope = CombatPresentationTargetScopeResolver.Resolve(((object)(TargetType)(ref targetType)/*cast due to .constrained prefix*/).ToString());
		if ((uint)(combatPresentationTargetScope - 3) <= 2u)
		{
			return true;
		}
		return false;
	}

	private static bool IsBuffRouteSkill(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		TargetType targetType = card.TargetType;
		if ((int)targetType <= 1 || targetType - 5 <= 2)
		{
			return true;
		}
		return false;
	}

	private bool HasBuffU3DescriptionSignal(string description)
	{
		return _profile.HasBuffU3DescriptionSignal(description);
	}

	private bool HasEnemySkillDebuffDescriptionSignal(string description)
	{
		return _profile.HasEnemySkillDebuffDescriptionSignal(description);
	}

	private static string GetCardDescriptionText(CardModel card)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			CardPile pile = card.Pile;
			PileType val = (PileType)((pile != null) ? ((int)pile.Type) : 0);
			return card.GetDescriptionForPile(val, card.CurrentTarget) ?? string.Empty;
		}
		catch
		{
			return ((object)card.Description)?.ToString() ?? string.Empty;
		}
	}
}
