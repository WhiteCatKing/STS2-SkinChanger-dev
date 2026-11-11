using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PengoTarot.ConfigFW;

namespace PengoTarot.Data.Judgement;

public sealed class DiscardPortableTarotAction : GameAction
{
	private readonly Player _player;

	private readonly int _slotIndex;

	private readonly string _expectedTarotId;

	private readonly uint _expectedRevision;

	public override ulong OwnerId => _player.NetId;

	public bool WasEnqueuedInCombat { get; }

	public override GameActionType ActionType
	{
		get
		{
			if (WasEnqueuedInCombat)
			{
				return (GameActionType)2;
			}
			return (GameActionType)3;
		}
	}

	public DiscardPortableTarotAction(Player player, int slotIndex, string expectedTarotId, uint expectedRevision, bool wasEnqueuedInCombat)
	{
		_player = player;
		_slotIndex = slotIndex;
		_expectedTarotId = expectedTarotId;
		_expectedRevision = expectedRevision;
		WasEnqueuedInCombat = wasEnqueuedInCombat;
	}

	protected override Task ExecuteAction()
	{
		if (!JudgementTarotInventoryState.TryGetSlot(_player.NetId, _slotIndex, out string tarotId, out uint revision) || revision != _expectedRevision || tarotId != _expectedTarotId)
		{
			((GameAction)this).Cancel();
			return Task.CompletedTask;
		}
		if (LocalContext.IsMe(_player))
		{
			NConfigFloatingWindowEntryButton.PreparePortableTarotDiscardAnimation(_slotIndex);
		}
		if (!JudgementTarotInventoryState.TryDiscard(_player.NetId, _slotIndex, _expectedTarotId, _expectedRevision))
		{
			((GameAction)this).Cancel();
			return Task.CompletedTask;
		}
		return Task.CompletedTask;
	}

	public override INetAction ToNetAction()
	{
		NetDiscardPortableTarotAction netDiscardPortableTarotAction = default(NetDiscardPortableTarotAction);
		netDiscardPortableTarotAction.SlotIndex = _slotIndex;
		netDiscardPortableTarotAction.ExpectedTarotId = _expectedTarotId;
		netDiscardPortableTarotAction.ExpectedRevision = _expectedRevision;
		netDiscardPortableTarotAction.WasEnqueuedInCombat = WasEnqueuedInCombat;
		return (INetAction)(object)netDiscardPortableTarotAction;
	}
}
