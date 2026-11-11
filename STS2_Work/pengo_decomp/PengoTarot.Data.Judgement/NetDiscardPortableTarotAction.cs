using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.Data.Judgement;

public struct NetDiscardPortableTarotAction : INetAction, IPacketSerializable
{
	public int SlotIndex;

	public string ExpectedTarotId;

	public uint ExpectedRevision;

	public bool WasEnqueuedInCombat;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(SlotIndex, 32);
		writer.WriteString(ExpectedTarotId);
		writer.WriteUInt(ExpectedRevision, 32);
		writer.WriteBool(WasEnqueuedInCombat);
	}

	public void Deserialize(PacketReader reader)
	{
		SlotIndex = reader.ReadInt(32);
		ExpectedTarotId = reader.ReadString();
		ExpectedRevision = reader.ReadUInt(32);
		WasEnqueuedInCombat = reader.ReadBool();
	}

	public GameAction ToGameAction(Player player)
	{
		return (GameAction)(object)new DiscardPortableTarotAction(player, SlotIndex, ExpectedTarotId, ExpectedRevision, WasEnqueuedInCombat);
	}
}
