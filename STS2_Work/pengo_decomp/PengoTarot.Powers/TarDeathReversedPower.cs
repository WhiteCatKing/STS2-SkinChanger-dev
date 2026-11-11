using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Powers;

public sealed class TarDeathReversedPower : PowerModel
{
	public override PowerType Type => (PowerType)0;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override bool IsVisibleInternal => false;

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		if (cardPlay.Card.Owner.Creature != ((PowerModel)this).Owner)
		{
			return Task.CompletedTask;
		}
		if ((int)cardPlay.Card.Type != 3)
		{
			return Task.CompletedTask;
		}
		if (((PowerModel)this).Owner.Player == null)
		{
			return Task.CompletedTask;
		}
		((PowerModel)this).Flash();
		PlayerCmd.EndTurn(((PowerModel)this).Owner.Player, false, (Func<Task>)null);
		return Task.CompletedTask;
	}
}
