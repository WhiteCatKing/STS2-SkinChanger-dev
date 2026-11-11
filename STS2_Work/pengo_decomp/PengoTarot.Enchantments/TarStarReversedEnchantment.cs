using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarStarReversedEnchantment : EnchantmentModel
{
	private enum SwapMode
	{
		None,
		ToStarX,
		ToEnergyX
	}

	private SwapMode _swapMode;

	public override bool HasExtraCardText => true;

	public bool IsSwappedToStarX
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			if (_swapMode == SwapMode.ToStarX)
			{
				return (int)((EnchantmentModel)this).Status == 0;
			}
			return false;
		}
	}

	public bool IsSwappedToEnergyX
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			if (_swapMode == SwapMode.ToEnergyX)
			{
				return (int)((EnchantmentModel)this).Status == 0;
			}
			return false;
		}
	}

	public override bool CanEnchant(CardModel card)
	{
		return ((EnchantmentModel)this).CanEnchant(card);
	}

	protected override void OnEnchant()
	{
		ApplyCostSwap();
	}

	private void ApplyCostSwap()
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (card != null)
		{
			CardEnergyCost energyCost = card.EnergyCost;
			bool costsX = energyCost.CostsX;
			bool hasStarCostX = card.HasStarCostX;
			int withModifiers = energyCost.GetWithModifiers((CostModifiers)0);
			int baseStarCost = card.BaseStarCost;
			if (costsX && !hasStarCostX)
			{
				RuntimeEnergyXCostHelper.SetCostsX(energyCost, value: false);
				energyCost.SetCustomBaseCost((baseStarCost >= 0) ? baseStarCost : 0);
				StarXReflectionHelper.SetStarCost(card, -1);
				_swapMode = SwapMode.ToStarX;
			}
			else if (!costsX && hasStarCostX)
			{
				RuntimeEnergyXCostHelper.SetCostsX(energyCost, value: true);
				StarXReflectionHelper.SetStarCost(card, -1);
				_swapMode = SwapMode.ToEnergyX;
			}
			else if (!costsX && !hasStarCostX)
			{
				int customBaseCost = ((baseStarCost >= 0) ? baseStarCost : 0);
				int cost = withModifiers;
				energyCost.SetCustomBaseCost(customBaseCost);
				StarXReflectionHelper.SetStarCost(card, cost);
				_swapMode = SwapMode.None;
			}
		}
	}
}
