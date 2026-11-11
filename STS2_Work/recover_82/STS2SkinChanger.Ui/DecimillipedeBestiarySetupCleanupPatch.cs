using System;
using System.Collections;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NBestiaryLayoutDecimillipede), "Setup")]
internal static class DecimillipedeBestiarySetupCleanupPatch
{
	[HarmonyPrefix]
	private static void Prefix(NBestiaryLayoutDecimillipede __instance)
	{
		try
		{
			if (AccessTools.Field(typeof(NBestiaryLayoutDecimillipede), "_creatures")?.GetValue(__instance) is IList list)
			{
				foreach (object item in list)
				{
					Node val = (Node)((item is Node) ? item : null);
					if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && !((GodotObject)val).IsQueuedForDeletion())
					{
						val.QueueFree();
					}
				}
				list.Clear();
			}
			object? obj = AccessTools.Field(typeof(NBestiaryLayoutDecimillipede), "_creatureContainer")?.GetValue(__instance);
			Node val2 = (Node)((obj is Node) ? obj : null);
			if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				return;
			}
			foreach (Node child in val2.GetChildren(false))
			{
				if (child != null)
				{
					Node val3 = child;
					if (GodotObject.IsInstanceValid((GodotObject)(object)val3) && !((GodotObject)val3).IsQueuedForDeletion())
					{
						val3.QueueFree();
					}
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("清理残杀千足虫图鉴旧段落失败：" + ex.GetBaseException().Message);
		}
	}
}
