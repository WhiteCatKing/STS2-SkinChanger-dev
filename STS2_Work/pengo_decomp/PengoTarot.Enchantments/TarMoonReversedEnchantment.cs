using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarMoonReversedEnchantment : EnchantmentModel
{
	private bool _allowDiscard;

	private bool _endOfTurnDiscard;

	private bool _slyReturnPending;

	private bool _returning;

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType != 3;
	}

	public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		_allowDiscard = true;
		return Task.CompletedTask;
	}

	public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		_endOfTurnDiscard = true;
		return Task.CompletedTask;
	}

	public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
	{
		_endOfTurnDiscard = false;
		return Task.CompletedTask;
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (card != ((EnchantmentModel)this).Card || _returning)
		{
			return;
		}
		CardPile pile = card.Pile;
		if (pile != null && (int)pile.Type == 2)
		{
			_allowDiscard = false;
		}
		if ((int)oldPile != 2)
		{
			return;
		}
		CardPile pile2 = card.Pile;
		if (pile2 != null && (int)pile2.Type == 3)
		{
			if (_endOfTurnDiscard)
			{
				_returning = true;
				await CardPileCmd.Add(card, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				_returning = false;
			}
			else if (_allowDiscard)
			{
				_allowDiscard = false;
			}
			else if (card.IsSlyThisTurn)
			{
				_slyReturnPending = true;
			}
			else
			{
				_returning = true;
				await CardPileCmd.Add(card, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				_returning = false;
			}
		}
	}

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (card != ((EnchantmentModel)this).Card || !_slyReturnPending)
		{
			return cardLocation;
		}
		_slyReturnPending = false;
		if ((int)card.Type == 3)
		{
			return cardLocation;
		}
		cardLocation.pileType = (PileType)2;
		cardLocation.position = (CardPilePosition)1;
		return cardLocation;
	}
}
