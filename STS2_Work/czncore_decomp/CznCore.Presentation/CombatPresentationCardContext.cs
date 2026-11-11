using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CombatPresentationCardContext(CombatPresentationCardRoute Route, string? CardId, string? CardName, string? CardType, string? TargetType, CombatPresentationTargetScope TargetScope, int EnergyCost = 0, bool CostsX = false, string? Rarity = null, int HitCount = 1, float AttackAnimDelaySeconds = 0f, decimal DamagePerHit = 0m, bool IsTargetCharacter = false, bool IsLocalPlayer = false, IReadOnlyList<string>? CardIdentifiers = null, bool IsUpgraded = false, int CurrentUpgradeLevel = 0, string? CardPoolTitle = null, string? CardPoolTypeName = null, string? CardPoolFullTypeName = null, bool IsColorlessCardPool = false, CombatPresentationCardSourceCategory SourceCategory = CombatPresentationCardSourceCategory.Unknown)
{
	public static CombatPresentationCardContext Unknown { get; } = new CombatPresentationCardContext(CombatPresentationCardRoute.Unknown, null, null, null, null, CombatPresentationTargetScope.Unknown);

	public bool IsAttack => Route == CombatPresentationCardRoute.Attack;

	public bool IsAreaTarget
	{
		get
		{
			CombatPresentationTargetScope targetScope = TargetScope;
			if (targetScope == CombatPresentationTargetScope.AllEnemies || targetScope == CombatPresentationTargetScope.All)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsSupplementalCardSource
	{
		get
		{
			CombatPresentationCardSourceCategory sourceCategory = SourceCategory;
			if ((uint)(sourceCategory - 2) <= 2u)
			{
				return true;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private CombatPresentationCardContext(CombatPresentationCardContext original)
	{
		Route = original.Route;
		CardId = original.CardId;
		CardName = original.CardName;
		CardType = original.CardType;
		TargetType = original.TargetType;
		TargetScope = original.TargetScope;
		EnergyCost = original.EnergyCost;
		CostsX = original.CostsX;
		Rarity = original.Rarity;
		HitCount = original.HitCount;
		AttackAnimDelaySeconds = original.AttackAnimDelaySeconds;
		DamagePerHit = original.DamagePerHit;
		IsTargetCharacter = original.IsTargetCharacter;
		IsLocalPlayer = original.IsLocalPlayer;
		CardIdentifiers = original.CardIdentifiers;
		IsUpgraded = original.IsUpgraded;
		CurrentUpgradeLevel = original.CurrentUpgradeLevel;
		CardPoolTitle = original.CardPoolTitle;
		CardPoolTypeName = original.CardPoolTypeName;
		CardPoolFullTypeName = original.CardPoolFullTypeName;
		IsColorlessCardPool = original.IsColorlessCardPool;
		SourceCategory = original.SourceCategory;
	}
}
