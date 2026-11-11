using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Powers;

public sealed class PlanetVenusPower : PowerModel
{
	public Player? PairedPlayer { get; set; }

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	public override PowerInstanceType InstanceType => (PowerInstanceType)1;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new StringVar[1]
	{
		new StringVar("PairedName", "")
	};

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (PairedPlayer != null)
		{
			((StringVar)((PowerModel)this).DynamicVars["PairedName"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, PairedPlayer.NetId);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)side != 1 || PairedPlayer == null || !participants.Contains(((PowerModel)this).Owner))
		{
			return;
		}
		List<CardModel> list = PileTypeExtensions.GetPile((PileType)3, PairedPlayer).Cards.Take(5).ToList();
		if (list.Count == 0)
		{
			return;
		}
		LocString val = new LocString("gameplay_ui", "PLANET_VENUS_SELECTION_PROMPT");
		CardSelectorPrefs val2 = default(CardSelectorPrefs);
		((CardSelectorPrefs)(ref val2))._002Ector(val, 0, list.Count);
		List<CardModel> list2 = (await CardSelectCmd.FromSimpleGrid(choiceContext, (IReadOnlyList<CardModel>)list, ((PowerModel)this).Owner.Player, val2)).ToList();
		((PowerModel)this).Flash();
		if (list2.Count == 0)
		{
			return;
		}
		foreach (CardModel item in list2)
		{
			await CardPileCmd.Add(item, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
		}
	}
}
