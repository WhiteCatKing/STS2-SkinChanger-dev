using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using PengoTarot.Data.Divination;
using PengoTarot.Data.Judgement;

namespace PengoTarot.ConfigFW;

public static class ConfigFloatingWindowRunData
{
	public const string SaveFieldName = "_pengotarot_cfw";

	public const int CurrentVersion = 2;

	private static bool _tarotEnabled;

	private static bool _planetEnabled;

	private static int _tarotPriceMin;

	private static int _tarotPriceMax;

	private static int _tarotPriceOffset;

	private static readonly Dictionary<ulong, int> _wheelOfFortuneCardCounts;

	private static readonly object _wheelOfFortuneCardCountsLock;

	private static readonly bool[] _flags;

	private static JsonObject? _saveConfig;

	public static bool TarotEnabled => _tarotEnabled;

	public static bool PlanetEnabled => _planetEnabled;

	public static int TarotPriceMin => _tarotPriceMin;

	public static int TarotPriceMax => _tarotPriceMax;

	public static int TarotPriceOffset => _tarotPriceOffset;

	public static JsonObject? SaveConfig => _saveConfig;

	static ConfigFloatingWindowRunData()
	{
		_tarotEnabled = true;
		_planetEnabled = true;
		_tarotPriceMin = 175;
		_tarotPriceMax = 200;
		_wheelOfFortuneCardCounts = new Dictionary<ulong, int>();
		_wheelOfFortuneCardCountsLock = new object();
		_flags = CreateDefaultFlags();
		SnapshotFromDefaults();
	}

	private static bool[] CreateDefaultFlags()
	{
		bool[] array = new bool[22];
		for (int i = 0; i < 6 && i < array.Length; i++)
		{
			array[i] = true;
		}
		return array;
	}

	public static int GetWheelOfFortuneCardCount(ulong playerNetId)
	{
		lock (_wheelOfFortuneCardCountsLock)
		{
			int value;
			return (!_wheelOfFortuneCardCounts.TryGetValue(playerNetId, out value)) ? 1 : value;
		}
	}

	public static bool GetTarFlag(int index)
	{
		if (TarotEnabled && index >= 0 && index < _flags.Length)
		{
			return _flags[index];
		}
		return false;
	}

	public static void SnapshotFromDefaults()
	{
		_tarotEnabled = ConfigFloatingWindowConfig.TarotEnabled;
		_planetEnabled = ConfigFloatingWindowConfig.PlanetEnabled;
		_tarotPriceMin = Math.Max(0, ConfigFloatingWindowConfig.TarotBasePriceMin);
		_tarotPriceMax = Math.Max(0, ConfigFloatingWindowConfig.TarotBasePriceMax);
		_tarotPriceOffset = 0;
		ClearWheelOfFortuneCardCounts();
		JudgementTarotInventoryState.Reset();
		for (int i = 0; i < _flags.Length; i++)
		{
			_flags[i] = ConfigFloatingWindowConfig.GetDifficultyFlag(i);
		}
		TarotMarkerSystem.Reset();
	}

	public static void Reset()
	{
		_tarotEnabled = true;
		_planetEnabled = true;
		_tarotPriceMin = 175;
		_tarotPriceMax = 200;
		_tarotPriceOffset = 0;
		ClearWheelOfFortuneCardCounts();
		JudgementTarotInventoryState.Reset();
		Array.Clear(_flags);
		for (int i = 0; i < 6 && i < _flags.Length; i++)
		{
			_flags[i] = true;
		}
		TarotMarkerSystem.Reset();
	}

	public static void Apply(bool tarot, bool planet, int priceMin, int priceMax, bool[]? flags)
	{
		_tarotEnabled = tarot;
		_planetEnabled = planet;
		_tarotPriceMin = Math.Max(0, priceMin);
		_tarotPriceMax = Math.Max(0, priceMax);
		_tarotPriceOffset = 0;
		ClearWheelOfFortuneCardCounts();
		if (flags != null)
		{
			for (int i = 0; i < _flags.Length; i++)
			{
				_flags[i] = i < flags.Length && flags[i];
			}
		}
		SetSaveConfigFromRunData();
	}

	public static void AdjustTarotPrice(int delta)
	{
		_tarotPriceOffset += delta;
	}

	public static int RecordWheelOfFortuneCard(ulong playerNetId)
	{
		lock (_wheelOfFortuneCardCountsLock)
		{
			int value;
			int num = ((!_wheelOfFortuneCardCounts.TryGetValue(playerNetId, out value)) ? 1 : value) + 1;
			_wheelOfFortuneCardCounts[playerNetId] = num;
			return num;
		}
	}

	public static JsonObject GetWheelOfFortuneCardCountsJson()
	{
		JsonObject jsonObject = new JsonObject((JsonNodeOptions?)null);
		lock (_wheelOfFortuneCardCountsLock)
		{
			foreach (KeyValuePair<ulong, int> wheelOfFortuneCardCount in _wheelOfFortuneCardCounts)
			{
				jsonObject[wheelOfFortuneCardCount.Key.ToString()] = wheelOfFortuneCardCount.Value;
			}
			return jsonObject;
		}
	}

