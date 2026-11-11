using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace STS2SkinChanger.Core;

internal struct SkinChangerNetMessage : INetMessage, IPacketSerializable
{
	public byte ProtocolVersion;

	public SkinSyncMessageKind Kind;

	public ulong PlayerNetId;

	public string CharacterId;

	public string GroupId;

	public string OptionId;

	public string SourceOptionManifest;

	public string ProviderId;

	public ulong WorkshopItemId;

	public string SafeResourceFingerprint;

	public string SafeResourceManifest;

	public string SafeResourceBindings;

	public string TransformManifest;

	public string OnlineFailure;

	public bool ShouldBroadcast => false;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => true;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteByte(ProtocolVersion, 8);
		writer.WriteByte((byte)Kind, 8);
		writer.WriteULong(PlayerNetId, 64);
		writer.WriteString(CharacterId ?? string.Empty);
		writer.WriteString(GroupId ?? string.Empty);
		writer.WriteString(OptionId ?? string.Empty);
		writer.WriteString(SourceOptionManifest ?? "[]");
		writer.WriteString(TransformManifest ?? string.Empty);
	}

	public void Deserialize(PacketReader reader)
	{
		ProtocolVersion = reader.ReadByte(8);
		Kind = (SkinSyncMessageKind)reader.ReadByte(8);
		PlayerNetId = reader.ReadULong(64);
		CharacterId = reader.ReadString();
		GroupId = reader.ReadString();
		OptionId = reader.ReadString();
		SourceOptionManifest = reader.ReadString();
		TransformManifest = reader.ReadString();
		ProviderId = string.Empty;
		WorkshopItemId = 0uL;
		SafeResourceFingerprint = string.Empty;
		SafeResourceManifest = string.Empty;
		SafeResourceBindings = string.Empty;
		OnlineFailure = string.Empty;
	}
}
