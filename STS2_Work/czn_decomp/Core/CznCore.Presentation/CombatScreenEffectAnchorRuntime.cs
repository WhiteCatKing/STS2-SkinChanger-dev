using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal static class CombatScreenEffectAnchorRuntime
{
	public static Node2D? Resolve(string anchorName, CombatScreenEffectLayer layer, Vector2 referenceSize, CombatScreenEffectScaleMode scaleMode = CombatScreenEffectScaleMode.Fit, Action<string>? warn = null)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null && val.Root != null)
			{
				NCombatRoom instance = NCombatRoom.Instance;
				if (instance != null)
				{
					Control val2 = ((layer == CombatScreenEffectLayer.Back) ? instance.BackCombatVfxContainer : instance.CombatVfxContainer);
					if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
					{
						Node2D val3 = ((Node)val2).GetNodeOrNull<Node2D>(new NodePath(anchorName));
						if (val3 == null)
						{
							val3 = new Node2D
							{
								Name = StringName.op_Implicit(anchorName)
							};
							((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
						}
						Rect2 visibleRect = ((Viewport)val.Root).GetVisibleRect();
						Vector2 size = ((Rect2)(ref visibleRect)).Size;
						float num = ResolveScale(size, referenceSize, scaleMode);
						((CanvasItem)val3).TopLevel = false;
						((CanvasItem)val3).ZAsRelative = false;
						((CanvasItem)val3).ZIndex = ((layer == CombatScreenEffectLayer.Back) ? (-10) : (-3));
						val3.GlobalPosition = size * 0.5f;
						val3.GlobalRotation = 0f;
						val3.GlobalScale = Vector2.One * num;
						((CanvasItem)val3).Visible = true;
						return val3;
					}
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			warn?.Invoke(ex.ToString());
			return null;
		}
	}

	private static float ResolveScale(Vector2 viewportSize, Vector2 referenceSize, CombatScreenEffectScaleMode scaleMode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (viewportSize.X <= 0f || viewportSize.Y <= 0f || referenceSize.X <= 0f || referenceSize.Y <= 0f)
		{
			return 1f;
		}
		float x = viewportSize.X / referenceSize.X;
		float y = viewportSize.Y / referenceSize.Y;
		float y2 = ((scaleMode == CombatScreenEffectScaleMode.Cover) ? MathF.Max(x, y) : MathF.Min(x, y));
		return MathF.Max(0.001f, y2);
	}
}
