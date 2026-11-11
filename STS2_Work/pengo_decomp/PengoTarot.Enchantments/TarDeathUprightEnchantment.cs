using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarDeathUprightEnchantment : EnchantmentModel
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
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) != null)
		{
			PlayerCmd.EndTurn(((EnchantmentModel)this).Card.Owner, false, (Func<Task>)null);
		}
		return Task.CompletedTask;
	}
}
