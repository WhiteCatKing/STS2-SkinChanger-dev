using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.UI;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NMerchantInventory), "GetAllSlots")]
public static class NMerchantInventory_GetAllSlots_Patch
{
	private static void Postfix(ref IEnumerable<NMerchantSlot> __result, NMerchantInventory __instance)
	{
		if (!ConfigFloatingWindowRunData.GetTarFlag(0))
		{
			return;
		}
		Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%SlotsContainer"));
		if (nodeOrNull != null)
		{
			NMerchantTarot nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<NMerchantTarot>(NodePath.op_Implicit("MerchantTarot"));
			if (nodeOrNull2 != null)
			{
				__result = __result.Concat((IEnumerable<NMerchantSlot>)(object)new NMerchantTarot[1] { nodeOrNull2 });
			}
		}
	}
}
