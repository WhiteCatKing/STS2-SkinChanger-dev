using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public static class StarXReflectionHelper
{
	private static readonly FieldInfo _baseStarCostField = AccessTools.Field(typeof(CardModel), "_baseStarCost");

	private static readonly FieldInfo _starCostSetField = AccessTools.Field(typeof(CardModel), "_starCostSet");

	private static readonly FieldInfo _starCostChangedField = AccessTools.Field(typeof(CardModel), "StarCostChanged");

	public static void SetStarCost(CardModel card, int cost)
	{
		((AbstractModel)card).AssertMutable();
		_baseStarCostField.SetValue(card, cost);
		_starCostSetField.SetValue(card, true);
		((Action)_starCostChangedField.GetValue(card))?.Invoke();
	}
}
