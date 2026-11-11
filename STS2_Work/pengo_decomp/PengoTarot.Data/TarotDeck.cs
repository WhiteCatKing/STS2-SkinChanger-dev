using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;
using PengoTarot.Cards;
using PengoTarot.Data.Judgement;
using PengoTarot.Enchantments;

namespace PengoTarot.Data;

public static class TarotDeck
{
	public static readonly List<TarotDef> All = new List<TarotDef>
	{
		new TarotDef("FOOL_UPRIGHT", 75, typeof(TarFoolUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarFoolUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarFoolUprightEnchantment>()) >= 1),
		new TarotDef("FOOL_REVERSED", 75, typeof(TarFoolReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarFoolReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarFoolReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("MAGICIAN_UPRIGHT", 75, typeof(TarMagicianUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarMagicianUprightEnchantment>(), 5, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMagicianUprightEnchantment>()) >= 1),
		new TarotDef("MAGICIAN_REVERSED", 75, typeof(TarMagicianReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarMagicianReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMagicianReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("HIGH_PRIESTESS_UPRIGHT", 75, typeof(TarHighPriestessUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarHighPriestessUprightEnchantment>(), 3, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHighPriestessUprightEnchantment>()) >= 1),
		new TarotDef("HIGH_PRIESTESS_REVERSED", 75, typeof(TarHighPriestessReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarHighPriestessReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHighPriestessReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("EMPRESS_UPRIGHT", 75, typeof(TarEmpressUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarEmpressUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarEmpressUprightEnchantment>()) >= 1),
		new TarotDef("EMPRESS_REVERSED", 75, typeof(TarEmpressReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarEmpressReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarEmpressReversedEnchantment>()) >= 1),
		new TarotDef("EMPEROR_UPRIGHT", 75, typeof(TarEmperorUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarEmperorUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarEmperorUprightEnchantment>()) >= 1),
		new TarotDef("EMPEROR_REVERSED", 75, typeof(TarEmperorReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarEmperorReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarEmperorReversedEnchantment>()) >= 1),
		new TarotDef("HIEROPHANT_UPRIGHT", 75, typeof(TarHierophantUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarHierophantUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHierophantUprightEnchantment>()) >= 1),
		new TarotDef("HIEROPHANT_REVERSED", 75, typeof(TarHierophantReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarHierophantReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHierophantReversedEnchantment>()) >= 1),
		new TarotDef("LOVERS_UPRIGHT", 75, typeof(TarLoversUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarLoversUprightEnchantment>(), 2, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarLoversUprightEnchantment>()) >= 2, 1, 0.6),
		new TarotDef("LOVERS_REVERSED", 75, typeof(TarLoversReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarLoversReversedEnchantment>(), 2, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarLoversReversedEnchantment>()) >= 2, 1, 0.6),
		new TarotDef("CHARIOT_UPRIGHT", 75, typeof(TarChariotUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarChariotUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarChariotUprightEnchantment>()) >= 1),
		new TarotDef("CHARIOT_REVERSED", 75, typeof(TarChariotReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarChariotReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarChariotReversedEnchantment>()) >= 1),
		new TarotDef("STRENGTH_UPRIGHT", 75, typeof(TarStrengthUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarStrengthUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStrengthUprightEnchantment>()) >= 1),
		new TarotDef("STRENGTH_REVERSED", 75, typeof(TarStrengthReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarStrengthReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStrengthReversedEnchantment>()) >= 1),
		new TarotDef("HERMIT_UPRIGHT", 75, typeof(TarHermitUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarHermitUprightEnchantment>(), 2, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHermitUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("HERMIT_REVERSED", 75, typeof(TarHermitReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarHermitReversedEnchantment>(), 2, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHermitReversedEnchantment>()) >= 1),
		new TarotDef("JUSTICE_UPRIGHT", 75, typeof(TarJusticeUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarJusticeUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarJusticeUprightEnchantment>()) >= 1),
		new TarotDef("JUSTICE_REVERSED", 75, typeof(TarJusticeReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarJusticeReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarJusticeReversedEnchantment>()) >= 1),
		new TarotDef("HANGED_MAN_UPRIGHT", 75, typeof(TarHangedManUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarHangedManUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHangedManUprightEnchantment>()) >= 1),
		new TarotDef("HANGED_MAN_REVERSED", 75, typeof(TarHangedManReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarHangedManReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarHangedManReversedEnchantment>()) >= 1),
		new TarotDef("DEATH_UPRIGHT", 75, typeof(TarDeathUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarDeathUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDeathUprightEnchantment>()) >= 1),
		new TarotDef("DEATH_REVERSED", 75, typeof(TarDeathReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarDeathReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDeathReversedEnchantment>()) >= 1),
		new TarotDef("TEMPERANCE_UPRIGHT", 75, typeof(TarTemperanceUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarTemperanceUprightEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarTemperanceUprightEnchantment>()) >= 1),
		new TarotDef("TEMPERANCE_REVERSED", 75, typeof(TarTemperanceReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarTemperanceReversedEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarTemperanceReversedEnchantment>()) >= 1),
		new TarotDef("DEVIL_UPRIGHT", 75, typeof(TarDevilUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilUprightEnchantment>(), 1, (Player p) => p.Character is Ironclad, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("DEVIL_REVERSED", 75, typeof(TarDevilReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilReversedEnchantment>(), 1, (Player p) => p.Character is Ironclad, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("STAR_UPRIGHT", 75, typeof(TarStarUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarStarUprightEnchantment>(), 3, (Player p) => p.Character is Regent, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStarUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("STAR_REVERSED", 75, typeof(TarStarReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarStarReversedEnchantment>(), 1, (Player p) => p.Character is Regent, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStarReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("MOON_UPRIGHT", 75, typeof(TarMoonUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonUprightEnchantment>(), 1, (Player p) => p.Character is Silent, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("MOON_REVERSED", 75, typeof(TarMoonReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonReversedEnchantment>(), 1, (Player p) => p.Character is Silent, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("SUN_UPRIGHT", 75, typeof(TarSunUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarSunUprightEnchantment>(), 3, (Player p) => p.Character is Necrobinder, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarSunUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("SUN_REVERSED", 75, typeof(TarSunReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarSunReversedEnchantment>(), 1, (Player p) => p.Character is Necrobinder, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarSunReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("JUDGEMENT_UPRIGHT", 75, typeof(TarJudgementUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarJudgementUprightEnchantment>(), 2, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarJudgementUprightEnchantment>()) >= 1),
		new TarotDef("JUDGEMENT_REVERSED", 75, typeof(TarJudgementReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarJudgementReversedEnchantment>(), 21, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarJudgementReversedEnchantment>()) >= 1),
		new TarotDef("WORLD_UPRIGHT", 75, typeof(TarWorldUpright), (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldUprightEnchantment>(), 1, (Player p) => p.Character is Defect, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldUprightEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("WORLD_REVERSED", 75, typeof(TarWorldReversed), (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldReversedEnchantment>(), 1, (Player p) => p.Character is Defect, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldReversedEnchantment>()) >= 1, 1, 0.8),
		new TarotDef("WHEEL_OF_FORTUNE_UPRIGHT", 75, typeof(TarWheelOfFortuneUpright), (Player p, Rng rng) => Task.FromResult(result: true), null, 3, null, (Player p) => p.Creature.CurrentHp >= 11, 2),
		new TarotDef("WHEEL_OF_FORTUNE_REVERSED", 75, typeof(TarWheelOfFortuneReversed), (Player p, Rng rng) => Task.FromResult(result: true), null, 3, null, (Player p) => p.Relics.Count((RelicModel r) => (int)r.Rarity != 7) >= 3, 3),
		new TarotDef("TOWER_UPRIGHT", 75, typeof(TarTowerUpright), (Player p, Rng rng) => Task.FromResult(result: true), null, 3, null, delegate(Player p)
		{
			CardRarity[] array = new CardRarity[3];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			return HasAnyCardOfRarity(p, (CardRarity[])(object)array);
		}, 2, 0.8),
		new TarotDef("TOWER_REVERSED", 75, typeof(TarTowerReversed), (Player p, Rng rng) => Task.FromResult(result: true), null, 3, null, (Player p) => HasAnyCardOfRarity(p, (CardRarity)1, (CardRarity)2), 3, 0.8),
		new TarotDef("DEVIL_UPRIGHT_SUB", 75, typeof(TarDevilUprightSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarDevilUprightSubEnchantment>(), 3, (Player p) => !(p.Character is Ironclad), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilUprightSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("DEVIL_REVERSED_SUB", 75, typeof(TarDevilReversedSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarDevilReversedSubEnchantment>(), 3, (Player p) => !(p.Character is Ironclad), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarDevilReversedSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("STAR_UPRIGHT_SUB", 75, typeof(TarStarUprightSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarStarUprightSubEnchantment>(), 3, (Player p) => !(p.Character is Regent), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStarUprightSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("STAR_REVERSED_SUB", 75, typeof(TarStarReversedSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarStarReversedSubEnchantment>(), 3, (Player p) => !(p.Character is Regent), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarStarReversedSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("MOON_UPRIGHT_SUB", 75, typeof(TarMoonUprightSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarMoonUprightSubEnchantment>(), 3, (Player p) => !(p.Character is Silent), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonUprightSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("MOON_REVERSED_SUB", 75, typeof(TarMoonReversedSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarMoonReversedSubEnchantment>(), 3, (Player p) => !(p.Character is Silent), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarMoonReversedSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("SUN_UPRIGHT_SUB", 75, typeof(TarSunUprightSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarSunUprightSubEnchantment>(), 3, (Player p) => !(p.Character is Necrobinder), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarSunUprightSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("SUN_REVERSED_SUB", 75, typeof(TarSunReversedSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarSunReversedSubEnchantment>(), 3, (Player p) => !(p.Character is Necrobinder), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarSunReversedSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("WORLD_UPRIGHT_SUB", 75, typeof(TarWorldUprightSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarWorldUprightSubEnchantment>(), 3, (Player p) => !(p.Character is Defect), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldUprightSubEnchantment>()) >= 1, 1, 0.0325),
		new TarotDef("WORLD_REVERSED_SUB", 75, typeof(TarWorldReversedSub), null, (EnchantmentModel?)(object)ModelDb.Enchantment<TarWorldReversedSubEnchantment>(), 3, (Player p) => !(p.Character is Defect), (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<TarWorldReversedSubEnchantment>()) >= 1, 1, 0.0325)
	};

	private static int CountEnchantableCards(Player player, EnchantmentModel enchantment)
	{
		return player.Piles.First((CardPile p) => (int)p.Type == 6).Cards.Count((CardModel card) => enchantment.CanEnchant(card));
	}

	private static bool HasAnyCardOfRarity(Player player, params CardRarity[] rarities)
	{
		return player.Piles.First((CardPile p) => (int)p.Type == 6).Cards.Any((CardModel card) => rarities.Contains(card.Rarity));
	}

	public static List<TarotDef> DrawThreeUnique(Player player, Rng rng)
	{
		return DrawUnique(player, rng, 3, includeReversed: false, includeCharacterSpecific: false);
	}

	public static List<TarotDef> DrawUnique(Player player, Rng rng, int count, bool includeReversed, bool includeCharacterSpecific, Func<TarotDef, bool>? extraFilter = null)
	{
		int currentAct = player.RunState.CurrentActIndex + 1;
		List<TarotDef> collection = All.Where((TarotDef def) => (includeReversed || !def.Id.EndsWith("_REVERSED")) && (includeCharacterSpecific || def.RequiredCharacter == null) && (def.RequiredCharacter == null || def.RequiredCharacter(player)) && (JudgementDivination.IsEnabled || def.AvailabilityCheck == null || def.AvailabilityCheck(player)) && (extraFilter == null || extraFilter(def)) && def.MinAct <= currentAct).ToList();
		List<TarotDef> list = new List<TarotDef>();
		List<TarotDef> list2 = new List<TarotDef>(collection);
		for (int i = 0; i < count; i++)
		{
			if (list2.Count <= 0)
			{
				break;
			}
			double num = list2.Sum((TarotDef def) => def.Weight);
			if (num <= 0.0)
			{
				break;
			}
			double num2 = (double)rng.NextFloat(1f) * num;
			double num3 = 0.0;
			TarotDef tarotDef = null;
			for (int j = 0; j < list2.Count; j++)
			{
				num3 += list2[j].Weight;
				if (num2 <= num3)
				{
					tarotDef = list2[j];
					list2.RemoveAt(j);
					break;
				}
			}
			if (tarotDef != null)
			{
				list.Add(tarotDef);
			}
		}
		return list;
	}
}
