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
		try
		{
			Node globalUi = (Node)(object)__instance.GlobalUi;
			if (globalUi != null)
			{
				string text = "JoanofArcIroncladHpwearTimer";
				if (!globalUi.HasNode(new NodePath(text)))
				{
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
					ModEntry.Log.Info("wear timer attached.", 1);
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"wear timer attach failed: {value}", 1);
		}
	}
}
