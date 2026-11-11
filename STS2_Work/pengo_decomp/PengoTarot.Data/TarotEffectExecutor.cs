using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Data;

public static class TarotEffectExecutor
{
	public static async Task<bool> ExecuteEffectAndEnchant(TarotDef def, Player player, bool isLocalBuyer, Func<Task>? onConfirmed = null)
	{
		CardSelectorPrefs val;
		Dictionary<CardModel, List<CardModel>> optionsMap;
		IEnumerable<CardModel> chosenCards;
		if (def.IsImmediateEffect)
		{
			if (def.Enchantment != null && def.Id.EndsWith("_SUB"))
			{
				val = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1, def.CardsToEnchant);
				((CardSelectorPrefs)(ref val)).set_Cancelable(true);
				((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
				CardSelectorPrefs val2 = val;
				optionsMap = new Dictionary<CardModel, List<CardModel>>();
				chosenCards = await CardSelectCmd.FromDeckForTransformation(player, val2, (Func<CardModel, CardTransformation>)BuildTransformation);
				if (chosenCards == null || !chosenCards.Any())
				{
					return false;
				}
				if (onConfirmed != null)
				{
					await onConfirmed();
				}
				if (isLocalBuyer)
				{
					Rng rng = player.PlayerRng.Shops;
					foreach (CardModel item in chosenCards)
					{
						if (optionsMap.TryGetValue(item, out List<CardModel> value) && value.Count != 0)
						{
							CardModel val3 = rng.NextItem<CardModel>((IEnumerable<CardModel>)value);
							await CardCmd.Transform(item, val3, (CardPreviewStyle)1);
						}
					}
					NotifyStateChange(player);
				}
				return true;
			}
			if (onConfirmed != null)
			{
				await onConfirmed();
			}
			if (isLocalBuyer)
			{
				Rng shops = player.PlayerRng.Shops;
				switch (def.Id)
				{
				case "WHEEL_OF_FORTUNE_UPRIGHT":
				{
					List<RelicModel> list5 = player.Relics.Where((RelicModel r) => (int)r.Rarity != 7 && TarotImmediateEffects.CanWheelOfFortuneCopy(r)).ToList();
					RelicModel targetRelic = null;
					if (list5.Count > 0)
					{
						List<RelicModel> list6 = list5.Where((RelicModel r) => (int)r.Rarity != 1).ToList();
						targetRelic = ((list6.Count > 0) ? shops.NextItem<RelicModel>((IEnumerable<RelicModel>)list6) : shops.NextItem<RelicModel>((IEnumerable<RelicModel>)list5));
					}
					await TarotImmediateEffects.WheelOfFortuneUpright(player, targetRelic);
					NotifyStateChange(player);
					break;
				}
				case "WHEEL_OF_FORTUNE_REVERSED":
				{
					List<RelicModel> list = player.Relics.Where((RelicModel r) => (int)r.Rarity != 7).ToList();
					if (list.Count < 3)
					{
						break;
					}
					List<RelicModel> list2 = new List<RelicModel>(list);
					List<RelicModel> list3 = new List<RelicModel>();
					for (int i = 0; i < 2; i++)
					{
						RelicModel val4 = shops.NextItem<RelicModel>((IEnumerable<RelicModel>)list2);
						if (val4 != null)
						{
							list3.Add(val4);
							list2.Remove(val4);
						}
					}
					List<RelicModel> list4 = list2.Where(TarotImmediateEffects.CanWheelOfFortuneCopy).ToList();
					RelicModel toClone = ((list4.Count > 0) ? shops.NextItem<RelicModel>((IEnumerable<RelicModel>)list4) : null);
					await TarotImmediateEffects.WheelOfFortuneReversed(player, list3, toClone);
					NotifyStateChange(player);
					break;
				}
				case "TOWER_UPRIGHT":
					await TarotImmediateEffects.TowerUpright(player);
					NotifyStateChange(player);
					break;
				case "TOWER_REVERSED":
					await TarotImmediateEffects.TowerReversed(player);
					NotifyStateChange(player);
					break;
				}
			}
			return true;
		}
		EnchantmentModel enchantment = def.Enchantment;
		int cardsToEnchant = def.CardsToEnchant;
		string id2 = def.Id;
		bool flag = ((id2 == "LOVERS_UPRIGHT" || id2 == "LOVERS_REVERSED") ? true : false);
		int num = ((!flag) ? 1 : cardsToEnchant);
		val = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, num, cardsToEnchant);
		((CardSelectorPrefs)(ref val)).set_Cancelable(true);
		((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
		CardSelectorPrefs val5 = val;
		chosenCards = await CardSelectCmd.FromDeckForEnchantment(player, enchantment, cardsToEnchant, val5);
		if (chosenCards == null || !chosenCards.Any())
		{
			return false;
		}
		if (onConfirmed != null)
		{
			await onConfirmed();
		}
		foreach (CardModel item2 in chosenCards)
		{
			CardCmd.Enchant(enchantment.ToMutable(), item2, 1m);
			if (LocalContext.IsMe(player))
			{
				CardCmd.Preview(item2, 1.2f, (CardPreviewStyle)1);
			}
		}
		return true;
		CardTransformation BuildTransformation(CardModel card)
		{
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			List<CardModel> list7 = (from id in TarotImmediateEffects.GetTargetPool(def.Id).AllCardIds
				select ModelDb.GetById<CardModel>(id) into c
				where c != null && c.Type == card.Type && c.Rarity == card.Rarity
				select ((ICardScope)player.RunState).CreateCard(c, player)).ToList();
			foreach (CardModel item3 in list7)
			{
				CardCmd.Enchant(def.Enchantment.ToMutable(), item3, 1m);
			}
			optionsMap[card] = list7;
			return new CardTransformation(card, (IEnumerable<CardModel>)list7);
		}
	}

	public static void NotifyStateChange(Player player)
	{
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer && ModInitializer.TarotSync != null)
		{
			ModInitializer.TarotSync.SendForceSyncMessage(player.ToSerializable());
		}
	}
}
