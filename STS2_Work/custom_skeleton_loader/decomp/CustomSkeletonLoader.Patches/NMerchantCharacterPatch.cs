using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NMerchantCharacter))]
internal static class NMerchantCharacterPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPrefix]
	internal static bool Prefix(NMerchantCharacter __instance)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((Node)__instance).GetChildCount(false) == 0)
			{
				return true;
			}
			Node child = ((Node)__instance).GetChild(0, false);
			if (child == null || ((GodotObject)child).GetClass() != "SpineSprite")
			{
				return true;
			}
			string skeletonName = SpineHelper.GetSkeletonName(child);
			if (string.IsNullOrEmpty(skeletonName))
			{
				return true;
			}
			MegaSkeletonDataResource val = SkeletonLoader.TryBuildMegaSkeletonDataResource(skeletonName);
			if (val == null)
			{
				return true;
			}
			ModLogger.Debug("[CustomSkeletonLoader] NMerchantCharacter intercepting: " + skeletonName);
			((GodotObject)child).Call(StringName.op_Implicit("set_skeleton_data_res"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(((MegaSpineBinding)val).BoundObject) });
			SpineHelper.PlayFirstAvailableAnimation(child);
			ModLogger.Debug("[CustomSkeletonLoader] NMerchantCharacter replaced and animation started: " + skeletonName);
			if (GodotObject.IsInstanceValid((GodotObject)(object)__instance) && ((Node)__instance).IsInsideTree() && !((GodotObject)__instance).IsQueuedForDeletion())
			{
				NMerchantCharacter instanceRef = __instance;
				Callable val2 = Callable.From((Action)delegate
				{
					//IL_003f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0044: Unknown result type (might be due to invalid IL or missing references)
					//IL_004c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					//IL_0056: Unknown result type (might be due to invalid IL or missing references)
					try
					{
						if (GodotObject.IsInstanceValid((GodotObject)(object)instanceRef) && ((Node)instanceRef).IsInsideTree())
						{
							((GodotObject)instanceRef).Call(StringName.op_Implicit("PlayAnimation"), (Variant[])(object)new Variant[2]
							{
								Variant.op_Implicit("relaxed_loop"),
								Variant.op_Implicit(true)
							});
							ModLogger.Debug("[CustomSkeletonLoader] NMerchantCharacter: deferred animation replay completed");
						}
					}
					catch (Exception ex)
					{
						ModLogger.Warn("[CustomSkeletonLoader] NMerchantCharacter deferred animation error: " + ex.Message);
					}
				});
				((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
				ModLogger.Debug("[CustomSkeletonLoader] NMerchantCharacter animation replay scheduled");
			}
		}
		catch (Exception ex2)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NMerchantCharacter patch error: " + ex2.Message);
		}
		return true;
	}
}
