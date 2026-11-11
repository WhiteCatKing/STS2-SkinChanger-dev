using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarMoonUprightEnchantment : EnchantmentModel
{
	private bool _triggered;

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType != 3;
	}

	public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) == null || _triggered || ((EnchantmentModel)this).Card == null || ((EnchantmentModel)this).Card.Owner != player)
		{
			return;
		}
		PlayerCombatState playerCombatState = player.PlayerCombatState;
		if (playerCombatState != null && playerCombatState.TurnNumber == 1)
		{
			CardPile pile = ((EnchantmentModel)this).Card.Pile;
			if (pile != null && pile.IsCombatPile)
			{
				_triggered = true;
				((EnchantmentModel)this).Status = (EnchantmentStatus)1;
				await CardCmd.AutoPlay(choiceContext, ((EnchantmentModel)this).Card, (Creature)null, (AutoPlayType)1, false, false);
				await CardPileCmd.Add(((EnchantmentModel)this).Card, PileTypeExtensions.GetPile((PileType)1, player), (CardPilePosition)3, (AbstractModel)null, false);
			}
		}
	}
}
