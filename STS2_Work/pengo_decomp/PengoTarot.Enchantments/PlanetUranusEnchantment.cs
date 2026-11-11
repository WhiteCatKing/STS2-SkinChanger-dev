using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Enchantments;

public sealed class PlanetUranusEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.Static((StaticHoverTip)14, (DynamicVar[])(object)new DynamicVar[1]
	{
		new DynamicVar("Times", 1m)
	}) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 1;
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

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (card == ((EnchantmentModel)this).Card)
		{
			cardLocation.pileType = (PileType)0;
			return cardLocation;
		}
		return cardLocation;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card != ((EnchantmentModel)this).Card || !cardPlay.IsLastInSeries || ((EnchantmentModel)this).Card.CombatState == null)
		{
			return;
		}
		List<Creature> list = (from c in ((EnchantmentModel)this).Card.CombatState.GetTeammatesOf(((EnchantmentModel)this).Card.Owner.Creature)
			where c.IsPlayer && c.Player != ((EnchantmentModel)this).Card.Owner
			select c).ToList();
		if (list.Count == 0)
		{
			return;
		}
		RunRngSet rng = ((EnchantmentModel)this).Card.Owner.RunState.Rng;
		if (((rng != null) ? rng.CombatTargets : null) != null)
		{
			Creature val = rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>)list);
			if (val.Player != null)
			{
				CardModel val2 = ((EnchantmentModel)this).Card.CreateClone();
				val2.EnergyCost.AddThisCombat(1, false);
				val2.BaseReplayCount += 2;
				typeof(CardModel).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(val2, val.Player);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(val2, (PileType)1, val.Player, (CardPilePosition)3), 1.2f, (CardPreviewStyle)1);
			}
		}
	}
}
