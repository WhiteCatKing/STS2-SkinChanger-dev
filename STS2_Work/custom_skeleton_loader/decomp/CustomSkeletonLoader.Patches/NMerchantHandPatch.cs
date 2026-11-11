using System;
using System.Reflection;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(NMerchantHand))]
internal static class NMerchantHandPatch
{
	[HarmonyPatch("_Ready")]
	[HarmonyPostfix]
	internal static void Postfix(NMerchantHand __instance)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Node2D parent = ((Node)__instance).GetParent<Node2D>();
			if (parent == null)
			{
				ModLogger.Debug("[CustomSkeletonLoader] NMerchantHand: parent is null");
				return;
			}
			string skeletonName = SpineHelper.GetSkeletonName((Node)(object)parent);
			ModLogger.Debug("[CustomSkeletonLoader] NMerchantHand: parent type=" + ((object)parent).GetType().Name + ", skeleton name=" + skeletonName);
			if (string.IsNullOrEmpty(skeletonName))
			{
				return;
			}
			MegaSkeletonDataResource skelData = SkeletonLoader.TryBuildMegaSkeletonDataResource(skeletonName);
			if (skelData == null)
			{
				ModLogger.Debug("[CustomSkeletonLoader] NMerchantHand: no custom skeleton for " + skeletonName);
				return;
			}
			ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] intercepting: " + skeletonName + ", scheduling deferred replacement");
			NMerchantHand handRef = __instance;
			Node2D parentRef = parent;
			Callable val4 = Callable.From((Action)delegate
			{
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				//IL_019c: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (!GodotObject.IsInstanceValid((GodotObject)(object)parentRef) || !((Node)parentRef).IsInsideTree())
					{
						ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] parent no longer valid, skipping");
					}
					else
					{
						((GodotObject)parentRef).Call(StringName.op_Implicit("set_skeleton_data_res"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(((MegaSpineBinding)skelData).BoundObject) });
						ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] skeleton_data_res replaced");
						FieldInfo field = typeof(NMerchantHand).GetField("_bone", BindingFlags.Instance | BindingFlags.NonPublic);
						FieldInfo field2 = typeof(NMerchantHand).GetField("_animController", BindingFlags.Instance | BindingFlags.NonPublic);
						if (field == null || field2 == null)
						{
							ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] reflection fields not found");
						}
						else
						{
							object? value = field2.GetValue(handRef);
							MegaSprite val = (MegaSprite)((value is MegaSprite) ? value : null);
							if (val != null)
							{
								MegaSkeleton skeleton = val.GetSkeleton();
								if (skeleton != null)
								{
									MegaBone val2 = skeleton.FindBone("rotate_me");
									if (val2 != null)
									{
										field.SetValue(handRef, val2);
										ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] 'rotate_me' bone re-initialized");
									}
									else
									{
										ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] 'rotate_me' not found in new skeleton");
									}
									MegaAnimationState animationState = val.GetAnimationState();
									if (animationState != null)
									{
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
										Array<GodotObject> val3 = (Array<GodotObject>)obj;
										string text = "default";
										if (val3 != null && val3.Count > 0)
										{
											text = new MegaAnimation(Variant.op_Implicit(val3[0])).GetName();
										}
										animationState.SetAnimation(text, true, 0);
										ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] animation set to '" + text + "'");
									}
								}
							}
							ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] deferred replacement complete");
						}
					}
				}
				catch (Exception ex)
				{
					ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] deferred error: " + ex.Message + "\n" + ex.StackTrace);
				}
			});
			((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
			ModLogger.Warn("[CustomSkeletonLoader] [MerchantHand] deferred replacement scheduled for " + skeletonName);
		}
		catch (Exception ex2)
		{
			ModLogger.Warn("[CustomSkeletonLoader] NMerchantHand patch error: " + ex2.Message + "\n" + ex2.StackTrace);
		}
	}
}
