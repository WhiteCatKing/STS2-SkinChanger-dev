using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Powers;

namespace PengoTarot.GameActions;

public sealed class TickTackGameAction : GameAction
{
	private readonly Player _player;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => (GameActionType)2;

	public TickTackGameAction(Player player)
	{
		_player = player;
	}

	protected override async Task ExecuteAction()
	{
		Creature creature = _player.Creature;
		if (creature == null)
		{
			return;
		}
		List<TickTackPower> list = creature.GetPowerInstances<TickTackPower>().ToList();
		bool anyReachedZero = false;
		foreach (TickTackPower power in list)
		{
			if (((AbstractModel)power).IsMutable && ((PowerModel)power).Amount > 0)
			{
				await PowerCmd.Decrement((PowerModel)(object)power);
				if (((PowerModel)power).Amount <= 0)
				{
					anyReachedZero = true;
				}
			}
		}
		if (anyReachedZero)
		{
			SfxCmd.Play("event:/sfx/debuff", 1f);
			PlayerCmd.EndTurn(_player, false, (Func<Task>)null);
		}
	}

	public override INetAction ToNetAction()
	{
		TickTackNetAction tickTackNetAction = default(TickTackNetAction);
		tickTackNetAction.TargetPlayerNetId = _player.NetId;
		return (INetAction)(object)tickTackNetAction;
	}

	public override string ToString()
	{
		return $"TickTackGameAction playerNetId={_player.NetId}";
	}
}
