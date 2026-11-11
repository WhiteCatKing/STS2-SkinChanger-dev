using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SerializablePlayerOddsSet : IPacketSerializable
{
	[JsonPropertyName("card_rarity_odds_value")]
	public float CardRarityOddsValue { get; set; }

	[JsonPropertyName("potion_reward_odds_value")]
	public float PotionRewardOddsValue { get; set; }

	public void Serialize(PacketWriter writer)
	{
		writer.WriteFloat(CardRarityOddsValue, null);
		writer.WriteFloat(PotionRewardOddsValue, null);
	}

	public void Deserialize(PacketReader reader)
	{
		CardRarityOddsValue = reader.ReadFloat(null);
		PotionRewardOddsValue = reader.ReadFloat(null);
	}
}
