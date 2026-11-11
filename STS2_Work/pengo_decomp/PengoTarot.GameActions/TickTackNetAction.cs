using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace PengoTarot.GameActions;

public struct TickTackNetAction : INetAction, IPacketSerializable
{
	public ulong TargetPlayerNetId;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteULong(TargetPlayerNetId, 64);
	}

	public void Deserialize(PacketReader reader)
	{
		TargetPlayerNetId = reader.ReadULong(64);
	}

	public GameAction ToGameAction(Player submitter)
	{
		return (GameAction)(object)new TickTackGameAction(LookupPlayer(submitter, TargetPlayerNetId));
	}

	private static Player LookupPlayer(Player anyPlayer, ulong targetNetId)
	{
		Creature creature = anyPlayer.Creature;
		ICombatState obj = ((creature != null) ? creature.CombatState : null);
		return ((obj != null) ? obj.GetPlayer(targetNetId) : null) ?? anyPlayer;
	}

	public override string ToString()
	{
		return $"TickTackNetAction targetNetId={TargetPlayerNetId}";
	}
}
