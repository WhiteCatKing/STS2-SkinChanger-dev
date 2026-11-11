using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PengoTarot.Enchantments;

public sealed class TarSunReversedEnchantment : EnchantmentModel
{
	private int _originalEnergyCost;

	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromPower<DoomPower>((int?)null) };

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
		_originalEnergyCost = ((EnchantmentModel)this).Card.EnergyCost.GetWithModifiers((CostModifiers)0);
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

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) != null)
		{
			int num = _originalEnergyCost * 6;
			if (num > 0)
			{
				await PowerCmd.Apply<DoomPower>(choiceContext, ((EnchantmentModel)this).Card.Owner.Creature, (decimal)num, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
			}
		}
	}
}
