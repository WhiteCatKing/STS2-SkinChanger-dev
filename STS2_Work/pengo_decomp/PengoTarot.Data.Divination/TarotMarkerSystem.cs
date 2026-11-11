using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Data.Divination;

public static class TarotMarkerSystem
{
	public readonly record struct MarkerConfig(int FlagIndex, MapPointType TargetType, int CountPerAct, int MinRow, bool MarkAll = false)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("FlagIndex = ");
			builder.Append(FlagIndex.ToString());
			builder.Append(", TargetType = ");
			MapPointType targetType = TargetType;
			builder.Append(((object)(MapPointType)(ref targetType)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", CountPerAct = ");
			builder.Append(CountPerAct.ToString());
			builder.Append(", MinRow = ");
			builder.Append(MinRow.ToString());
			builder.Append(", MarkAll = ");
			builder.Append(MarkAll.ToString());
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out int FlagIndex, out MapPointType TargetType, out int CountPerAct, out int MinRow, out bool MarkAll)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected I4, but got Unknown
			FlagIndex = this.FlagIndex;
			TargetType = (MapPointType)(int)this.TargetType;
			CountPerAct = this.CountPerAct;
			MinRow = this.MinRow;
			MarkAll = this.MarkAll;
		}
	}

	public sealed class MarkState
	{
		public int ActIndex = -1;

		public List<MapCoord> Coords = new List<MapCoord>();

		public int CompletedCount;

		public int RewardsAwarded;

		public bool Expired;
	}

	public const int LoversFlagIndex = 6;

	public const int RewardInterval = 2;

	public static readonly MarkerConfig[] Configs = new MarkerConfig[7]
	{
		new MarkerConfig(6, (MapPointType)6, 0, 0, MarkAll: true),
		new MarkerConfig(7, (MapPointType)6, 1, 0),
		new MarkerConfig(8, (MapPointType)6, 1, 0),
		new MarkerConfig(9, (MapPointType)6, 1, 0),
		new MarkerConfig(11, (MapPointType)5, 3, 5),
		new MarkerConfig(12, (MapPointType)5, 3, 5),
		new MarkerConfig(13, (MapPointType)5, 3, 0)
	};

	private static readonly Dictionary<int, MarkState> _states = new Dictionary<int, MarkState>();

	private static readonly string[] IconPaths = new string[22]
	{
		"", "", "", "", "", "", "res://images/enchantments/tar_lovers_upright_enchantment.png", "res://images/enchantments/tar_chariot_upright_enchantment.png", "res://images/enchantments/tar_strength_upright_enchantment.png", "res://images/enchantments/tar_hermit_upright_enchantment.png",
		"", "res://images/enchantments/tar_justice_upright_enchantment.png", "res://images/enchantments/tar_hanged_man_upright_enchantment.png", "res://images/enchantments/tar_death_upright_enchantment.png", "", "", "", "", "", "",
		"", ""
	};

	public static List<int> GetMarkedFlagsAt(MapCoord coord, int actIndex)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig markerConfig = configs[i];
			if (markerConfig.FlagIndex >= 0 && IsFlagEnabled(markerConfig.FlagIndex) && _states.TryGetValue(markerConfig.FlagIndex, out MarkState value) && value.ActIndex == actIndex && value.Coords.Contains(coord))
			{
				list.Add(markerConfig.FlagIndex);
			}
		}
		return list;
	}

	public static List<int> GetDisplayedFlagsAt(MapCoord coord)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		List<int> list = new List<int>();
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig markerConfig = configs[i];
			if (markerConfig.FlagIndex >= 0 && ConfigFloatingWindowRunData.GetTarFlag(markerConfig.FlagIndex) && _states.TryGetValue(markerConfig.FlagIndex, out MarkState value) && value.ActIndex >= 0 && value.Coords.Contains(coord))
			{
				list.Add(markerConfig.FlagIndex);
			}
		}
		return list;
	}

	public static bool IsMarkedAt(MapCoord coord, int actIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return GetMarkedFlagsAt(coord, actIndex).Count > 0;
	}

	public static bool IsFlagEnabled(int flagIndex)
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(flagIndex))
		{
			return !IsExpired(flagIndex);
		}
		return false;
	}

	public static bool IsExpired(int flagIndex)
	{
		if (_states.TryGetValue(flagIndex, out MarkState value))
		{
			return value.Expired;
		}
		return false;
	}

	public static IReadOnlyList<MapCoord> GetMarkedCoords(int flagIndex)
	{
		if (!IsFlagEnabled(flagIndex))
		{
			return Array.Empty<MapCoord>();
		}
		if (!_states.TryGetValue(flagIndex, out MarkState value))
		{
			return Array.Empty<MapCoord>();
		}
		return value.Coords;
	}

	public static int GetCompletedCount(int flagIndex)
	{
		if (!_states.TryGetValue(flagIndex, out MarkState value))
		{
			return 0;
		}
		return value.CompletedCount;
	}

	public static int GetProgressForDisplay(int flagIndex)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		int completedCount = GetCompletedCount(flagIndex);
		bool flag = false;
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig markerConfig = configs[i];
			if (markerConfig.FlagIndex == flagIndex)
			{
				flag = (int)markerConfig.TargetType == 6;
				break;
			}
		}
		if (!flag)
		{
			return completedCount % 2;
		}
		return completedCount;
	}

	public static void RecordCompletion(int flagIndex)
	{
		if (_states.TryGetValue(flagIndex, out MarkState value))
		{
			value.CompletedCount++;
		}
	}

	public static void Expire(int flagIndex)
	{
		if (_states.TryGetValue(flagIndex, out MarkState value))
		{
			value.Expired = true;
		}
	}

	public static void OnMarkedCombatVictory(MapCoord coord, int actIndex, CombatRoom room, IReadOnlyList<Player> players)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig cfg = configs[i];
			if (cfg.FlagIndex >= 0 && cfg.FlagIndex != 6 && _states.TryGetValue(cfg.FlagIndex, out MarkState value) && value.ActIndex == actIndex && value.Coords.Contains(coord) && IsFlagEnabled(cfg.FlagIndex))
			{
				int completedCount = value.CompletedCount;
				int rewardsAwarded = value.RewardsAwarded;
				value.CompletedCount++;
				AwardTarotReward(cfg, value, room, players);
				Log.Info($"[PengoTarot] [DivinationReward] peer={RunManager.Instance.NetService.Type} flag={cfg.FlagIndex} act={actIndex} coord={coord} count={completedCount}->{value.CompletedCount} awarded={rewardsAwarded}->{value.RewardsAwarded} rewardAdded={value.RewardsAwarded > rewardsAwarded}", 2);
			}
		}
	}

	private static void AwardTarotReward(MarkerConfig cfg, MarkState st, CombatRoom room, IReadOnlyList<Player> players)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)cfg.TargetType == 6)
		{
			if (st.CompletedCount >= 2 && st.RewardsAwarded == 0)
			{
				st.RewardsAwarded = 1;
				st.Expired = true;
				AddTarotReward(room, players, cfg.FlagIndex);
			}
		}
		else
		{
			int num = st.CompletedCount / 2;
			if (num > st.RewardsAwarded)
			{
				st.RewardsAwarded = num;
				AddTarotReward(room, players, cfg.FlagIndex);
			}
		}
	}

	private static void AddTarotReward(CombatRoom room, IReadOnlyList<Player> players, int flagIndex)
	{
		foreach (Player player in players)
		{
			room.AddExtraReward(player, (Reward)(object)new TarotReward(player, flagIndex));
		}
	}

	public static void ApplyMarkers(IRunState runState, ActMap map, int actIndex)
	{
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig cfg = configs[i];
			if (cfg.FlagIndex >= 0 && ConfigFloatingWindowRunData.GetTarFlag(cfg.FlagIndex))
			{
				if (!_states.TryGetValue(cfg.FlagIndex, out MarkState value))
				{
					value = new MarkState();
					_states[cfg.FlagIndex] = value;
				}
				if (value.ActIndex != actIndex || !((IEnumerable<MapCoord>)value.Coords).All((Func<MapCoord, bool>)map.HasPoint))
				{
					value.ActIndex = actIndex;
					value.Coords = PickCoords(map, actIndex, cfg);
				}
			}
		}
		ModInitializer.TarotSync?.BroadcastMarkerState();
	}

	private static List<MapCoord> PickCoords(ActMap map, int actIndex, MarkerConfig cfg)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		List<MapCoord> list = (from c in (from p in map.GetAllMapPoints()
				where p.PointType == cfg.TargetType && p.coord.row >= cfg.MinRow
				select p.coord).Distinct()
			orderby c.col, c.row
			select c).ToList();
		bool flag = (int)cfg.TargetType == 6 && ConfigFloatingWindowRunData.GetTarFlag(6);
		int num = ((cfg.MarkAll || flag) ? list.Count : Math.Min(cfg.CountPerAct, list.Count));
		if (num <= 0 || list.Count == 0)
		{
			return new List<MapCoord>();
		}
		Random random = new Random((actIndex + 1) * 1000 + cfg.FlagIndex * 17);
		List<MapCoord> list2 = new List<MapCoord>(list);
		List<MapCoord> list3 = new List<MapCoord>(num);
		for (int i = 0; i < num; i++)
		{
			if (list2.Count <= 0)
			{
				break;
			}
			int index = random.Next(list2.Count);
			list3.Add(list2[index]);
			list2.RemoveAt(index);
		}
		return list3;
	}

	public static void Reset()
	{
		_states.Clear();
	}

	public static string GetMarkerIconPath(int flagIndex)
	{
		if (flagIndex < 0 || flagIndex >= IconPaths.Length)
		{
			return string.Empty;
		}
		return IconPaths[flagIndex];
	}

	public static string GetReversedMarkerIconPath(int flagIndex)
	{
		string markerIconPath = GetMarkerIconPath(flagIndex);
		if (!string.IsNullOrEmpty(markerIconPath))
		{
			return markerIconPath.Replace("_upright_", "_reversed_");
		}
		return string.Empty;
	}

	public static JsonObject ToJson()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		JsonObject jsonObject = new JsonObject((JsonNodeOptions?)null);
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig markerConfig = configs[i];
			if (!_states.TryGetValue(markerConfig.FlagIndex, out MarkState value))
			{
				continue;
			}
			JsonArray jsonArray = new JsonArray((JsonNodeOptions?)null);
			foreach (MapCoord coord in value.Coords)
			{
				jsonArray.Add(new JsonObject((JsonNodeOptions?)null)
				{
					["c"] = coord.col,
					["r"] = coord.row
				});
			}
			jsonObject[markerConfig.FlagIndex.ToString()] = new JsonObject((JsonNodeOptions?)null)
			{
				["act"] = value.ActIndex,
				["coords"] = jsonArray,
				["done"] = value.CompletedCount,
				["awarded"] = value.RewardsAwarded,
				["exp"] = value.Expired
			};
		}
		return jsonObject;
	}

	public static void FromJson(JsonObject? obj)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		_states.Clear();
		if (obj == null)
		{
			return;
		}
		MarkerConfig[] configs = Configs;
		for (int i = 0; i < configs.Length; i++)
		{
			MarkerConfig markerConfig = configs[i];
			if (!(obj[markerConfig.FlagIndex.ToString()] is JsonObject jsonObject))
			{
				continue;
			}
			MarkState markState = new MarkState
			{
				ActIndex = (jsonObject["act"]?.GetValue<int>() ?? (-1)),
				CompletedCount = (jsonObject["done"]?.GetValue<int>() ?? 0),
				RewardsAwarded = (jsonObject["awarded"]?.GetValue<int>() ?? 0),
				Expired = (jsonObject["exp"]?.GetValue<bool>() ?? false)
			};
			if (jsonObject["coords"] is JsonArray jsonArray)
			{
				foreach (JsonNode item in jsonArray)
				{
					if (!(item is JsonObject jsonObject2))
					{
						continue;
					}
					JsonNode jsonNode = jsonObject2["c"];
					if (jsonNode != null)
					{
						JsonNode jsonNode2 = jsonObject2["r"];
						if (jsonNode2 != null)
						{
							markState.Coords.Add(new MapCoord(jsonNode.GetValue<int>(), jsonNode2.GetValue<int>()));
						}
					}
				}
			}
			_states[markerConfig.FlagIndex] = markState;
		}
	}
}
