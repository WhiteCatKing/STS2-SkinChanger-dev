using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace PengoTarot.Data.Judgement;

public sealed class UseWheelOfFortuneAction : GameAction
{
	private readonly Player _player;

	private readonly int _slot;

	private readonly string _id;

	private readonly uint _revision;

	private readonly PortableTarotUseContext _context;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => (GameActionType)3;

	public UseWheelOfFortuneAction(Player player, int slot, string id, uint revision, PortableTarotUseContext context)
	{
		_player = player;
		_slot = slot;
		_id = id;
		_revision = revision;
		_context = context;
	}

	protected override Task ExecuteAction()
	{
		if (!JudgementTarotInventoryState.TryGetSlot(_player.NetId, _slot, out string tarotId, out uint revision) || !(tarotId == _id) || revision != _revision || !TarotDeck.All.Any((TarotDef d) => d.Id == _id) || !JudgementTarotInventoryState.TryDiscard(_player.NetId, _slot, _id, _revision))
		{
			((GameAction)this).Cancel();
			return Task.CompletedTask;
		}
		bool flag = _id == "WHEEL_OF_FORTUNE_UPRIGHT";
		if (_player.PlayerRng.Rewards.NextInt(4) < ((!flag) ? 1 : 3))
		{
			try
			{
				WheelOfFortuneRelicCopyState.Apply(_player, _context, flag ? 2 : 3);
			}
			catch (Exception value)
			{
				Log.Error($"[PengoTarot] 命运之轮复制应用失败：{value}", 2);
			}
		}
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		NetUseWheelOfFortuneAction netUseWheelOfFortuneAction = default(NetUseWheelOfFortuneAction);
		netUseWheelOfFortuneAction.Slot = _slot;
		netUseWheelOfFortuneAction.Id = _id;
		netUseWheelOfFortuneAction.Revision = _revision;
		netUseWheelOfFortuneAction.Context = (int)_context;
		return (INetAction)(object)netUseWheelOfFortuneAction;
	}
}
