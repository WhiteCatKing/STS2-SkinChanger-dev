using System;
using CznCore.Presentation;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinCombatRouteRules
{
	internal enum MeirinCardRarityTier
	{
		BasicOrCommon,
		Uncommon,
		Rare,
		Ancient,
		TokenOrEvent,
		Other
	}

	public const bool UgPresentationAvailable = true;

	public const bool UxPresentationAvailable = true;

	private const decimal U2DamageThreshold = 20m;

	private const decimal UgScoreThreshold = 40m;

	private const decimal RareScoreBonus = 3m;

	private const decimal AncientScoreBonus = 20m;

	private const decimal AreaScoreBonus = 16m;

	private const int UxPowerMinimumEnergyCost = 3;

	private const decimal TwoCostScoreBonus = 3m;

	private const decimal ThreeOrMoreCostScoreBonus = 8m;

	public static MeirinCombatRoute ResolveAttackRoute(CombatPresentationCardContext context)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Invalid comparison between Unknown and I4
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Invalid comparison between Unknown and I4
		ArgumentNullException.ThrowIfNull(context, "context");
		int num = Math.Max(1, context.HitCount);
		bool flag = num >= 2;
		if (flag)
		{
			CombatPresentationTargetScope targetScope = context.TargetScope;
			bool flag2 = (((int)targetScope == 3 || (int)targetScope == 5) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return MeirinCombatRoute.MultiHitAttack;
		}
		MeirinCardRarityTier meirinCardRarityTier = ResolveRarityTier(context.Rarity);
		bool flag3 = IsWeightedAttackRarity(meirinCardRarityTier);
		bool flag4 = context.DamagePerHit >= 20m;
		if (context.IsAreaTarget)
		{
			if (num != 1)
			{
				if (!flag3)
				{
					return MeirinCombatRoute.NormalAttack;
				}
				return MeirinCombatRoute.UgAttack;
			}
			if (!(flag3 && flag4) || !(ResolveAttackScore(context, meirinCardRarityTier, 16m) >= 40m))
			{
				return MeirinCombatRoute.U2Attack;
			}
			return MeirinCombatRoute.UgAttack;
		}
		flag = !flag3 || !flag4 || num != 1;
		if (!flag)
		{
			CombatPresentationTargetScope targetScope = context.TargetScope;
			bool flag2 = (((int)targetScope == 3 || (int)targetScope == 5) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			return MeirinCombatRoute.NormalAttack;
		}
		if (!(ResolveAttackScore(context, meirinCardRarityTier, 0m) >= 40m))
		{
			return MeirinCombatRoute.U2Attack;
		}
		return MeirinCombatRoute.UgAttack;
	}

	private static decimal ResolveAttackScore(CombatPresentationCardContext context, MeirinCardRarityTier rarityTier, decimal areaScoreBonus)
	{
		return context.DamagePerHit + ResolveRarityScoreBonus(rarityTier) + ResolveEnergyScoreBonus(context.EnergyCost) + areaScoreBonus;
	}

	public static MeirinCombatRoute ResolveSupportRoute(CombatPresentationCardContext context, bool gainsBlock, MeirinCardCapabilities capabilities, MeirinCardOrigin origin, bool allowUx)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		ArgumentNullException.ThrowIfNull(context, "context");
		bool flag = IsCardType(context.CardType, "Power");
		if (allowUx && QualifiesForUx(context, capabilities, origin, flag))
		{
			return MeirinCombatRoute.UxBuff;
		}
		if (flag || capabilities.MantraRelated)
		{
			return MeirinCombatRoute.U4Buff;
		}
		if (capabilities.ChangesStance)
		{
			return MeirinCombatRoute.U3Buff;
		}
		if (capabilities.AppliesPower && IsEnemyScope(context.TargetScope))
		{
			return MeirinCombatRoute.Debuff;
		}
		if (!gainsBlock)
		{
			return MeirinCombatRoute.U1Buff;
		}
		return MeirinCombatRoute.Defense;
	}

	private static bool QualifiesForUx(CombatPresentationCardContext context, MeirinCardCapabilities capabilities, MeirinCardOrigin origin, bool isPower)
	{
		if (IsCardType(context.CardType, "Skill") && capabilities.EntersDivinity)
		{
			return true;
		}
		if (!isPower)
		{
			return false;
		}
		if (origin == MeirinCardOrigin.NonWatcher)
		{
			return context.EnergyCost >= 3;
		}
		return ResolveRarityTier(context.Rarity) switch
		{
			MeirinCardRarityTier.Ancient => true, 
			MeirinCardRarityTier.Rare => context.EnergyCost >= 3, 
			_ => false, 
		};
	}

	private static bool IsEnemyScope(CombatPresentationTargetScope scope)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		if (scope - 3 <= 2)
		{
			return true;
		}
		return false;
	}

	private static bool IsWeightedAttackRarity(MeirinCardRarityTier tier)
	{
		if ((uint)(tier - 1) <= 3u)
		{
			return true;
		}
		return false;
	}

	private static decimal ResolveRarityScoreBonus(MeirinCardRarityTier tier)
	{
		return tier switch
		{
			MeirinCardRarityTier.Rare => 3m, 
			MeirinCardRarityTier.Ancient => 20m, 
			_ => 0m, 
		};
	}

	private static decimal ResolveEnergyScoreBonus(int energyCost)
	{
		if (energyCost >= 3)
		{
			return 8m;
		}
		if (energyCost != 2)
		{
			return 0m;
		}
		return 3m;
	}

	internal static MeirinCardRarityTier ResolveRarityTier(string? rarity)
	{
		if (string.IsNullOrWhiteSpace(rarity))
		{
			return MeirinCardRarityTier.Other;
		}
		if (IsRarity(rarity, "Basic") || IsRarity(rarity, "Common") || IsRarity(rarity, "Starter"))
		{
			return MeirinCardRarityTier.BasicOrCommon;
		}
		if (IsRarity(rarity, "Uncommon"))
		{
			return MeirinCardRarityTier.Uncommon;
		}
		if (IsRarity(rarity, "Rare"))
		{
			return MeirinCardRarityTier.Rare;
		}
		if (IsRarity(rarity, "Ancient"))
		{
			return MeirinCardRarityTier.Ancient;
		}
		if (IsRarity(rarity, "Token") || IsRarity(rarity, "Event"))
		{
			return MeirinCardRarityTier.TokenOrEvent;
		}
		return MeirinCardRarityTier.Other;
	}

	private static bool IsRarity(string rarity, string expected)
	{
		return string.Equals(rarity, expected, StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsCardType(string? cardType, string expected)
	{
		return string.Equals(cardType, expected, StringComparison.OrdinalIgnoreCase);
	}
}
