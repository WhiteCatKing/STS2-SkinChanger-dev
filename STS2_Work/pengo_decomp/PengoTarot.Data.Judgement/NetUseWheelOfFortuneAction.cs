using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.Data.Judgement;

public struct NetUseWheelOfFortuneAction : INetAction, IPacketSerializable
{
	public int Slot;

	public string Id;

	public uint Revision;

	public int Context;

	public void Serialize(PacketWriter w)
	{
		w.WriteInt(Slot, 32);
		w.WriteString(Id);
		w.WriteUInt(Revision, 32);
		w.WriteInt(Context, 32);
	}

	public void Deserialize(PacketReader r)
	{
		Slot = r.ReadInt(32);
		Id = r.ReadString();
		Revision = r.ReadUInt(32);
		Context = r.ReadInt(32);
	}

	public GameAction ToGameAction(Player player)
	{
		return (GameAction)(object)new UseWheelOfFortuneAction(player, Slot, Id, Revision, (PortableTarotUseContext)Context);
	}
}
