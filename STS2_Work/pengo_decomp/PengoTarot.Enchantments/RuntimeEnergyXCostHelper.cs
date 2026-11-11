using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public static class RuntimeEnergyXCostHelper
{
	private static readonly FieldInfo CostsXField = AccessTools.Field(typeof(CardEnergyCost), "<CostsX>k__BackingField");

	private static readonly FieldInfo BaseField = AccessTools.Field(typeof(CardEnergyCost), "_base");

	private static readonly FieldInfo CardField = AccessTools.Field(typeof(CardEnergyCost), "_card");

	private static readonly MethodInfo InvokeEnergyCostChangedMethod = AccessTools.Method(typeof(CardModel), "InvokeEnergyCostChanged", (Type[])null, (Type[])null);

	public static void SetCostsX(CardEnergyCost cost, bool value)
	{
		object? value2 = CardField.GetValue(cost);
		CardModel val = (CardModel)((value2 is CardModel) ? value2 : null);
		if (val != null)
		{
			((AbstractModel)val).AssertMutable();
			CostsXField.SetValue(cost, value);
			if (value)
			{
				BaseField.SetValue(cost, 0);
			}
			else
			{
				BaseField.SetValue(cost, cost.Canonical);
			}
			InvokeEnergyCostChangedMethod.Invoke(val, null);
		}
	}
}
