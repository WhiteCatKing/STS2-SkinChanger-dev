using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data;
using PengoTarot.Data.Divination;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Network;

public class TarotSynchronizer : IDisposable
{
	private readonly RunLocationTargetedMessageBuffer _messageBuffer;

	private readonly INetGameService _gameService;

	private readonly IPlayerCollection _playerCollection;

	private readonly ulong _localPlayerId;

	public TarotSynchronizer(RunLocationTargetedMessageBuffer messageBuffer, INetGameService gameService, IPlayerCollection playerCollection, ulong localPlayerId)
	{
		_messageBuffer = messageBuffer;
		_gameService = gameService;
		_playerCollection = playerCollection;
		_localPlayerId = localPlayerId;
		_messageBuffer.RegisterMessageHandler<TarotPurchaseRequestMessage>((MessageHandlerDelegate<TarotPurchaseRequestMessage>)HandleTarotPurchaseRequest);
		_messageBuffer.RegisterMessageHandler<ForcePlayerSyncMessage>((MessageHandlerDelegate<ForcePlayerSyncMessage>)HandleForcePlayerSyncMessage);
		_messageBuffer.RegisterMessageHandler<TarotMarkerStateMessage>((MessageHandlerDelegate<TarotMarkerStateMessage>)HandleTarotMarkerStateMessage);
		_messageBuffer.RegisterMessageHandler<PortableTarotInventoryStateMessage>((MessageHandlerDelegate<PortableTarotInventoryStateMessage>)HandlePortableTarotInventoryStateMessage);
		JudgementTarotInventoryState.Changed += HandlePortableTarotInventoryChanged;
	}

	public void Dispose()
	{
		_messageBuffer.UnregisterMessageHandler<TarotPurchaseRequestMessage>((MessageHandlerDelegate<TarotPurchaseRequestMessage>)HandleTarotPurchaseRequest);
		_messageBuffer.UnregisterMessageHandler<ForcePlayerSyncMessage>((MessageHandlerDelegate<ForcePlayerSyncMessage>)HandleForcePlayerSyncMessage);
		_messageBuffer.UnregisterMessageHandler<TarotMarkerStateMessage>((MessageHandlerDelegate<TarotMarkerStateMessage>)HandleTarotMarkerStateMessage);
		_messageBuffer.UnregisterMessageHandler<PortableTarotInventoryStateMessage>((MessageHandlerDelegate<PortableTarotInventoryStateMessage>)HandlePortableTarotInventoryStateMessage);
		JudgementTarotInventoryState.Changed -= HandlePortableTarotInventoryChanged;
	}

	public void SendMessage(TarotPurchaseRequestMessage msg)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		msg.Location = _messageBuffer.CurrentLocation;
		_gameService.SendMessage<TarotPurchaseRequestMessage>(msg);
	}

	public void SendForceSyncMessage(SerializablePlayer serializablePlayer)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ForcePlayerSyncMessage forcePlayerSyncMessage = default(ForcePlayerSyncMessage);
		forcePlayerSyncMessage.player = serializablePlayer;
		forcePlayerSyncMessage.Location = _messageBuffer.CurrentLocation;
		ForcePlayerSyncMessage forcePlayerSyncMessage2 = forcePlayerSyncMessage;
		_gameService.SendMessage<ForcePlayerSyncMessage>(forcePlayerSyncMessage2);
	}

	public void BroadcastMarkerState()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_gameService.Type == 2)
		{
			_gameService.SendMessage<TarotMarkerStateMessage>(new TarotMarkerStateMessage
			{
				json = TarotMarkerSystem.ToJson().ToJsonString(),
				Location = _messageBuffer.CurrentLocation
			});
		}
	}

	private void HandlePortableTarotInventoryChanged(ulong _)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_gameService.Type == 2)
		{
			_gameService.SendMessage<PortableTarotInventoryStateMessage>(new PortableTarotInventoryStateMessage
			{
				Json = JudgementTarotInventoryState.ToJson().ToJsonString(),
				Location = _messageBuffer.CurrentLocation
			});
		}
	}

	private void HandleTarotPurchaseRequest(TarotPurchaseRequestMessage message, ulong senderId)
	{
		if (senderId != _localPlayerId)
		{
			Player player = _playerCollection.GetPlayer(senderId);
			if (player != null)
			{
				TaskHelper.RunSafely((Task)MerchantTarotEntry.HandleRemoteTarotPurchase(player, message.goldCost, message.cachedDefIds));
			}
		}
	}

	private void HandleForcePlayerSyncMessage(ForcePlayerSyncMessage message, ulong senderId)
	{
		if (senderId != _localPlayerId)
		{
			Player player = _playerCollection.GetPlayer(senderId);
			if (player != null)
			{
				player.SyncWithSerializedPlayer(message.player);
			}
		}
	}

	private void HandleTarotMarkerStateMessage(TarotMarkerStateMessage message, ulong senderId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		if (senderId == _localPlayerId || (int)_gameService.Type != 3)
		{
			return;
		}
		INetGameService gameService = _gameService;
		INetClientGameService val = (INetClientGameService)(object)((gameService is INetClientGameService) ? gameService : null);
		if (val == null || val.NetClient == null || senderId != val.NetClient.HostNetId)
		{
			return;
		}
		try
		{
			TarotMarkerSystem.FromJson(JsonNode.Parse(message.json, null) as JsonObject);
		}
		catch
		{
		}
	}

	private void HandlePortableTarotInventoryStateMessage(PortableTarotInventoryStateMessage message, ulong senderId)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		if (senderId == _localPlayerId || (int)_gameService.Type != 3)
		{
			return;
		}
		INetGameService gameService = _gameService;
		INetClientGameService val = (INetClientGameService)(object)((gameService is INetClientGameService) ? gameService : null);
		if (val == null || val.NetClient == null || senderId != val.NetClient.HostNetId)
		{
			return;
		}
		try
		{
			JudgementTarotInventoryState.FromJson(JsonNode.Parse(message.Json, null) as JsonObject);
		}
		catch
		{
		}
	}
}
