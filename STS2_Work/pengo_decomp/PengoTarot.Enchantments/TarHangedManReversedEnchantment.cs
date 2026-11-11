using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHangedManReversedEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return card.Keywords.Contains((CardKeyword)1);
	}

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (card == ((EnchantmentModel)this).Card)
		{
			cardLocation.pileType = (PileType)3;
			return cardLocation;
		}
		return cardLocation;
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		if (((EnchantmentModel)this).Card == null)
		{
			return;
		}
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, ((EnchantmentModel)this).Card.Owner).Cards.Where((CardModel c) => c != ((EnchantmentModel)this).Card).ToList();
		if (list.Count > 0)
		{
			CardModel val = ((EnchantmentModel)this).Card.Owner.RunState.Rng.CombatCardSelection.NextItem<CardModel>((IEnumerable<CardModel>)list);
			if (val != null)
			{
				await CardCmd.Exhaust(choiceContext, val, false, false);
			}
		}
	}
}
