using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments;

public sealed class PlanetMercuryEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 3;
	}

	public override bool CanEnchant(CardModel card)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		if ((int)card.MultiplayerConstraint == 1)
		{
			return false;
		}
		return true;
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		object obj;
		if (cardPlay == null)
		{
			obj = null;
		}
		else
		{
			Creature target = cardPlay.Target;
			obj = ((target != null) ? target.Player : null);
		}
		if (obj != null)
		{
			PowerModel val = ((PowerModel)ModelDb.Power<PlanetMercuryPower>()).ToMutable(0);
			if (val is PlanetMercuryPower planetMercuryPower)
			{
				planetMercuryPower.PairedPlayer = cardPlay.Target.Player;
			}
			await PowerCmd.Apply(choiceContext, val, ((EnchantmentModel)this).Card.Owner.Creature, 1m, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
		}
	}
}
