using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace PengoTarot.Network;

public struct ForcePlayerSyncMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public SerializablePlayer player;

	public required RunLocation Location { get; set; }

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)2;

	public bool ShouldBuffer => false;

	public RunLocation location => Location;

	public void Serialize(PacketWriter writer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		writer.Write<SerializablePlayer>(player);
		writer.Write<RunLocation>(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		player = reader.Read<SerializablePlayer>();
		Location = reader.Read<RunLocation>();
	}
}
