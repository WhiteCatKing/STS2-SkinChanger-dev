using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarTemperanceUprightEnchantment : EnchantmentModel
{
	private bool _triggered;

	public override bool HasExtraCardText => true;

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == ((EnchantmentModel)this).Card && !_triggered)
		{
			_triggered = true;
			((EnchantmentModel)this).Status = (EnchantmentStatus)1;
			CardModel card = ((EnchantmentModel)this).Card;
			if (((card != null) ? card.Owner : null) != null)
			{
				await PlayerCmd.GainGold(10m, ((EnchantmentModel)this).Card.Owner, false);
			}
		}
	}
}
