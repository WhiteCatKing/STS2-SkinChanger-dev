using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarEmpressReversedEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override void OnEnchant()
	{
		((EnchantmentModel)this).Card.EnergyCost.UpgradeBy(-1);
		((EnchantmentModel)this).Status = (EnchantmentStatus)0;
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (card == ((EnchantmentModel)this).Card)
		{
			return (int)((EnchantmentModel)this).Status == 1;
		}
		return true;
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (card == ((EnchantmentModel)this).Card && (int)((EnchantmentModel)this).Status == 0)
		{
			CardPile pile = card.Pile;
			if (pile != null && (int)pile.Type == 3)
			{
				((EnchantmentModel)this).Status = (EnchantmentStatus)1;
			}
		}
		return Task.CompletedTask;
	}
}
