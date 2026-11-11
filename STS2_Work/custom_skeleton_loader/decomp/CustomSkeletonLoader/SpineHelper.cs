using System;
using System.IO;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CustomSkeletonLoader;

internal static class SpineHelper
{
	internal static string GetSkeletonName(GodotObject spineObj)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (spineObj == null)
		{
			return "";
		}
		try
		{
			Variant skelData = spineObj.Get(StringName.op_Implicit("skeleton_data_res"));
			return ExtractNameFromSkeletonData(skelData);
		}
		catch
		{
			return "";
		}
	}

	internal static string GetSkeletonName(Node node)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (node == null)
		{
			return "";
		}
		try
		{
			Variant skelData = ((GodotObject)node).Get(StringName.op_Implicit("skeleton_data_res"));
			return ExtractNameFromSkeletonData(skelData);
		}
		catch
		{
			return "";
		}
	}

	internal static string? GetCurrentAnimationName(GodotObject spineObj, int track = 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		try
		{
			MegaSprite val = new MegaSprite(Variant.op_Implicit(spineObj));
			MegaAnimationState animationState = val.GetAnimationState();
			MegaTrackEntry val2 = ((animationState != null) ? animationState.GetCurrent(track) : null);
			object result;
			if (val2 == null)
			{
				result = null;
			}
			else
			{
				MegaAnimation animation = val2.GetAnimation();
				result = ((animation != null) ? animation.GetName() : null);
			}
			return (string?)result;
		}
		catch (Exception ex)
		{
			ModLogger.Debug("[CustomSkeletonLoader] Failed to get animation name: " + ex.Message);
			return null;
		}
	}

	internal static void SetAnimation(GodotObject spineObj, string animName, bool loop = true, int track = 0)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (spineObj == null || string.IsNullOrEmpty(animName))
		{
			return;
		}
		try
		{
			MegaSprite val = new MegaSprite(Variant.op_Implicit(spineObj));
			MegaSkeleton skeleton = val.GetSkeleton();
			MegaSkeletonDataResource val2 = ((skeleton != null) ? skeleton.GetData() : null);
			if (val2 != null)
			{
				MegaAnimation val3 = val2.FindAnimation(animName);
				if (val3 == null)
				{
					ModLogger.Warn("[CustomSkeletonLoader] Animation '" + animName + "' not found in new skeleton. Skipping.");
					return;
				}
			}
			MegaAnimationState animationState = val.GetAnimationState();
			if (animationState != null)
			{
				animationState.SetAnimation(animName, loop, 0);
			}
		}
		catch (Exception ex)
		{
			ModLogger.Debug("[CustomSkeletonLoader] Failed to set animation: " + ex.Message);
		}
	}

	internal static void PlayFirstAvailableAnimation(Node spineNode, bool loop = true)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		try
		{
			MegaSprite val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spineNode));
			MegaSkeleton skeleton = val.GetSkeleton();
			object obj;
			if (skeleton == null)
			{
				obj = null;
			}
			else
			{
				MegaSkeletonDataResource data = skeleton.GetData();
				obj = ((data != null) ? data.GetAnimations() : null);
			}
			Array<GodotObject> val2 = (Array<GodotObject>)obj;
			if (val2 != null && val2.Count > 0)
			{
				MegaAnimation val3 = new MegaAnimation(Variant.op_Implicit(val2[0]));
				string name = val3.GetName();
				MegaAnimationState animationState = val.GetAnimationState();
				if (animationState != null)
				{
					animationState.SetAnimation(name, loop, 0);
				}
				ModLogger.Debug("[CustomSkeletonLoader] Playing first available animation: " + name);
			}
		}
		catch (Exception ex)
		{
			ModLogger.Debug("[CustomSkeletonLoader] Failed to play first animation: " + ex.Message);
		}
	}

	private static string ExtractNameFromSkeletonData(Variant skelData)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		GodotObject val = ((Variant)(ref skelData)).AsGodotObject();
		if (val == null)
		{
			return "";
		}
		Variant val2 = val.Get(StringName.op_Implicit("resource_path"));
		string text = ((Variant)(ref val2)).AsString();
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = Path.GetFileNameWithoutExtension(text);
			if (text2.EndsWith("_skel_data", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text2;
				int length = "_skel_data".Length;
				text2 = text3.Substring(0, text3.Length - length);
			}
			return text2;
		}
		return "";
	}
}
