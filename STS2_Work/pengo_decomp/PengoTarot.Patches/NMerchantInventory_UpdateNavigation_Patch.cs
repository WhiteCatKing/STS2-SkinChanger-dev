using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.UI;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NMerchantInventory), "UpdateNavigation")]
public static class NMerchantInventory_UpdateNavigation_Patch
{
	private static void Postfix(NMerchantInventory __instance)
	{
		if (!ConfigFloatingWindowRunData.GetTarFlag(0))
		{
			return;
		}
		Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%SlotsContainer"));
		if (nodeOrNull == null)
		{
			return;
		}
		NMerchantTarot nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<NMerchantTarot>(NodePath.op_Implicit("MerchantTarot"));
		if (nodeOrNull2 == null || !((CanvasItem)nodeOrNull2).Visible)
		{
			return;
		}
		NMerchantCardRemoval nodeOrNull3 = ((Node)__instance).GetNodeOrNull<NMerchantCardRemoval>(NodePath.op_Implicit("%MerchantCardRemoval"));
		if (nodeOrNull3 != null && ((CanvasItem)nodeOrNull3).Visible)
		{
			((Control)nodeOrNull2).FocusNeighborLeft = ((Node)nodeOrNull3).GetPath();
			((Control)nodeOrNull3).FocusNeighborRight = ((Node)nodeOrNull2).GetPath();
		}
		else
		{
			((Control)nodeOrNull2).FocusNeighborLeft = ((Node)nodeOrNull2).GetPath();
		}
		((Control)nodeOrNull2).FocusNeighborRight = ((Node)nodeOrNull2).GetPath();
		Control nodeOrNull4 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%CharacterCards"));
		List<NMerchantSlot> list = ((nodeOrNull4 != null) ? (from s in ((IEnumerable)((Node)nodeOrNull4).GetChildren(false)).OfType<NMerchantSlot>()
			where ((CanvasItem)s).Visible
			select s).ToList() : null) ?? new List<NMerchantSlot>();
		if (list.Count > 0)
		{
			((Control)nodeOrNull2).FocusNeighborTop = ((Node)list[0]).GetPath();
			foreach (NMerchantSlot item in list)
			{
				if (((Control)item).FocusNeighborBottom == (NodePath)null || ((Control)item).FocusNeighborBottom == ((Node)item).GetPath())
				{
					((Control)item).FocusNeighborBottom = ((Node)nodeOrNull2).GetPath();
				}
			}
		}
		else
		{
			((Control)nodeOrNull2).FocusNeighborTop = ((Node)nodeOrNull2).GetPath();
		}
		((Control)nodeOrNull2).FocusNeighborBottom = ((Node)nodeOrNull2).GetPath();
	}
}
