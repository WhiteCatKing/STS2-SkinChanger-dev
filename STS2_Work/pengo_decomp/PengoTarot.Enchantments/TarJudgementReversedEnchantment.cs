using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace PengoTarot.Enchantments;

public sealed class TarJudgementReversedEnchantment : EnchantmentModel
{
	private static readonly HashSet<Player> _processedPlayers = new HashSet<Player>();

	private static readonly object _lock = new object();

	public override bool HasExtraCardText => true;

	public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) != player)
		{
			return Task.CompletedTask;
		}
		PlayerCombatState playerCombatState = player.PlayerCombatState;
		if (playerCombatState == null || playerCombatState.TurnNumber != 1)
		{
			return Task.CompletedTask;
		}
		lock (_lock)
		{
			if (!_processedPlayers.Add(player))
			{
				return Task.CompletedTask;
			}
		}
		PlayerCombatState playerCombatState2 = player.PlayerCombatState;
		foreach (CardModel item in ((playerCombatState2 != null) ? (from c in playerCombatState2.AllCards.Where(delegate(CardModel c)
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Invalid comparison between Unknown and I4
				if (c.Enchantment is TarJudgementReversedEnchantment)
				{
					CardPile pile = c.Pile;
					if (pile == null)
					{
						return false;
					}
					return (int)pile.Type == 1;
				}
				return false;
			})
			orderby c.FloorAddedToDeck ?? int.MaxValue descending
			select c).ToList() : null) ?? new List<CardModel>())
		{
			CardPileCmd.Add(item, (PileType)1, (CardPilePosition)1, (AbstractModel)null, false);
		}
		return Task.CompletedTask;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		lock (_lock)
		{
			_processedPlayers.Clear();
		}
		return Task.CompletedTask;
	}
}
