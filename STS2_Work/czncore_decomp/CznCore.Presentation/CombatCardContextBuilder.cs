using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class CombatCardContextBuilder
{
	private readonly CombatCardRouteResolver _routeResolver;

	private readonly Func<Player?, bool> _isTargetPlayer;

	private readonly Func<Player?, bool> _isLocalPlayer;

	public CombatCardContextBuilder(CombatCardPresentationProfile profile, Func<Player?, bool> isTargetPlayer, Func<Player?, bool> isLocalPlayer)
	{
		_routeResolver = new CombatCardRouteResolver(profile);
		_isTargetPlayer = isTargetPlayer ?? throw new ArgumentNullException("isTargetPlayer");
		_isLocalPlayer = isLocalPlayer ?? throw new ArgumentNullException("isLocalPlayer");
	}

	public CombatPresentationCardRoute GetRoute(CardModel? card)
	{
		return _routeResolver.Resolve(card);
	}

	public CombatPresentationCardContext BuildPreview(CardModel card)
	{
		var (damagePerHit, hitCount) = ReadPreviewAttackFacts(card);
		return Build(card, hitCount, 0f, damagePerHit);
	}

	public static (decimal Damage, int Hits) ReadPreviewAttackFacts(CardModel card)
	{
		decimal item = (from value in card.DynamicVars.Values
			where (value is DamageVar || value is CalculatedDamageVar || value is OstyDamageVar) ? true : false
			select value.PreviewValue).DefaultIfEmpty(0m).Max();
		int item2 = (from value in card.DynamicVars.Values.OfType<RepeatVar>()
			select (int)((DynamicVar)value).PreviewValue).DefaultIfEmpty(1).Max();
		return (Damage: item, Hits: item2);
	}

	public CombatPresentationCardContext Build(CardModel? card, int hitCount = 1, float attackAnimDelaySeconds = 0f, decimal damagePerHit = 0m)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (card == null)
		{
			return CombatPresentationCardContext.Unknown;
		}
		TargetType targetType = card.TargetType;
		string targetType2 = ((object)(TargetType)(ref targetType)/*cast due to .constrained prefix*/).ToString();
		CardRarity rarity = card.Rarity;
		string rarity2 = ((object)(CardRarity)(ref rarity)/*cast due to .constrained prefix*/).ToString();
		(string, string, string, bool) tuple = ResolveCardPoolFacts(card);
		IReadOnlyList<string> cardIdentifiers = CombatCardIdentifiers.Collect(card);
		CombatPresentationCardRoute route = _routeResolver.Resolve(card, cardIdentifiers);
		string entry = ((AbstractModel)card).Id.Entry;
		string? cardName = ((object)((AbstractModel)card).Id)?.ToString();
		CardType type = card.Type;
		return new CombatPresentationCardContext(route, entry, cardName, ((object)(CardType)(ref type)/*cast due to .constrained prefix*/).ToString(), targetType2, CombatPresentationTargetScopeResolver.Resolve(targetType2), ResolveEnergyCost(card), card.EnergyCost.CostsX, rarity2, Math.Max(1, hitCount), Math.Max(0f, attackAnimDelaySeconds), Math.Max(0m, damagePerHit), _isTargetPlayer(card.Owner), _isLocalPlayer(card.Owner), cardIdentifiers, card.IsUpgraded, Math.Max(0, card.CurrentUpgradeLevel), tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, ResolveCardSourceCategory(rarity2, tuple.Item4));
	}

	private static CombatPresentationCardSourceCategory ResolveCardSourceCategory(string? rarity, bool isColorlessCardPool)
	{
		if (isColorlessCardPool)
		{
			return CombatPresentationCardSourceCategory.Colorless;
		}
		if (IsRarity(rarity, "Event"))
		{
			return CombatPresentationCardSourceCategory.Event;
		}
		if (IsRarity(rarity, "Token"))
		{
			return CombatPresentationCardSourceCategory.Token;
		}
		if (!string.IsNullOrWhiteSpace(rarity))
		{
			return CombatPresentationCardSourceCategory.Standard;
		}
		return CombatPresentationCardSourceCategory.Unknown;
	}

	private static bool IsRarity(string? rarity, string expected)
	{
		return string.Equals(rarity, expected, StringComparison.OrdinalIgnoreCase);
	}

	private static (string? Title, string? TypeName, string? FullTypeName, bool IsColorless) ResolveCardPoolFacts(CardModel card)
	{
		try
		{
			CardPoolModel pool = card.Pool;
			Type type = ((object)pool).GetType();
			return (Title: pool.Title, TypeName: type.Name, FullTypeName: type.FullName, IsColorless: pool.IsColorless);
		}
		catch
		{
			return (Title: null, TypeName: null, FullTypeName: null, IsColorless: false);
		}
	}

	private static int ResolveEnergyCost(CardModel card)
	{
		CardEnergyCost energyCost = card.EnergyCost;
		if (energyCost.CostsX)
		{
			return Math.Max(0, ResolveXEnergyCost(card, energyCost));
		}
		return Math.Max(0, TryGetResolvedEnergyCost(energyCost, energyCost.Canonical));
	}

	private static int ResolveXEnergyCost(CardModel card, CardEnergyCost energyCost)
	{
		if (energyCost.CapturedXValue > 0)
		{
			return energyCost.CapturedXValue;
		}
		if (card.LastStarsSpent > 0)
		{
			return card.LastStarsSpent;
		}
		return TryGetAmountToSpend(energyCost, TryGetResolvedEnergyCost(energyCost, energyCost.Canonical));
	}

	private static int TryGetResolvedEnergyCost(CardEnergyCost energyCost, int fallback)
	{
		try
		{
			return energyCost.GetResolved();
		}
		catch
		{
			return fallback;
		}
	}

	private static int TryGetAmountToSpend(CardEnergyCost energyCost, int fallback)
	{
		try
		{
			return energyCost.GetAmountToSpend();
		}
		catch
		{
			return fallback;
		}
	}
}
