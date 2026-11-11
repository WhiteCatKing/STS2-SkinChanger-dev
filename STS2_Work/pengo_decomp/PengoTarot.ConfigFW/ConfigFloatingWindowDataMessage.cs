using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using PengoTarot.Data.Divination;
using PengoTarot.Data.Judgement;

namespace PengoTarot.ConfigFW;

public struct ConfigFloatingWindowDataMessage : INetMessage, IPacketSerializable
{
	public int version;

	public bool tarot;

	public bool planet;

	public int priceMin;

	public int priceMax;

	public string wheelCardCountsJson;

	public bool[] flags;

	public string markerStateJson;

	public string portableTarotsJson;

	public bool ShouldBroadcast => true;

	public NetTransferMode Mode => (NetTransferMode)2;

	public LogLevel LogLevel => (LogLevel)0;

	public bool ShouldBuffer => false;

	public void Serialize(PacketWriter writer)
	{
		writer.WriteInt(version, 8);
		writer.WriteBool(tarot);
		writer.WriteBool(planet);
		writer.WriteInt(priceMin, 32);
		writer.WriteInt(priceMax, 32);
		writer.WriteString(wheelCardCountsJson ?? "{}");
		bool[] array = flags;
		int num = ((array != null) ? array.Length : 0);
		writer.WriteInt(num, 8);
		for (int i = 0; i < num; i++)
		{
			writer.WriteBool(flags[i]);
		}
		writer.WriteString(markerStateJson ?? "{}");
		writer.WriteString(portableTarotsJson ?? "{}");
	}

	public void Deserialize(PacketReader reader)
	{
		version = reader.ReadInt(8);
		tarot = reader.ReadBool();
		planet = reader.ReadBool();
		priceMin = reader.ReadInt(32);
		priceMax = reader.ReadInt(32);
		wheelCardCountsJson = reader.ReadString();
		int num = reader.ReadInt(8);
		flags = new bool[num];
		for (int i = 0; i < num; i++)
		{
			flags[i] = reader.ReadBool();
		}
		markerStateJson = reader.ReadString();
		portableTarotsJson = reader.ReadString();
	}

	public static ConfigFloatingWindowDataMessage FromRunData()
	{
		int num = 22;
		bool[] array = new bool[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ConfigFloatingWindowRunData.GetTarFlag(i);
		}
		ConfigFloatingWindowDataMessage result = default(ConfigFloatingWindowDataMessage);
		result.version = 2;
		result.tarot = ConfigFloatingWindowRunData.TarotEnabled;
		result.planet = ConfigFloatingWindowRunData.PlanetEnabled;
		result.priceMin = ConfigFloatingWindowRunData.TarotPriceMin;
		result.priceMax = ConfigFloatingWindowRunData.TarotPriceMax;
		result.wheelCardCountsJson = ConfigFloatingWindowRunData.GetWheelOfFortuneCardCountsJson().ToJsonString();
		result.flags = array;
		result.markerStateJson = TarotMarkerSystem.ToJson().ToJsonString();
		result.portableTarotsJson = JudgementTarotInventoryState.ToJson().ToJsonString();
		return result;
	}

	public void ApplyToRunData()
	{
		ConfigFloatingWindowRunData.Apply(tarot, planet, priceMin, priceMax, flags);
		try
		{
			ConfigFloatingWindowRunData.SetWheelOfFortuneCardCounts(JsonNode.Parse(wheelCardCountsJson, null) as JsonObject);
		}
		catch
		{
			ConfigFloatingWindowRunData.SetWheelOfFortuneCardCounts(null);
		}
		try
		{
			TarotMarkerSystem.FromJson(JsonNode.Parse(markerStateJson, null) as JsonObject);
		}
		catch
		{
		}
		try
		{
			JudgementTarotInventoryState.FromJson(JsonNode.Parse(portableTarotsJson, null) as JsonObject);
		}
		catch
		{
			JudgementTarotInventoryState.FromJson(null);
		}
	}
}
