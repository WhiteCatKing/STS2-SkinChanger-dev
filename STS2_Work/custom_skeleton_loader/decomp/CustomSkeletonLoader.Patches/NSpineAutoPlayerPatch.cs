using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Animation;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NSpineAutoPlayer))]
internal static class NSpineAutoPlayerPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPrefix]
	internal static bool Prefix(NSpineAutoPlayer __instance)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Node parent = ((Node)__instance).GetParent();
			if (parent == null || !parent.IsInsideTree())
			{
				return true;
			}
			string name = SpineHelper.GetSkeletonName(parent);
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}
			MegaSkeletonDataResource skelData = SkeletonLoader.TryBuildMegaSkeletonDataResource(name);
			if (skelData == null)
			{
				return true;
			}
			ModLogger.Debug("[CustomSkeletonLoader] NSpineAutoPlayer: scheduling deferred replacement for " + name);
			Node parentRef = parent;
			Callable val = Callable.From((Action)delegate
			{
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)parentRef) && parentRef.IsInsideTree() && !((GodotObject)parentRef).IsQueuedForDeletion())
					{
						((GodotObject)parentRef).Call(StringName.op_Implicit("set_skeleton_data_res"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(((MegaSpineBinding)skelData).BoundObject) });
						SpineHelper.PlayFirstAvailableAnimation(parentRef);
						ModLogger.Debug("[CustomSkeletonLoader] NSpineAutoPlayer: deferred skeleton replacement + animation started for " + name);
					}
					else
					{
						ModLogger.Debug("[CustomSkeletonLoader] NSpineAutoPlayer: skipped deferred call, node no longer valid for " + name);
					}
				}
				catch (Exception ex)
				{
					ModLogger.Warn("[CustomSkeletonLoader] NSpineAutoPlayer deferred call error: " + ex.Message);
				}
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			ModLogger.Debug("[CustomSkeletonLoader] NSpineAutoPlayer: skeleton replacement scheduled for " + name);
			return false;
		}
		catch (Exception ex2)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NSpineAutoPlayer prefix error: " + ex2.Message);
			return true;
		}
	}
}
