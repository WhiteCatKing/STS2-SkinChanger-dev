using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace PengoTarot.Enchantments;

public sealed class TarHierophantUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool CanEnchant(CardModel card)
	{
		return ((EnchantmentModel)this).CanEnchant(card);
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		Player owner = ((EnchantmentModel)this).Card.Owner;
		CardModel val = await CardSelectCmd.FromHandForUpgrade(choiceContext, owner, (AbstractModel)(object)this);
		if (val != null)
		{
			CardCmd.Upgrade(val, (CardPreviewStyle)1);
			CardCmd.Preview(val, 1.2f, (CardPreviewStyle)1);
		}
	}
}
