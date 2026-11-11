using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class PlanetXEnchantment : EnchantmentModel
{
	private bool _usedThisCombat;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.Static((StaticHoverTip)14, (DynamicVar[])(object)new DynamicVar[1]
	{
		new DynamicVar("Times", 1m)
	}) };

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

	public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		if (_usedThisCombat)
		{
			return Task.CompletedTask;
		}
		_usedThisCombat = true;
		((EnchantmentModel)this).Status = (EnchantmentStatus)1;
		if (((EnchantmentModel)this).Card.CombatState == null)
		{
			return Task.CompletedTask;
		}
		foreach (Player item in ((EnchantmentModel)this).Card.CombatState.Players.Where((Player p) => p != ((EnchantmentModel)this).Card.Owner))
		{
			foreach (CardModel allCard in item.PlayerCombatState.AllCards)
			{
				if (allCard.Enchantment is PlanetXEnchantment)
				{
					allCard.BaseReplayCount += 4;
				}
			}
		}
		return Task.CompletedTask;
	}
}
