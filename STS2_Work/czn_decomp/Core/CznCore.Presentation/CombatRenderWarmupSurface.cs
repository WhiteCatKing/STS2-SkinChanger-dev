using System;
using Godot;

namespace CznCore.Presentation;

public static class CombatRenderWarmupSurface
{
	public static bool MountTransient(Node2D root, string surfaceName, string rootName)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = Resolve(surfaceName);
		if (val == null)
		{
			return false;
		}
		Node parent = ((Node)root).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)root);
		}
		((Node)root).Name = StringName.op_Implicit(rootName);
		root.Position = Vector2.Zero;
		root.Scale = Vector2.One;
		root.Rotation = 0f;
		((CanvasItem)root).TopLevel = false;
		((CanvasItem)root).ZAsRelative = true;
		((CanvasItem)root).ZIndex = 0;
		((CanvasItem)root).Modulate = Colors.White;
		((CanvasItem)root).Visible = true;
		((Node)val).AddChild((Node)(object)root, false, (InternalMode)0);
		return true;
	}

	public static Node2D? Resolve(string name)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		if (!(DisplayServer.GetName() == "headless"))
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				string text = name + "Surface";
				SubViewport nodeOrNull = ((Node)val.Root).GetNodeOrNull<SubViewport>(NodePath.op_Implicit(text));
				if (nodeOrNull != null && !((GodotObject)nodeOrNull).IsQueuedForDeletion())
				{
					return ((Node)nodeOrNull).GetNode<Node2D>(NodePath.op_Implicit("Anchor"));
				}
				Rect2 visibleRect = ((Viewport)val.Root).GetVisibleRect();
				Vector2 size = ((Rect2)(ref visibleRect)).Size;
				SubViewport viewport = new SubViewport
				{
					Name = StringName.op_Implicit(text),
					Size = (Vector2I)size,
					TransparentBg = true,
					Disable3D = true,
					UseHdr2D = ((Viewport)val.Root).UseHdr2D,
					RenderTargetUpdateMode = (UpdateMode)4
				};
				Node2D anchor = new Node2D
				{
					Name = StringName.op_Implicit("Anchor"),
					Position = size / 2f
				};
				((Node)viewport).AddChild((Node)(object)anchor, false, (InternalMode)0);
				((Node)val.Root).AddChild((Node)(object)viewport, false, (InternalMode)0);
				((Node)anchor).ChildExitingTree += (ChildExitingTreeEventHandler)delegate
				{
					//IL_001f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0024: Unknown result type (might be due to invalid IL or missing references)
					Callable val2 = Callable.From((Action)delegate
					{
						if (GodotObject.IsInstanceValid((GodotObject)(object)viewport) && !((GodotObject)viewport).IsQueuedForDeletion() && ((Node)anchor).GetChildCount(false) == 0)
						{
							viewport.RenderTargetUpdateMode = (UpdateMode)0;
							((Node)viewport).GetParent().RemoveChild((Node)(object)viewport);
							((Node)viewport).QueueFree();
						}
					});
					((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
				};
				return anchor;
			}
		}
		return null;
	}
}
