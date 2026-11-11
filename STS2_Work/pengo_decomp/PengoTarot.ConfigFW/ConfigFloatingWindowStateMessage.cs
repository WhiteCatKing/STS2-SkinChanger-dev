using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace PengoTarot.ConfigFW;

public struct ConfigFloatingWindowStateMessage : INetMessage, IPacketSerializable
{
	public int version;

	public bool isOpen;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => false;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(version, 8);
		writer.WriteBool(isOpen);
	}

	public void Deserialize(PacketReader reader)
	{
		version = reader.ReadInt(8);
		isOpen = reader.ReadBool();
	}
}
