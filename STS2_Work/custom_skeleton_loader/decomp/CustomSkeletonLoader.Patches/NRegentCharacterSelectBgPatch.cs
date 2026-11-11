using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NRegentCharacterSelectBg))]
internal static class NRegentCharacterSelectBgPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPrefix]
	internal static void Prefix(NRegentCharacterSelectBg __instance)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		try
		{
			Node nodeOrNull = ((Node)__instance).GetNodeOrNull(NodePath.op_Implicit("SpineSprite"));
			if (nodeOrNull == null)
			{
				return;
			}
			string skeletonName = SpineHelper.GetSkeletonName(nodeOrNull);
			if (!string.IsNullOrEmpty(skeletonName))
			{
				MegaSkeletonDataResource val = SkeletonLoader.TryBuildMegaSkeletonDataResource(skeletonName);
				if (val != null)
				{
					ModLogger.Debug("[CustomSkeletonLoader] NRegentCharacterSelectBg: replacing skeleton for " + skeletonName);
					MegaSprite val2 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)nodeOrNull));
					val2.SetSkeletonDataRes(val);
					SpineHelper.PlayFirstAvailableAnimation(nodeOrNull);
					ModLogger.Debug("[CustomSkeletonLoader] NRegentCharacterSelectBg: skeleton replaced and animation started for " + skeletonName);
				}
			}
		}
		catch (Exception ex)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NRegentCharacterSelectBg prefix error: " + ex.Message);
		}
	}
}
