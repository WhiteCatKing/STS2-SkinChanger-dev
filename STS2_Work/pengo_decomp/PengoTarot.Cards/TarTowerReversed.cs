using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Cards;

public sealed class TarTowerReversed : TarCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			if (!JudgementDivination.IsEnabledForCardDisplay)
			{
				return ((CardModel)this).ExtraHoverTips;
			}
			return (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromCard<TheBomb>(false) };
		}
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		((CardModel)this).AddExtraArgsToDescription(description);
		description.Add("JudgementDivination", JudgementDivination.IsEnabledForCardDisplay);
	}

	protected override async Task<bool> ExecutePortableCombatEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ICombatState combatState = ((CardModel)this).CombatState ?? throw new InvalidOperationException("高塔逆位的携带式效果只能在战斗中执行。");
		int count = Math.Max(0, CardPile.MaxCardsInHand - CardPile.GetCards(((CardModel)this).Owner, (PileType[])(object)new PileType[1] { (PileType)2 }).Count());
		List<TheBomb> list = (from _ in Enumerable.Range(0, count)
			select combatState.CreateCard<TheBomb>(((CardModel)this).Owner)).ToList();
		foreach (TheBomb item in list)
		{
			((CardModel)item).SetToFreeThisCombat();
		}
		await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>)list, (PileType)2, ((CardModel)this).Owner, (CardPilePosition)1);
		return true;
	}
}
