using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NCardGrid), "SetCards")]
public static class NCardGrid_PlanetSort_Patch
{
	private static readonly Dictionary<string, int> PlanetIndex = new Dictionary<string, int>
	{
		{ "Mercury", 0 },
		{ "Venus", 1 },
		{ "Earth", 2 },
		{ "Mars", 3 },
		{ "Jupiter", 4 },
		{ "Saturn", 5 },
		{ "Uranus", 6 },
		{ "Neptune", 7 },
		{ "Pluto", 8 },
		{ "X", 9 },
		{ "Ceres", 10 },
		{ "Eris", 11 }
	};

	private static void Prefix(NCardGrid __instance, object[] __args)
	{
		if (__args.Length < 3 || !(__args[0] is IReadOnlyList<CardModel> { Count: not 0 } readOnlyList))
		{
			return;
		}
		bool flag = true;
		foreach (CardModel item in readOnlyList)
		{
			if (!(item.Pool is PlanetPool))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			List<CardModel> list = new List<CardModel>(readOnlyList);
			list.Sort(ComparePlanetCards);
			__args[0] = list;
			object obj = __args[2];
			if (obj != null)
			{
				Type type = obj.GetType();
				type.GetMethod("Add");
				object value = Enum.Parse(type.GetGenericArguments()[0], "Ascending");
				IList list2 = (IList)Activator.CreateInstance(type);
				list2.Add(value);
				__args[2] = list2;
			}
		}
	}

	private static int ComparePlanetCards(CardModel? x, CardModel? y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		int sortKey = GetSortKey(x);
		int sortKey2 = GetSortKey(y);
		return sortKey.CompareTo(sortKey2);
	}

	private static int GetSortKey(CardModel card)
	{
		string name = ((object)card).GetType().Name;
		if (!name.StartsWith("Planet"))
		{
			return int.MaxValue;
		}
		string text = name;
		string key = text.Substring(6, text.Length - 6);
		return PlanetIndex.GetValueOrDefault(key, int.MaxValue);
	}
}
