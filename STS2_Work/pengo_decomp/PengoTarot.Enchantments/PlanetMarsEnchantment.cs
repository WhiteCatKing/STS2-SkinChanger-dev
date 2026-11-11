using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments;

public sealed class PlanetMarsEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 3;
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

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		object obj;
		if (cardPlay == null)
		{
			obj = null;
		}
		else
		{
			Creature target = cardPlay.Target;
			obj = ((target != null) ? target.Player : null);
		}
		if (obj == null)
		{
			return;
		}
		Player player2 = cardPlay.Target.Player;
		Player selfPlayer = ((EnchantmentModel)this).Card.Owner;
		HashSet<Player> connectedSet = new HashSet<Player>();
		Collect(selfPlayer);
		Collect(player2);
		foreach (Player player3 in connectedSet)
		{
			Creature creature = player3.Creature;
			PlanetMarsPower planetMarsPower = ((creature != null) ? creature.GetPower<PlanetMarsPower>() : null);
			if (planetMarsPower != null)
			{
				bool flag = false;
				foreach (Player item in connectedSet)
				{
					if (item != player3 && !planetMarsPower.PairedPlayers.Contains(item))
					{
						planetMarsPower.PairedPlayers.Add(item);
						flag = true;
					}
				}
				if (flag)
				{
					planetMarsPower.RefreshPairedName();
				}
			}
			else
			{
				PlanetMarsPower planetMarsPower2 = (PlanetMarsPower)(object)((PowerModel)ModelDb.Power<PlanetMarsPower>()).ToMutable(0);
				planetMarsPower2.PairedPlayers = connectedSet.Where((Player p) => p != player3).ToList();
				await PowerCmd.Apply(choiceContext, (PowerModel)(object)planetMarsPower2, player3.Creature, 1m, selfPlayer.Creature, ((EnchantmentModel)this).Card, false);
			}
		}
		void Collect(Player? player)
		{
			if (player != null && connectedSet.Add(player))
			{
				Creature creature2 = player.Creature;
				PlanetMarsPower planetMarsPower3 = ((creature2 != null) ? creature2.GetPower<PlanetMarsPower>() : null);
				if (planetMarsPower3 != null)
				{
					foreach (Player pairedPlayer in planetMarsPower3.PairedPlayers)
					{
						Collect(pairedPlayer);
					}
				}
			}
		}
	}
}
