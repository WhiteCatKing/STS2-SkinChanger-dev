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

public sealed class PlanetEarthEnchantment : EnchantmentModel
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
		Player targetPlayer = cardPlay.Target.Player;
		Player selfPlayer = ((EnchantmentModel)this).Card.Owner;
		HashSet<Player> connectedSet = new HashSet<Player>();
		Collect(selfPlayer);
		Collect(targetPlayer);
		bool flag = false;
		foreach (Player player2 in connectedSet)
		{
			Creature creature = player2.Creature;
			PlanetEarthPower planetEarthPower = ((creature != null) ? creature.GetPower<PlanetEarthPower>() : null);
			if (planetEarthPower != null)
			{
				bool flag2 = false;
				foreach (Player item in connectedSet)
				{
					if (item != player2 && !planetEarthPower.PairedPlayers.Contains(item))
					{
						planetEarthPower.PairedPlayers.Add(item);
						flag2 = true;
						flag = true;
					}
				}
				if (flag2)
				{
					planetEarthPower.RefreshPairedName();
				}
			}
			else
			{
				PlanetEarthPower planetEarthPower2 = (PlanetEarthPower)(object)((PowerModel)ModelDb.Power<PlanetEarthPower>()).ToMutable(0);
				planetEarthPower2.PairedPlayers = connectedSet.Where((Player p) => p != player2).ToList();
				await PowerCmd.Apply(choiceContext, (PowerModel)(object)planetEarthPower2, player2.Creature, 1m, selfPlayer.Creature, ((EnchantmentModel)this).Card, false);
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		PlayerCombatState playerCombatState = selfPlayer.PlayerCombatState;
		int num = ((playerCombatState != null) ? playerCombatState.Energy : 0);
		PlayerCombatState playerCombatState2 = targetPlayer.PlayerCombatState;
		int totalEnergy = num + ((playerCombatState2 != null) ? playerCombatState2.Energy : 0);
		foreach (Player item2 in connectedSet)
		{
			await PlayerCmd.SetEnergy((decimal)totalEnergy, item2);
		}
		void Collect(Player? player)
		{
			if (player != null && connectedSet.Add(player))
			{
				Creature creature2 = player.Creature;
				PlanetEarthPower planetEarthPower3 = ((creature2 != null) ? creature2.GetPower<PlanetEarthPower>() : null);
				if (planetEarthPower3 != null)
				{
					foreach (Player pairedPlayer in planetEarthPower3.PairedPlayers)
					{
						Collect(pairedPlayer);
					}
				}
			}
		}
	}
}
