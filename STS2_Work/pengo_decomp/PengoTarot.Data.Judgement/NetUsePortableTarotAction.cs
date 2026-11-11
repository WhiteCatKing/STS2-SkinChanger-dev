using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.Data.Judgement;

public struct NetUsePortableTarotAction : INetAction, IPacketSerializable
{
	public int SlotIndex;

	public string TarotId;

	public uint ExpectedRevision;

	public int Context;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(SlotIndex, 32);
		writer.WriteString(TarotId);
		writer.WriteUInt(ExpectedRevision, 32);
		writer.WriteInt(Context, 32);
	}

	public void Deserialize(PacketReader reader)
	{
		SlotIndex = reader.ReadInt(32);
		TarotId = reader.ReadString();
		ExpectedRevision = reader.ReadUInt(32);
		Context = reader.ReadInt(32);
	}

	public GameAction ToGameAction(Player player)
	{
		return (GameAction)(object)new UsePortableTarotAction(player, SlotIndex, TarotId, ExpectedRevision, (PortableTarotUseContext)Context);
	}
}
