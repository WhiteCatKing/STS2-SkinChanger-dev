using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;

namespace PengoTarot.Data;

public static class TarotCombatEffectExecutor
{
	public static async Task<bool> Execute(TarCard tarot, PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		TarotDef def = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.CardType == ((object)tarot).GetType());
		if (def?.Enchantment == null || def.CardsToEnchant <= 0)
		{
			return false;
		}
		EnchantmentModel enchantment = def.Enchantment;
		if (((IEnumerable<CardModel>)PileTypeExtensions.GetPile((PileType)2, ((CardModel)tarot).Owner).Cards).Count((Func<CardModel, bool>)enchantment.CanEnchant) < def.CardsToEnchant)
		{
			return false;
		}
		CardSelectorPrefs val = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, def.CardsToEnchant, def.CardsToEnchant);
		((CardSelectorPrefs)(ref val)).set_Cancelable(true);
		((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
		CardSelectorPrefs val2 = val;
		List<CardModel> list = (await CardSelectCmd.FromHand(choiceContext, ((CardModel)tarot).Owner, val2, (Func<CardModel, bool>)enchantment.CanEnchant, (AbstractModel)(object)tarot)).ToList();
		if (list.Count != def.CardsToEnchant || list.Any(delegate(CardModel card)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			CardPile pile = card.Pile;
			return pile == null || (int)pile.Type != 2 || !enchantment.CanEnchant(card);
		}))
		{
			return false;
		}
		foreach (CardModel item in list)
		{
			CardCmd.Enchant(enchantment.ToMutable(), item, 1m);
		}
		return true;
	}
}
