using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace PengoTarot.Data.Judgement;

public static class JudgementTarotInventoryState
{
	public const int Capacity = 4;

	private static readonly Dictionary<ulong, List<string>> Inventories = new Dictionary<ulong, List<string>>();

	private static readonly Dictionary<ulong, uint> Revisions = new Dictionary<ulong, uint>();

	private static readonly object Sync = new object();

	public static event Action<ulong>? Changed;

	public static IReadOnlyList<string> GetTarotIds(ulong playerNetId)
	{
		lock (Sync)
		{
			List<string> value;
			return Inventories.TryGetValue(playerNetId, out value) ? value.ToArray() : Array.Empty<string>();
		}
	}

	public static bool HasOpenSlot(ulong playerNetId)
	{
		lock (Sync)
		{
			List<string> value;
			return !Inventories.TryGetValue(playerNetId, out value) || value.Count < 4;
		}
	}

	public static bool TryGetSlot(ulong playerNetId, int slotIndex, out string tarotId, out uint revision)
	{
		lock (Sync)
		{
			revision = Revisions.GetValueOrDefault(playerNetId);
			if (Inventories.TryGetValue(playerNetId, out List<string> value) && slotIndex >= 0 && slotIndex < value.Count)
			{
				tarotId = value[slotIndex];
				return true;
			}
			tarotId = string.Empty;
			return false;
		}
	}

	public static bool TryProcure(ulong playerNetId, string tarotId)
	{
		if (!TarotDeck.All.Any((TarotDef def) => def.Id == tarotId))
		{
			return false;
		}
		lock (Sync)
		{
			if (!Inventories.TryGetValue(playerNetId, out List<string> value))
			{
				value = (Inventories[playerNetId] = new List<string>(4));
			}
			if (value.Count >= 4)
			{
				return false;
			}
			value.Add(tarotId);
			Revisions[playerNetId] = Revisions.GetValueOrDefault(playerNetId) + 1;
		}
		JudgementTarotInventoryState.Changed?.Invoke(playerNetId);
		PortableTarotInventoryUi.RefreshLocal(playerNetId);
		return true;
	}

	public static bool RollbackLast(ulong playerNetId, string tarotId)
	{
		bool flag = false;
		lock (Sync)
		{
			if (Inventories.TryGetValue(playerNetId, out List<string> value) && value.Count > 0)
			{
				List<string> list = value;
				if (list[list.Count - 1] == tarotId)
				{
					value.RemoveAt(value.Count - 1);
					if (value.Count == 0)
					{
						Inventories.Remove(playerNetId);
					}
					Revisions[playerNetId] = Revisions.GetValueOrDefault(playerNetId) + 1;
					flag = true;
				}
			}
		}
		if (flag)
		{
			JudgementTarotInventoryState.Changed?.Invoke(playerNetId);
			PortableTarotInventoryUi.RefreshLocal(playerNetId);
		}
		return flag;
	}

	public static bool TryDiscard(ulong playerNetId, int slotIndex, string expectedTarotId, uint expectedRevision)
	{
		bool flag = false;
		lock (Sync)
		{
			if (Inventories.TryGetValue(playerNetId, out List<string> value) && Revisions.GetValueOrDefault(playerNetId) == expectedRevision && slotIndex >= 0 && slotIndex < value.Count && value[slotIndex] == expectedTarotId)
			{
				value.RemoveAt(slotIndex);
				if (value.Count == 0)
				{
					Inventories.Remove(playerNetId);
				}
				Revisions[playerNetId] = expectedRevision + 1;
				flag = true;
			}
		}
		if (flag)
		{
			JudgementTarotInventoryState.Changed?.Invoke(playerNetId);
			PortableTarotInventoryUi.RefreshLocal(playerNetId);
		}
		return flag;
	}

	public static void Reset()
	{
		lock (Sync)
		{
			Inventories.Clear();
			Revisions.Clear();
		}
		PortableTarotInventoryUi.RefreshLocal(null);
	}

	public static JsonObject ToJson()
	{
		JsonObject jsonObject = new JsonObject((JsonNodeOptions?)null);
		lock (Sync)
		{
			foreach (KeyValuePair<ulong, List<string>> inventory in Inventories)
			{
				JsonArray jsonArray = new JsonArray((JsonNodeOptions?)null);
				foreach (string item in inventory.Value.Take(4))
				{
					jsonArray.Add(item);
				}
				jsonObject[inventory.Key.ToString()] = new JsonObject((JsonNodeOptions?)null)
				{
					["revision"] = Revisions.GetValueOrDefault(inventory.Key),
					["cards"] = jsonArray
				};
			}
			return jsonObject;
		}
	}

	public static void FromJson(JsonObject? obj)
	{
		lock (Sync)
		{
			Inventories.Clear();
			Revisions.Clear();
			if (obj != null)
			{
				foreach (KeyValuePair<string, JsonNode> item in obj)
				{
					if (!ulong.TryParse(item.Key, out var result))
					{
						continue;
					}
					uint value = 0u;
					JsonArray jsonArray;
					if (item.Value is JsonObject jsonObject)
					{
						jsonArray = jsonObject["cards"] as JsonArray;
						try
						{
							value = jsonObject["revision"]?.GetValue<uint>() ?? 0;
						}
						catch
						{
						}
					}
					else
					{
						jsonArray = item.Value as JsonArray;
					}
					if (jsonArray == null)
					{
						continue;
					}
					List<string> list = new List<string>(4);
					foreach (JsonNode item2 in jsonArray)
					{
						string id = null;
						try
						{
							id = item2?.GetValue<string>();
						}
						catch
						{
						}
						if (id != null && TarotDeck.All.Any((TarotDef def) => def.Id == id))
						{
							list.Add(id);
						}
						if (list.Count == 4)
						{
							break;
						}
					}
					if (list.Count > 0)
					{
						Inventories[result] = list;
						Revisions[result] = value;
					}
				}
			}
		}
		PortableTarotInventoryUi.RefreshLocal(null);
	}
}
