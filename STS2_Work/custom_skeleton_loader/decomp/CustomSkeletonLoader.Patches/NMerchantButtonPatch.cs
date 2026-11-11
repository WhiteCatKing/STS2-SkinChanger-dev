using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NMerchantButton))]
internal static class NMerchantButtonPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPrefix]
	internal static bool Prefix(NMerchantButton __instance)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Node nodeOrNull = ((Node)__instance).GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual"));
			if (nodeOrNull == null)
			{
				return true;
			}
			string skeletonName = SpineHelper.GetSkeletonName(nodeOrNull);
			if (string.IsNullOrEmpty(skeletonName))
			{
				return true;
			}
			MegaSkeletonDataResource val = SkeletonLoader.TryBuildMegaSkeletonDataResource(skeletonName);
			if (val == null)
			{
				return true;
			}
			ModLogger.Debug("[CustomSkeletonLoader] NMerchantButton intercepting: " + skeletonName);
			((GodotObject)nodeOrNull).Call(StringName.op_Implicit("set_skeleton_data_res"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(((MegaSpineBinding)val).BoundObject) });
			ModLogger.Debug("[CustomSkeletonLoader] NMerchantButton replaced: " + skeletonName);
		}
		catch (Exception ex)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NMerchantButton patch error: " + ex.Message);
		}
		return true;
	}
}
