using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Patches;

public static class ModelHoverTipCanonicalPatch
{
	[HarmonyPatch(typeof(EnchantmentModel), "get_HoverTip")]
	public static class Enchantment
	{
		[HarmonyPostfix]
		internal static void Postfix(EnchantmentModel __instance, ref HoverTip __result)
		{
			EnchantmentModel val = (EnchantmentModel)(((AbstractModel)__instance).IsCanonical ? ((object)__instance) : ((object)ModelDb.GetByIdOrNull<EnchantmentModel>(((AbstractModel)__instance).Id)));
			if (val != null)
			{
				((HoverTip)(ref __result)).SetCanonicalModel((AbstractModel)(object)val);
			}
		}
	}

	[HarmonyPatch(typeof(PowerModel), "GetDumbHoverTip")]
	public static class Power
	{
		[HarmonyPostfix]
		internal static void Postfix(PowerModel __instance, ref HoverTip __result)
		{
			PowerModel val = (PowerModel)(((AbstractModel)__instance).IsCanonical ? ((object)__instance) : ((object)ModelDb.GetByIdOrNull<PowerModel>(((AbstractModel)__instance).Id)));
			if (val != null)
			{
				((HoverTip)(ref __result)).SetCanonicalModel((AbstractModel)(object)val);
			}
		}
	}

	[HarmonyPatch(typeof(AfflictionModel), "get_HoverTip")]
	public static class Affliction
	{
		[HarmonyPostfix]
		internal static void Postfix(AfflictionModel __instance, ref HoverTip __result)
		{
			AfflictionModel val = (AfflictionModel)(((AbstractModel)__instance).IsCanonical ? ((object)__instance) : ((object)ModelDb.GetByIdOrNull<AfflictionModel>(((AbstractModel)__instance).Id)));
			if (val != null)
			{
				((HoverTip)(ref __result)).SetCanonicalModel((AbstractModel)(object)val);
			}
		}
	}
}
