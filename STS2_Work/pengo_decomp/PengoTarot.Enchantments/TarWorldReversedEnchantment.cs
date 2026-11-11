using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PengoTarot.Enchantments;

public sealed class TarWorldReversedEnchantment : EnchantmentModel
{
	private bool _triggered;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromPower<ArtifactPower>((int?)null) };

	public override bool HasExtraCardText => true;

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) != null && !_triggered)
		{
			_triggered = true;
			((EnchantmentModel)this).Status = (EnchantmentStatus)1;
			await PowerCmd.Apply<ArtifactPower>(choiceContext, ((EnchantmentModel)this).Card.Owner.Creature, 1m, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
		}
	}
}
