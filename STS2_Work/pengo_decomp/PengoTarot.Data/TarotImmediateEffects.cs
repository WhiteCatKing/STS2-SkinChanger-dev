using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Relics;

namespace PengoTarot.Data;

public static class TarotImmediateEffects
{
	public static bool CanWheelOfFortuneCopy(RelicModel relic)
	{
		return !(relic is StargazerKit);
	}

	public static async Task WheelOfFortuneUpright(Player player, RelicModel? targetRelic)
	{
		await CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), player.Creature, 11m, (ValueProp)14, (CardModel)null, (CardPlay)null);
		if (targetRelic != null && CanWheelOfFortuneCopy(targetRelic))
		{
			await RelicCmd.Obtain(ModelDb.GetById<RelicModel>(((AbstractModel)targetRelic).Id).ToMutable(), player, -1);
		}
	}

	public static async Task WheelOfFortuneReversed(Player player, List<RelicModel> toRemove, RelicModel? toClone)
	{
		if (player.Relics.Where((RelicModel r) => (int)r.Rarity != 7).ToList().Count < 3)
		{
			return;
		}
		foreach (RelicModel item in toRemove)
		{
			await RelicCmd.Remove(item);
		}
		if (toClone != null && CanWheelOfFortuneCopy(toClone))
		{
			for (int i = 0; i < 3; i++)
			{
				await RelicCmd.Obtain(ModelDb.GetById<RelicModel>(((AbstractModel)toClone).Id).ToMutable(), player, -1);
			}
		}
	}

	public static async Task TowerUpright(Player player)
	{
		List<CardModel> toRemove = player.Deck.Cards.Where((CardModel c) => (int)c.Rarity == 2 || (int)c.Rarity == 3 || (int)c.Rarity == 1).ToList();
		if (toRemove.Count != 0)
		{
			await CardPileCmd.RemoveFromDeck((IReadOnlyList<CardModel>)toRemove, true);
			await PlayerCmd.GainGold((decimal)(toRemove.Count * 45), player, false);
		}
	}

	public static async Task TowerReversed(Player player)
	{
		List<CardModel> toRemove = player.Deck.Cards.Where((CardModel c) => (int)c.Rarity == 2 || (int)c.Rarity == 1).ToList();
		if (toRemove.Count != 0)
		{
			await CardPileCmd.RemoveFromDeck((IReadOnlyList<CardModel>)toRemove, true);
			await PlayerCmd.GainGold((decimal)(toRemove.Count * 15), player, false);
		}
	}

	public static CardPoolModel GetTargetPool(string defId)
	{
		switch (defId)
		{
		case "DEVIL_UPRIGHT_SUB":
		case "DEVIL_REVERSED_SUB":
			return (CardPoolModel)(object)ModelDb.CardPool<IroncladCardPool>();
		case "STAR_REVERSED_SUB":
		case "STAR_UPRIGHT_SUB":
			return (CardPoolModel)(object)ModelDb.CardPool<RegentCardPool>();
		case "MOON_REVERSED_SUB":
		case "MOON_UPRIGHT_SUB":
			return (CardPoolModel)(object)ModelDb.CardPool<SilentCardPool>();
		case "SUN_REVERSED_SUB":
		case "SUN_UPRIGHT_SUB":
			return (CardPoolModel)(object)ModelDb.CardPool<NecrobinderCardPool>();
		case "WORLD_UPRIGHT_SUB":
		case "WORLD_REVERSED_SUB":
			return (CardPoolModel)(object)ModelDb.CardPool<DefectCardPool>();
		default:
			throw new InvalidOperationException("Unknown sub def: " + defId);
		}
	}
}
