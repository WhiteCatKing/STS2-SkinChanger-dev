using System;
using System.Collections;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using PengoTarot.ConfigFW;
using PengoTarot.Data;
using PengoTarot.UI;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NMerchantInventory), "Initialize")]
public static class NMerchantInventory_Initialize_Patch
{
	private static void Postfix(NMerchantInventory __instance, MerchantInventory inventory)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		if (!ConfigFloatingWindowRunData.GetTarFlag(0))
		{
			return;
		}
		MerchantTarotEntry entry = TarotEntryHolder.GetEntry(inventory);
		if (entry != null)
		{
			NMerchantTarot nMerchantTarot = new NMerchantTarot();
			((Node)nMerchantTarot).Name = StringName.op_Implicit("MerchantTarot");
			((Control)nMerchantTarot).Scale = new Vector2(0.65f, 0.65f);
			((Control)nMerchantTarot).Position = new Vector2(1520f, 678f);
			((Node)((Node)__instance).GetNode<Control>(NodePath.op_Implicit("%SlotsContainer"))).AddChild((Node)(object)nMerchantTarot, false, (InternalMode)0);
			nMerchantTarot.Initialize(__instance);
			nMerchantTarot.FillSlot(entry);
			Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%MerchantCardRemoval"));
			if (nodeOrNull != null)
			{
				((Control)nMerchantTarot).FocusNeighborLeft = ((Node)nMerchantTarot).GetPathTo((Node)(object)nodeOrNull, false);
				nodeOrNull.FocusNeighborRight = ((Node)nodeOrNull).GetPathTo((Node)(object)nMerchantTarot, false);
			}
			NMerchantSlot val = null;
			Control nodeOrNull2 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%CharacterCards"));
			if (nodeOrNull2 != null && ((Node)nodeOrNull2).GetChildCount(false) > 0)
			{
				val = ((IEnumerable)((Node)nodeOrNull2).GetChildren(false)).OfType<NMerchantSlot>().LastOrDefault((Func<NMerchantSlot, bool>)((NMerchantSlot s) => ((CanvasItem)s).Visible));
			}
			if (val == null)
			{
				Control nodeOrNull3 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%ColorlessCards"));
				if (nodeOrNull3 != null && ((Node)nodeOrNull3).GetChildCount(false) > 0)
				{
					val = ((IEnumerable)((Node)nodeOrNull3).GetChildren(false)).OfType<NMerchantSlot>().LastOrDefault((Func<NMerchantSlot, bool>)((NMerchantSlot s) => ((CanvasItem)s).Visible));
				}
			}
			if (val != null)
			{
				((Control)nMerchantTarot).FocusNeighborTop = ((Node)nMerchantTarot).GetPathTo((Node)(object)val, false);
			}
			else
			{
				((Control)nMerchantTarot).FocusNeighborTop = NodePath.op_Implicit(".");
			}
			((Control)nMerchantTarot).FocusNeighborBottom = NodePath.op_Implicit(".");
			((Control)nMerchantTarot).FocusNeighborRight = NodePath.op_Implicit(".");
		}
		string[] array = new string[3] { "%ColorlessCards", "%Relics", "%Potions" };
		foreach (string text in array)
		{
			Control nodeOrNull4 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit(text));
			if (nodeOrNull4 != null)
			{
				nodeOrNull4.Position = new Vector2(nodeOrNull4.Position.X - 80f, nodeOrNull4.Position.Y);
			}
		}
		Control nodeOrNull5 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%MerchantCardRemoval"));
		if (nodeOrNull5 != null)
		{
			nodeOrNull5.Position = new Vector2(nodeOrNull5.Position.X - 80f, nodeOrNull5.Position.Y);
		}
	}
}
