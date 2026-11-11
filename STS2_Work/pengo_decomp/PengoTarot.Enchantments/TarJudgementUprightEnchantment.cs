using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace PengoTarot.Enchantments;

public sealed class TarJudgementUprightEnchantment : EnchantmentModel
{
	private bool _hasTransformed;

	public override bool HasExtraCardText => true;

	public override async Task BeforeCombatStart()
	{
		if (!_hasTransformed && ((EnchantmentModel)this).Card != null && ((EnchantmentModel)this).Card.Owner != null && ((EnchantmentModel)this).Card.IsInCombat)
		{
			_hasTransformed = true;
			bool wasUpgraded = ((EnchantmentModel)this).Card.IsUpgraded;
			CardModel card = ((EnchantmentModel)this).Card;
			CardPileAddResult val = await CardCmd.TransformToRandom(card, card.Owner.RunState.Rng.Shuffle, (CardPreviewStyle)1);
			if (val.success && val.cardAdded != null && wasUpgraded)
			{
				CardCmd.Upgrade(val.cardAdded, (CardPreviewStyle)0);
			}
		}
	}
}
