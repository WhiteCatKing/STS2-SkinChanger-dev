using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Cards;

public sealed class TarTowerUpright : TarCard
{
	private const int BombsToPlay = 6;

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
		ICombatState val = ((CardModel)this).CombatState ?? throw new InvalidOperationException("高塔正位的携带式效果只能在战斗中执行。");
		TheBomb bomb = val.CreateCard<TheBomb>(((CardModel)this).Owner);
		for (int i = 0; i < 6; i++)
		{
			CardModel val2 = ((CardModel)bomb).CreateDupe(((CardModel)this).Owner);
			await CardCmd.AutoPlay(choiceContext, val2, (Creature)null, (AutoPlayType)1, false, false);
		}
		return true;
	}
}