	public static void SetWheelOfFortuneCardCounts(JsonObject? obj)
	{
		lock (_wheelOfFortuneCardCountsLock)
		{
			_wheelOfFortuneCardCounts.Clear();
			if (obj == null)
			{
				return;
			}
			foreach (KeyValuePair<string, JsonNode> item in obj)
			{
				if (ulong.TryParse(item.Key, out var result))
				{
					_wheelOfFortuneCardCounts[result] = Math.Max(1, TryGet(item.Value, 1));
				}
			}
		}
	}

	private static void ClearWheelOfFortuneCardCounts()
	{
		lock (_wheelOfFortuneCardCountsLock)
		{
			_wheelOfFortuneCardCounts.Clear();
		}
	}

	public static void SetTarotEnabled(bool value)
	{
		_tarotEnabled = value;
	}

	public static void SetPlanetEnabled(bool value)
	{
		_planetEnabled = value;
	}

	public static void SetDifficultyFlag(int index, bool value)
	{
		if (index >= 0 && index < _flags.Length)
		{
			_flags[index] = value;
		}
	}

	public static void SetSaveConfigFromRunData()
	{
		_saveConfig = BuildConfigSnapshot();
	}

	private static JsonObject BuildConfigSnapshot()
	{
		JsonArray jsonArray = new JsonArray((JsonNodeOptions?)null);
		bool[] flags = _flags;
		foreach (bool value in flags)
		{
			jsonArray.Add(value);
		}
		return new JsonObject((JsonNodeOptions?)null)
		{
			["tarot"] = _tarotEnabled,
			["planet"] = _planetEnabled,
			["pmin"] = _tarotPriceMin,
			["pmax"] = _tarotPriceMax,
			["flags"] = jsonArray
		};
	}

	public static JsonObject ToJson()
	{
		JsonObject jsonObject = _saveConfig ?? BuildConfigSnapshot();
		return new JsonObject((JsonNodeOptions?)null)
		{
			["v"] = 2,
			["cfg"] = jsonObject.DeepClone(),
			["run"] = new JsonObject((JsonNodeOptions?)null)
			{
				["poff"] = _tarotPriceOffset,
				["wheelCards"] = GetWheelOfFortuneCardCountsJson(),
				["portableTarots"] = JudgementTarotInventoryState.ToJson(),
				["markers"] = TarotMarkerSystem.ToJson()
			}
		};
	}

	public static void FromJson(JsonObject? obj)
	{
		if (obj == null)
		{
			return;
		}
		if (obj["cfg"] is JsonObject jsonObject)
		{
			_tarotEnabled = TryGet(jsonObject["tarot"], defaultValue: true);
			_planetEnabled = TryGet(jsonObject["planet"], defaultValue: true);
			_tarotPriceMin = Math.Max(0, TryGet(jsonObject["pmin"], 175));
			_tarotPriceMax = Math.Max(0, TryGet(jsonObject["pmax"], 200));
			if (jsonObject["flags"] is JsonArray jsonArray)
			{
				for (int i = 0; i < _flags.Length; i++)
				{
					_flags[i] = i < jsonArray.Count && TryGet(jsonArray[i], defaultValue: false);
				}
			}
			if (obj["run"] is JsonObject jsonObject2)
			{
				_tarotPriceOffset = TryGet(jsonObject2["poff"], 0);
				SetWheelOfFortuneCardCounts(jsonObject2["wheelCards"] as JsonObject);
				JudgementTarotInventoryState.FromJson(jsonObject2["portableTarots"] as JsonObject);
				TarotMarkerSystem.FromJson(jsonObject2["markers"] as JsonObject);
			}
			return;
		}
		_tarotEnabled = TryGet(obj["tarot"], defaultValue: true);
		_planetEnabled = TryGet(obj["planet"], defaultValue: true);
		_tarotPriceMin = Math.Max(0, TryGet(obj["pmin"], 175));
		_tarotPriceMax = Math.Max(0, TryGet(obj["pmax"], 200));
		_tarotPriceOffset = TryGet(obj["poff"], 0);
		SetWheelOfFortuneCardCounts(obj["wheelCards"] as JsonObject);
		JudgementTarotInventoryState.FromJson(obj["portableTarots"] as JsonObject);
		if (obj["flags"] is JsonArray jsonArray2)
		{
			for (int j = 0; j < _flags.Length; j++)
			{
				_flags[j] = j < jsonArray2.Count && TryGet(jsonArray2[j], defaultValue: false);
			}
		}
		TarotMarkerSystem.FromJson(obj["markers"] as JsonObject);
	}

	private static T TryGet<T>(JsonNode? node, T defaultValue) where T : struct
	{
		try
		{
			return node?.GetValue<T>() ?? defaultValue;
		}
		catch
		{
			return defaultValue;
		}
	}
}
