using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data.Divination;

namespace PengoTarot.Powers;

public sealed class TarJusticeReversedPower : PowerModel
{
	public override PowerType Type => (PowerType)0;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == ((PowerModel)this).Owner && (int)cardPlay.Card.Type == 1 && NormalDivinationTurnState.CountCardsPlayedThisTurn(((PowerModel)this).Owner, (CardType)1) == 1)
		{
			await CardCmd.Exhaust(choiceContext, cardPlay.Card, false, false);
		}
	}
}
