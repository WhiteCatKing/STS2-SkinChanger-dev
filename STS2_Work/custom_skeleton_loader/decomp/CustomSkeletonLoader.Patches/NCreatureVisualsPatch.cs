using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NCreatureVisuals))]
internal static class NCreatureVisualsPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPostfix]
	internal static void Postfix(NCreatureVisuals __instance)
	{
		try
		{
			if (__instance == null || __instance.SpineBody == null)
			{
				return;
			}
			MegaSprite spineBody = __instance.SpineBody;
			GodotObject val = ((spineBody != null) ? ((MegaSpineBinding)spineBody).BoundObject : null);
			if (val == null)
			{
				return;
			}
			string skeletonName = SpineHelper.GetSkeletonName(val);
			if (string.IsNullOrEmpty(skeletonName))
			{
				return;
			}
			MegaSkeletonDataResource val2 = SkeletonLoader.TryBuildMegaSkeletonDataResource(skeletonName);
			if (val2 == null)
			{
				return;
			}
			string currentAnimationName = SpineHelper.GetCurrentAnimationName(val);
			try
			{
				__instance.SpineBody.SetSkeletonDataRes(val2);
				if (!string.IsNullOrEmpty(currentAnimationName))
				{
					SpineHelper.SetAnimation(val, currentAnimationName);
					ModLogger.Debug("[CustomSkeletonLoader] NCreatureVisuals animation restored: " + currentAnimationName);
				}
			}
			catch (Exception ex)
			{
				ModLogger.Warn("[CustomSkeletonLoader] NCreatureVisuals error setting skeleton data for '" + skeletonName + "': " + ex.Message);
			}
		}
		catch (Exception ex2)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NCreatureVisuals patch error: " + ex2.Message);
		}
	}
}
