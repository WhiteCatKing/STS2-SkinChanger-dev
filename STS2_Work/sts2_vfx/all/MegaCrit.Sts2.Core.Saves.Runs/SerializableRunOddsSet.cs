using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializableRunOddsSet : IPacketSerializable
{
	[JsonPropertyName("unknown_map_point_monster_odds_value")]
	public float UnknownMapPointMonsterOddsValue { get; set; }

	[JsonPropertyName("unknown_map_point_elite_odds_value")]
	public float UnknownMapPointEliteOddsValue { get; set; }

	[JsonPropertyName("unknown_map_point_treasure_odds_value")]
	public float UnknownMapPointTreasureOddsValue { get; set; }

	[JsonPropertyName("unknown_map_point_shop_odds_value")]
	public float UnknownMapPointShopOddsValue { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteFloat(UnknownMapPointMonsterOddsValue, null);
		writer.WriteFloat(UnknownMapPointEliteOddsValue, null);
		writer.WriteFloat(UnknownMapPointTreasureOddsValue, null);
		writer.WriteFloat(UnknownMapPointShopOddsValue, null);
	}

	public void Deserialize(PacketReader reader)
	{
		UnknownMapPointMonsterOddsValue = reader.ReadFloat(null);
		UnknownMapPointEliteOddsValue = reader.ReadFloat(null);
		UnknownMapPointTreasureOddsValue = reader.ReadFloat(null);
		UnknownMapPointShopOddsValue = reader.ReadFloat(null);
	}
}
