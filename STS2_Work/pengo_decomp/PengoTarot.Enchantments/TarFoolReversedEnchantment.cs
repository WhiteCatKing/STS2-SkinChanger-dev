using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace PengoTarot.Enchantments;

public sealed class TarFoolReversedEnchantment : EnchantmentModel
{
	private bool _triggered;

	public override bool HasExtraCardText => true;

	public int Generation { get; internal set; }

	internal bool IgnoreGenerationCost { get; set; }

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		if (card.EnergyCost.CostsX)
		{
			return false;
		}
		if (card.HasStarCostX)
		{
			return false;
		}
		return true;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card != ((EnchantmentModel)this).Card || _triggered)
		{
			return;
		}
		CardModel val = ((EnchantmentModel)this).Card.CreateClone();
		if (val != null)
		{
			CardCmd.ClearEnchantment(val);
			CardCmd.Enchant<TarFoolReversedEnchantment>(val, 1m);
			if (val.Enchantment is TarFoolReversedEnchantment tarFoolReversedEnchantment)
			{
				tarFoolReversedEnchantment.Generation = Generation + 1;
				tarFoolReversedEnchantment.IgnoreGenerationCost = IgnoreGenerationCost;
			}
			if (cardPlay.Card is Stomp)
			{
				val.EnergyCost.AddThisTurn(1, false);
			}
			await CardPileCmd.AddGeneratedCardToCombat(val, (PileType)2, ((EnchantmentModel)this).Card.Owner, (CardPilePosition)1);
		}
		_triggered = true;
		((EnchantmentModel)this).Status = (EnchantmentStatus)1;
	}

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card != ((EnchantmentModel)this).Card || Generation <= 0 || IgnoreGenerationCost)
		{
			return false;
		}
		modifiedCost = originalCost + (decimal)Generation;
		return true;
	}
}
