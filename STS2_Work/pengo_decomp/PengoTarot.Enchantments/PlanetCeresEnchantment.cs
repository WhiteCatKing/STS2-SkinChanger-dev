using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments;

public sealed class PlanetCeresEnchantment : EnchantmentModel
{
	private bool _usedThisCombat;

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 2;
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
		if (_usedThisCombat)
		{
			return;
		}
		_usedThisCombat = true;
		((EnchantmentModel)this).Status = (EnchantmentStatus)1;
		if (((EnchantmentModel)this).Card.CombatState == null)
		{
			return;
		}
		foreach (Player player in ((EnchantmentModel)this).Card.CombatState.Players.Where((Player p) => p != ((EnchantmentModel)this).Card.Owner))
		{
			List<CardModel> list = player.PlayerCombatState.AllCards.Where((CardModel c) => c.Enchantment is PlanetCeresEnchantment).ToList();
			foreach (CardModel item in list)
			{
				CardModel obj = PlanetMarsPower.CreateCloneForPlayer(item, player);
				CardPile pile = item.Pile;
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(obj, (PileType)((pile != null) ? ((int)pile.Type) : 3), player, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
			}
		}
	}
}
