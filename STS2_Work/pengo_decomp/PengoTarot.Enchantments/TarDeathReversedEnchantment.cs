using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarDeathReversedEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShouldGlowRed => true;

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

	protected override void OnEnchant()
	{
		((EnchantmentModel)this).Card.EnergyCost.SetCustomBaseCost(0);
		if (((EnchantmentModel)this).Card.BaseStarCost >= 0)
		{
			StarXReflectionHelper.SetStarCost(((EnchantmentModel)this).Card, 0);
		}
	}

	public override Task BeforeCombatStart()
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (card != null)
		{
			card.SetToFreeThisCombat();
		}
		return Task.CompletedTask;
	}

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card != ((EnchantmentModel)this).Card)
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	public override bool ShouldDraw(Player player, bool fromHandDraw)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if (((EnchantmentModel)this).Card == null)
		{
			return true;
		}
		if (((EnchantmentModel)this).Card.Owner != player)
		{
			return true;
		}
		CardPile pile = ((EnchantmentModel)this).Card.Pile;
		if (pile == null)
		{
			return true;
		}
		return (int)pile.Type != 2;
	}
}
