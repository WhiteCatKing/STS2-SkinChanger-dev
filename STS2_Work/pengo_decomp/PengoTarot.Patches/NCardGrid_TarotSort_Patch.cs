using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NCardGrid), "SetCards")]
public static class NCardGrid_TarotSort_Patch
{
	private static readonly Dictionary<string, int> TarotIndex = new Dictionary<string, int>
	{
		{ "Fool", 0 },
		{ "Magician", 1 },
		{ "HighPriestess", 2 },
		{ "Empress", 3 },
		{ "Emperor", 4 },
		{ "Hierophant", 5 },
		{ "Lovers", 6 },
		{ "Chariot", 7 },
		{ "Strength", 8 },
		{ "Hermit", 9 },
		{ "WheelOfFortune", 10 },
		{ "Justice", 11 },
		{ "HangedMan", 12 },
		{ "Death", 13 },
		{ "Temperance", 14 },
		{ "Devil", 15 },
		{ "Tower", 16 },
		{ "Star", 17 },
		{ "Moon", 18 },
		{ "Sun", 19 },
		{ "Judgement", 20 },
		{ "World", 21 }
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
			if (!(item.Pool is TarotPool))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			List<CardModel> list = new List<CardModel>(readOnlyList);
			list.Sort(CompareTarotCards);
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

	private static int CompareTarotCards(CardModel? x, CardModel? y)
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
		if (!name.StartsWith("Tar"))
		{
			return int.MaxValue;
		}
		ReadOnlySpan<char> span = name.AsSpan(3);
		bool num = span.EndsWith("Reversed".AsSpan());
		int length = (num ? (span.Length - 8) : (span.Length - 7));
		int num2 = CollectionExtensions.GetValueOrDefault(key: span.Slice(0, length).ToString(), dictionary: TarotIndex, defaultValue: int.MaxValue);
		if (!num)
		{
			return num2;
		}
		return num2 + 100;
	}
}
