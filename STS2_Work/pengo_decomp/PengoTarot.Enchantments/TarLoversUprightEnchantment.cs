using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarLoversUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (card == ((EnchantmentModel)this).Card)
		{
			CardPile pile = card.Pile;
			if (pile != null && (int)pile.Type == 2 && (int)oldPile != 2)
			{
				CardModel val = FindOtherLoversCard(card);
				if (val != null)
				{
					return CardPileCmd.Add(val, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				}
				return Task.CompletedTask;
			}
		}
		return Task.CompletedTask;
	}

	private CardModel? FindOtherLoversCard(CardModel self)
	{
		CardModel obj = self;
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			Player owner = obj.Owner;
			obj2 = ((owner != null) ? owner.PlayerCombatState : null);
		}
		if (obj2 == null)
		{
			return null;
		}
		return self.Owner.PlayerCombatState.AllCards.FirstOrDefault((Func<CardModel, bool>)delegate(CardModel c)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			if (c != self)
			{
				CardPile pile = c.Pile;
				if (pile == null || (int)pile.Type != 2)
				{
					CardPile pile2 = c.Pile;
					if (pile2 == null || (int)pile2.Type != 5)
					{
						if (!(c.Enchantment is TarLoversUprightEnchantment))
						{
							return c.Enchantment is TarLoversReversedEnchantment;
						}
						return true;
					}
				}
			}
			return false;
		});
	}
}
