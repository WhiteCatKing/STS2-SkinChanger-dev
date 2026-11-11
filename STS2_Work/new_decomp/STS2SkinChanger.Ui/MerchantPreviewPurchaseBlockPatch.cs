using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantSlot), "OnSelected")]
internal static class MerchantPreviewPurchaseBlockPatch
{
	[HarmonyPrefix]
	private static bool Prefix(NMerchantSlot __instance, ref Task __result)
	{
		if (!IsPreviewDescendant((Node)(object)__instance))
		{
			return true;
		}
		__result = Task.CompletedTask;
		return false;
	}

	private static bool IsPreviewDescendant(Node node)
	{
		for (Node val = node; val != null; val = val.GetParent())
		{
			if (MerchantRuntimeAppearance.IsMerchantPreviewRoot(val))
			{
				return true;
			}
		}
		return false;
	}
}
