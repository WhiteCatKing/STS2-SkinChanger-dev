using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NRun), "_Ready")]
internal static class WearTimerPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRun __instance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		try
		{
			Node globalUi = (Node)(object)__instance.GlobalUi;
			if (globalUi == null)
			{
				return;
			}
			string text = "JoanofArcIroncladHpwearTimer";
			if (globalUi.HasNode(new NodePath(text)))
			{
				return;
			}
			Timer val = new Timer
			{
				Name = StringName.op_Implicit(text),
				WaitTime = 0.15,
				OneShot = false,
				Autostart = true,
				ProcessCallback = (TimerProcessCallback)1,
				ProcessMode = (ProcessModeEnum)3
			};
			val.Timeout += HpWear.Tick;
			globalUi.AddChild((Node)(object)val, false, (InternalMode)0);
			val.Start(-1.0);
			Timer val2 = new Timer
			{
				Name = StringName.op_Implicit("JoanofArcIroncladHpwearSafetyTimer"),
				WaitTime = 0.03,
				OneShot = false,
				Autostart = true,
				ProcessCallback = (TimerProcessCallback)1,
				ProcessMode = (ProcessModeEnum)3
			};
			val2.Timeout += delegate
			{
				if (SkinSelectionGate.SceneAllowed())
				{
					WearSettingsRuntime.TickHotkey();
				}
			};
			globalUi.AddChild((Node)(object)val2, false, (InternalMode)0);
			val2.Start(-1.0);
			ModEntry.Log.Info("wear timer attached.", 1);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"wear timer attach failed: {value}", 1);
		}
	}
}
