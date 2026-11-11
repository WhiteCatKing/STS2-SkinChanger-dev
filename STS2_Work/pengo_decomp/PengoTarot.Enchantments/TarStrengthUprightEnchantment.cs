using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PengoTarot.Utils;

namespace PengoTarot.Enchantments;

public sealed class TarStrengthUprightEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromPower<WeakPower>((int?)null) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 1;
	}

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return !MultiHitDetector.IsMultiHitCard(card);
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		if (((EnchantmentModel)this).Card == null)
		{
			return;
		}
		IReadOnlyList<Creature> readOnlyList;
		if ((int)((EnchantmentModel)this).Card.TargetType == 3)
		{
			readOnlyList = ((EnchantmentModel)this).Card.CombatState.HittableEnemies.ToList();
		}
		else
		{
			if (((cardPlay != null) ? cardPlay.Target : null) == null)
			{
				return;
			}
			readOnlyList = (IReadOnlyList<Creature>)(object)new Creature[1] { cardPlay.Target };
		}
		await PowerCmd.Apply<WeakPower>(choiceContext, (IEnumerable<Creature>)readOnlyList, 1m, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
	}
}
