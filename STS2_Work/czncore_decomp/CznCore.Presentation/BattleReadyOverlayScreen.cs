using System;
using Godot;

namespace CznCore.Presentation;

internal static class BattleReadyOverlayScreen
{
	public static (float Scale, float BottomOffset) Fit(float width, float height)
	{
		if (!float.IsFinite(width) || !float.IsFinite(height) || width <= 0f || height <= 0f)
		{
			return (Scale: 1f, BottomOffset: 0f);
		}
		float num = Math.Min(width / 1920f, height / 1080f);
		return (Scale: num, BottomOffset: height - 1080f * num);
	}

	public static Node? Resolve(Node? parent)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		Control val = (Control)(object)((parent is Control) ? parent : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return parent;
		}
		Node2D nodeOrNull = ((Node)val).GetNodeOrNull<Node2D>(NodePath.op_Implicit("CznBattleReadyScreen"));
		if (GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
		{
			return (Node?)(object)nodeOrNull;
		}
		nodeOrNull = new Node2D
		{
			Name = StringName.op_Implicit("CznBattleReadyScreen")
		};
		((Node)val).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
		Bind(nodeOrNull, (CanvasItem)(object)val);
		return (Node?)(object)nodeOrNull;
	}

	public static void Bind(Node2D screen, CanvasItem parent)
	{
		Viewport viewport = ((Node)parent).GetViewport();
		viewport.SizeChanged += Layout;
		((Node)screen).TreeExiting += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				viewport.SizeChanged -= Layout;
			}
		};
		Layout();
		void Layout()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen) && GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				Rect2 visibleRect = viewport.GetVisibleRect();
				Vector2 size = ((Rect2)(ref visibleRect)).Size;
				(float, float) tuple = Fit(size.X, size.Y);
				Transform2D val = default(Transform2D);
				((Transform2D)(ref val))._002Ector(Vector2.Right * tuple.Item1, Vector2.Down * tuple.Item1, new Vector2(0f, tuple.Item2));
				Node2D obj = screen;
				Transform2D globalTransformWithCanvas = parent.GetGlobalTransformWithCanvas();
				obj.Transform = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * val;
			}
		}
	}
}
