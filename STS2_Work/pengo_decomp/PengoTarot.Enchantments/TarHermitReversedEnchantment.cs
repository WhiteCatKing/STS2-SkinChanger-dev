using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHermitReversedEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType != 3;
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (((EnchantmentModel)this).Card != null && ((EnchantmentModel)this).Card.Owner == player)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			if (playerCombatState != null && playerCombatState.TurnNumber == 1 && ((EnchantmentModel)this).Card.Pile != null && (int)((EnchantmentModel)this).Card.Pile.Type != 4)
			{
				await CardCmd.Exhaust(choiceContext, ((EnchantmentModel)this).Card, false, false);
			}
		}
	}

	public override async Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
	{
		if (((EnchantmentModel)this).Card != null && ((EnchantmentModel)this).Card.Owner == player && player.PlayerCombatState != null && player.PlayerCombatState.TurnNumber == 7)
		{
			CardPile pile = ((EnchantmentModel)this).Card.Pile;
			if (pile != null && (int)pile.Type == 4)
			{
				await CardPileCmd.Add(((EnchantmentModel)this).Card, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				((EnchantmentModel)this).Card.SetToFreeThisCombat();
			}
		}
	}
}
