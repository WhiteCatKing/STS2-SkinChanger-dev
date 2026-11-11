using System;
using System.Collections.Generic;
using Godot;

namespace JoanofArcIroncladHpwear;

internal static class WearCrossfade
{
	private sealed class Transition
	{
		internal required Node2D Overlay;

		internal required Tween Tween;
	}

	private static readonly Dictionary<ulong, Transition> Active = new Dictionary<ulong, Transition>();

	internal static bool Begin(Node node)
	{
		return BeginDeferred(node, () => true, delegate
		{
		});
	}

	internal static bool IsActive(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return Active.ContainsKey(((GodotObject)node).GetInstanceId());
		}
		return false;
	}

	internal static bool BeginDeferred(Node node, Func<bool> validate, Action commit)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		Node2D body = (Node2D)(object)((node is Node2D) ? node : null);
		if (body == null || !GodotObject.IsInstanceValid((GodotObject)(object)body) || !((Node)body).IsInsideTree())
		{
			return false;
		}
		Node parent = ((Node)body).GetParent();
		Resource skeletonData = SpineSwap.GetSkeletonData((Node)(object)body);
		if (parent == null || skeletonData == null)
		{
			return false;
		}
		Cancel((Node)(object)body);
		Node2D overlay = null;
		try
		{
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			Node2D ghost = default(Node2D);
			ref Node2D reference = ref ghost;
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			reference = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (ghost == null)
			{
				return false;
			}
			((Node)ghost).Name = StringName.op_Implicit("PreviousWear");
			((CanvasItem)ghost).Modulate = ((CanvasItem)body).Modulate;
			((CanvasItem)ghost).SelfModulate = ((CanvasItem)body).SelfModulate;
			((CanvasItem)ghost).TextureFilter = ((CanvasItem)body).TextureFilter;
			((CanvasItem)ghost).TextureRepeat = ((CanvasItem)body).TextureRepeat;
			((CanvasItem)ghost).Material = ((CanvasItem)body).Material;
			overlay = new Node2D
			{
				Name = StringName.op_Implicit("JoanWearCrossfade"),
				Transform = body.Transform,
				Visible = false,
				ZIndex = ((CanvasItem)body).ZIndex,
				ZAsRelative = ((CanvasItem)body).ZAsRelative,
				LightMask = ((CanvasItem)body).LightMask,
				VisibilityLayer = ((CanvasItem)body).VisibilityLayer
			};
			((Node)overlay).AddChild((Node)(object)ghost, false, (InternalMode)0);
			parent.AddChild((Node)(object)overlay, false, (InternalMode)0);
			parent.MoveChild((Node)(object)overlay, Math.Min(((Node)body).GetIndex(false) + 1, parent.GetChildCount(false) - 1));
			((GodotObject)ghost).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)skeletonData));
			UnderwearVisibility.Attach((Node)(object)ghost);
			Palette.ApplyNow((Node)(object)ghost);
			string text = SpineSwap.CurrentAnimation((Node)(object)body);
			if (!string.IsNullOrEmpty(text))
			{
				SpineSwap.Play((Node)(object)ghost, text, loop: true);
			}
			if (((GodotObject)ghost).HasMethod(StringName.op_Implicit("update_skeleton")))
			{
				((GodotObject)ghost).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
			}
			ulong id = ((GodotObject)body).GetInstanceId();
			Tween val2 = ((Node)overlay).CreateTween();
			Transition transition = new Transition
			{
				Overlay = overlay,
				Tween = val2
			};
			Active[id] = transition;
			((Node)overlay).TreeExiting += delegate
			{
				if (Active.TryGetValue(id, out Transition value) && value == transition)
				{
					Active.Remove(id);
				}
			};
			val2.TweenInterval(0.02);
			val2.TweenCallback(Callable.From((Action)delegate
			{
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (!GodotObject.IsInstanceValid((GodotObject)(object)body) || !((Node)body).IsInsideTree() || !validate())
					{
						CancelById(id);
					}
					else
					{
						string text2 = SpineSwap.CurrentAnimation((Node)(object)body);
						if (!string.IsNullOrEmpty(text2))
						{
							SpineSwap.Play((Node)(object)ghost, text2, loop: true);
						}
						if (((GodotObject)ghost).HasMethod(StringName.op_Implicit("update_skeleton")))
						{
							((GodotObject)ghost).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
						}
						((CanvasItem)overlay).Visible = true;
						commit();
						if (((GodotObject)body).HasMethod(StringName.op_Implicit("update_skeleton")))
						{
							((GodotObject)body).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
						}
					}
				}
				catch (Exception ex)
				{
					ModEntry.TryInfo("wear crossfade commit: " + ex.Message);
					CancelById(id);
				}
			}));
			val2.TweenProperty((GodotObject)(object)overlay, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.0), 0.25).SetTrans((TransitionType)1).SetEase((EaseType)2);
			val2.TweenCallback(Callable.From((Action)delegate
			{
				CancelById(id);
			}));
			return true;
		}
		catch (Exception ex2)
		{
			if (overlay != null && GodotObject.IsInstanceValid((GodotObject)(object)overlay))
			{
				((Node)overlay).QueueFree();
			}
			ModEntry.TryInfo("wear crossfade unavailable: " + ex2.Message);
			return false;
		}
	}

	internal static void Cancel(Node body)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)body))
		{
			CancelById(((GodotObject)body).GetInstanceId());
		}
	}

	private static void CancelById(ulong id)
	{
		if (Active.Remove(id, out Transition value))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Tween))
			{
				value.Tween.Kill();
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Overlay))
			{
				((Node)value.Overlay).QueueFree();
			}
		}
	}
}
