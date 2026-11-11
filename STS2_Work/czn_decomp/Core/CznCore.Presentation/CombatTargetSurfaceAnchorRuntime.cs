using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatTargetSurfaceAnchorRuntime
{
	public static Node2D? Resolve(NCreature? targetNode, string anchorName, string failurePrefix, Action<string, int>? warn = null, Action<NCreature, Node2D, Vector2, string>? diagnostic = null, CombatTargetSurfaceAnchorMode mode = CombatTargetSurfaceAnchorMode.FloorLockedVfxX)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (targetNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)targetNode))
		{
			return null;
		}
		CombatCreatureUiDepthRuntime.EnsureAbovePresentation(targetNode);
		try
		{
			Node2D val = ((Node)targetNode).GetNodeOrNull<Node2D>(new NodePath(anchorName));
			if (val == null)
			{
				val = new Node2D
				{
					Name = StringName.op_Implicit(anchorName)
				};
				((Node)targetNode).AddChild((Node)(object)val, false, (InternalMode)0);
			}
			string selectedAnchor;
			Vector2 val2 = ResolveTargetSurfaceGlobal(targetNode, mode, out selectedAnchor);
			Node2D obj = val;
			Transform2D globalTransform = ((CanvasItem)targetNode).GetGlobalTransform();
			obj.Position = ((Transform2D)(ref globalTransform)).Inverse() * val2;
			val.Scale = Vector2.One;
			val.Rotation = 0f;
			((CanvasItem)val).TopLevel = false;
			((CanvasItem)val).ZAsRelative = true;
			((CanvasItem)val).ZIndex = 0;
			((CanvasItem)val).ShowBehindParent = true;
			((CanvasItem)val).Visible = true;
			diagnostic?.Invoke(targetNode, val, val2, selectedAnchor);
			return val;
		}
		catch (Exception ex)
		{
			warn?.Invoke(failurePrefix + SkinLog.FormatException(ex), 2);
			return null;
		}
	}

	private static bool TryGetTargetVfxSpawnGlobal(NCreature targetNode, out Vector2 vfxSpawnGlobal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		vfxSpawnGlobal = Vector2.Zero;
		try
		{
			vfxSpawnGlobal = targetNode.VfxSpawnPosition;
			return vfxSpawnGlobal != Vector2.Zero;
		}
		catch
		{
			return false;
		}
	}

	private static bool TryGetTargetHitboxCenterGlobal(NCreature targetNode, out Vector2 hitboxCenterGlobal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		hitboxCenterGlobal = Vector2.Zero;
		try
		{
			Control hitbox = targetNode.Hitbox;
			if (hitbox == null || !GodotObject.IsInstanceValid((GodotObject)(object)hitbox) || hitbox.Size.X <= 0f || hitbox.Size.Y <= 0f)
			{
				return false;
			}
			hitboxCenterGlobal = hitbox.GlobalPosition + hitbox.Size * 0.5f;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static Vector2 ResolveTargetSurfaceGlobal(NCreature targetNode, CombatTargetSurfaceAnchorMode mode, out string selectedAnchor)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (mode == CombatTargetSurfaceAnchorMode.VfxSpawnPosition && TryGetTargetVfxSpawnGlobal(targetNode, out var vfxSpawnGlobal))
		{
			selectedAnchor = "vfx_spawn";
			return vfxSpawnGlobal;
		}
		if (mode == CombatTargetSurfaceAnchorMode.HitboxCenter)
		{
			if (TryGetTargetHitboxCenterGlobal(targetNode, out var hitboxCenterGlobal))
			{
				selectedAnchor = "hitbox_center";
				return hitboxCenterGlobal;
			}
			if (TryGetTargetVfxSpawnGlobal(targetNode, out vfxSpawnGlobal))
			{
				selectedAnchor = "vfx_spawn_fallback";
				return vfxSpawnGlobal;
			}
		}
		Vector2 bottomOfHitbox = targetNode.GetBottomOfHitbox();
		selectedAnchor = "hitbox_bottom";
		if (TryGetTargetVfxSpawnGlobal(targetNode, out vfxSpawnGlobal))
		{
			selectedAnchor = "floor_locked_vfx_x";
			return new Vector2(vfxSpawnGlobal.X, bottomOfHitbox.Y);
		}
		return bottomOfHitbox;
	}
}
