using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace PengoTarot.BalatroEffect;

[HarmonyPatch(typeof(NCardLibrary))]
public static class GlobalDynamicPatch
{
	private static readonly HashSet<NCardLibrary> _initialized = new HashSet<NCardLibrary>();

	[HarmonyPatch("_Ready")]
	[HarmonyPostfix]
	private static void Ready_Postfix(NCardLibrary __instance)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized.Contains(__instance))
		{
			return;
		}
		_initialized.Add(__instance);
		VBoxContainer nodeOrNull = ((Node)__instance).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox"));
		NTickbox nodeOrNull2 = ((Node)__instance).GetNodeOrNull<NTickbox>(NodePath.op_Implicit("%Upgrades"));
		if (nodeOrNull == null || nodeOrNull2 == null)
		{
			return;
		}
		Node obj = ((Node)nodeOrNull2).Duplicate(15);
		Control val = (Control)(object)((obj is Control) ? obj : null);
		if (val == null)
		{
			return;
		}
		((Node)val).Name = StringName.op_Implicit("GlobalDynamicEffect");
		((Node)val).UniqueNameInOwner = true;
		((Node)nodeOrNull).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)nodeOrNull).MoveChild((Node)(object)val, ((Node)nodeOrNull2).GetIndex(false) + 1);
		Node obj2 = ((Node)val).FindChild("Label", true, false);
		Label val2 = (Label)(object)((obj2 is Label) ? obj2 : null);
		if (val2 != null)
		{
			string formattedText = new LocString("gameplay_ui", "BAL_GLOBAL_DYNAMIC_EFFECT").GetFormattedText();
			if (!string.IsNullOrEmpty(formattedText))
			{
				val2.Text = formattedText;
			}
		}
		NTickbox val3 = (NTickbox)(object)((val is NTickbox) ? val : null);
		if (val3 != null)
		{
			((GodotObject)val3).SetBlockSignals(true);
			val3.IsTicked = Config.GlobalDynamicEffect;
			((GodotObject)val3).SetBlockSignals(false);
			((GodotObject)val3).Connect(StringName.op_Implicit("Toggled"), Callable.From<NTickbox>((Action<NTickbox>)delegate(NTickbox nt)
			{
				Config.GlobalDynamicEffect = nt.IsTicked;
			}), 0u);
		}
	}
}
