using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Powers;

public sealed class PlanetMarsPower : PowerModel
{
	public List<Player> PairedPlayers { get; set; } = new List<Player>();

	public bool Syncing { get; set; }

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new StringVar[1]
	{
		new StringVar("PairedName", "")
	};

	public void RefreshPairedName()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (PairedPlayers.Count != 0)
		{
			IEnumerable<string> values = PairedPlayers.Select((Player p) => PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, p.NetId));
			((StringVar)((PowerModel)this).DynamicVars["PairedName"]).StringValue = string.Join(", ", values);
		}
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		RefreshPairedName();
		return Task.CompletedTask;
	}

	public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (Syncing || PairedPlayers.Count == 0)
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		if (creator != ((owner != null) ? owner.Player : null))
		{
			return;
		}
		Syncing = true;
		foreach (Player pairedPlayer in PairedPlayers)
		{
			PlanetMarsPower power = pairedPlayer.Creature.GetPower<PlanetMarsPower>();
			if (power != null)
			{
				power.Syncing = true;
			}
		}
		foreach (Player pairedPlayer2 in PairedPlayers)
		{
			CardModel obj = CreateCloneForPlayer(card, pairedPlayer2);
			CardPile pile = card.Pile;
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(obj, (PileType)((pile != null) ? ((int)pile.Type) : 2), pairedPlayer2, (CardPilePosition)1), 1.2f, (CardPreviewStyle)1);
		}
		foreach (Player pairedPlayer3 in PairedPlayers)
		{
			PlanetMarsPower power2 = pairedPlayer3.Creature.GetPower<PlanetMarsPower>();
			if (power2 != null)
			{
				power2.Syncing = false;
			}
		}
		Syncing = false;
		((PowerModel)this).Flash();
	}

	public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
	{
		if (Syncing || PairedPlayers.Count == 0)
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		if (player != ((owner != null) ? owner.Player : null))
		{
			return;
		}
		Syncing = true;
		foreach (Player pairedPlayer in PairedPlayers)
		{
			PlanetMarsPower power = pairedPlayer.Creature.GetPower<PlanetMarsPower>();
			if (power != null)
			{
				power.Syncing = true;
			}
		}
		foreach (Player pairedPlayer2 in PairedPlayers)
		{
			OrbModel val = ModelDb.GetById<OrbModel>(((AbstractModel)orb).Id).ToMutable(0);
			await OrbCmd.Channel(choiceContext, val, pairedPlayer2);
		}
		foreach (Player pairedPlayer3 in PairedPlayers)
		{
			PlanetMarsPower power2 = pairedPlayer3.Creature.GetPower<PlanetMarsPower>();
			if (power2 != null)
			{
				power2.Syncing = false;
			}
		}
		Syncing = false;
		((PowerModel)this).Flash();
	}

	public override async Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
	{
		if (Syncing || PairedPlayers.Count == 0)
		{
			return;
		}
		Creature owner = ((PowerModel)this).Owner;
		if (summoner != ((owner != null) ? owner.Player : null))
		{
			return;
		}
		Syncing = true;
		foreach (Player pairedPlayer in PairedPlayers)
		{
			PlanetMarsPower power = pairedPlayer.Creature.GetPower<PlanetMarsPower>();
			if (power != null)
			{
				power.Syncing = true;
			}
		}
		foreach (Player pairedPlayer2 in PairedPlayers)
		{
			await OstyCmd.Summon(choiceContext, pairedPlayer2, amount, (AbstractModel)(object)this);
		}
		foreach (Player pairedPlayer3 in PairedPlayers)
		{
			PlanetMarsPower power2 = pairedPlayer3.Creature.GetPower<PlanetMarsPower>();
			if (power2 != null)
			{
				power2.Syncing = false;
			}
		}
		Syncing = false;
		((PowerModel)this).Flash();
	}

	public static CardModel CreateCloneForPlayer(CardModel card, Player player)
	{
		CardModel val = card.CreateClone();
		typeof(CardModel).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(val, player);
		return val;
	}
}
