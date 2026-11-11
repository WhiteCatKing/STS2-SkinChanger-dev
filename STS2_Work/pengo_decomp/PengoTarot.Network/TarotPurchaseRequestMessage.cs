using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Network;

public struct TarotPurchaseRequestMessage : INetMessage, IPacketSerializable, IRunLocationTargetedMessage
{
	public int goldCost;

	public List<string>? cachedDefIds;

	public required RunLocation Location { get; set; }

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => true;

	public RunLocation location => Location;

	public void Serialize(PacketWriter writer)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteInt(goldCost, 32);
		bool flag = cachedDefIds != null;
		writer.WriteBool(flag);
		if (flag)
		{
			writer.WriteInt(cachedDefIds.Count, 8);
			foreach (string cachedDefId in cachedDefIds)
			{
				writer.WriteString(cachedDefId);
			}
		}
		writer.Write<RunLocation>(Location);
	}

	public void Deserialize(PacketReader reader)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		goldCost = reader.ReadInt(32);
		if (reader.ReadBool())
		{
			int num = reader.ReadInt(8);
			cachedDefIds = new List<string>(num);
			for (int i = 0; i < num; i++)
			{
				cachedDefIds.Add(reader.ReadString());
			}
		}
		Location = reader.Read<RunLocation>();
	}
}
