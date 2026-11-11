using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Network;

public struct PortableTarotInventoryStateMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public string Json;

	public required RunLocation Location { get; set; }

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => true;

	public RunLocation location => Location;

	public void Serialize(PacketWriter writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteString(Json ?? "{}");
		writer.Write<RunLocation>(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Json = reader.ReadString();
		Location = reader.Read<RunLocation>();
	}
}
