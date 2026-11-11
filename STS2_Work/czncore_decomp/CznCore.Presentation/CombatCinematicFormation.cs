using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation;

internal static class CombatCinematicFormation
{
	public static bool TryGetViewportTransform(Node2D? anchor, out Transform2D viewportTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		viewportTransform = Transform2D.Identity;
		if (anchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return false;
		}
		try
		{
			Transform2D globalTransformWithCanvas = ((CanvasItem)anchor).GetGlobalTransformWithCanvas();
			if (!IsFinite(globalTransformWithCanvas))
			{
				return false;
			}
			viewportTransform = globalTransformWithCanvas;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool TryGetViewportPosition(Node2D? anchor, out Vector2 viewportPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		viewportPosition = Vector2.Zero;
		if (!TryGetViewportTransform(anchor, out var viewportTransform))
		{
			return false;
		}
		viewportPosition = viewportTransform.Origin;
		return true;
	}

	public static CombatCinematicFormationSnapshot Capture(Node2D? actorAnchor, IReadOnlyList<Node2D>? targetAnchors)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 viewportPosition;
		bool flag = TryGetViewportPosition(actorAnchor, out viewportPosition);
		List<Vector2> list = new List<Vector2>(targetAnchors?.Count ?? 0);
		if (targetAnchors != null)
		{
			foreach (Node2D targetAnchor in targetAnchors)
			{
				if (TryGetViewportPosition(targetAnchor, out var viewportPosition2))
				{
					list.Add(viewportPosition2);
				}
			}
		}
		Rect2 viewportRect = default(Rect2);
		Node2D val = (flag ? actorAnchor : FirstValidAnchor(targetAnchors));
		if (val != null)
		{
			try
			{
				Viewport viewport = ((Node)val).GetViewport();
				if (viewport != null && GodotObject.IsInstanceValid((GodotObject)(object)viewport))
				{
					viewportRect = viewport.GetVisibleRect();
				}
			}
			catch
			{
			}
		}
		return new CombatCinematicFormationSnapshot(flag, viewportPosition, list.ToArray(), viewportRect);
	}

	public static CombatCinematicFormationSnapshot CaptureSingle(Node2D? actorAnchor, Node2D? targetAnchor)
	{
		if (targetAnchor != null && GodotObject.IsInstanceValid((GodotObject)(object)targetAnchor))
		{
			return Capture(actorAnchor, (IReadOnlyList<Node2D>?)(object)new Node2D[1] { targetAnchor });
		}
		return Capture(actorAnchor, null);
	}

	public static Vector2 ResolvePackedTargetProxyOffset(int index, int count, Vector2 proxyBaseScale, float screenSpacing, float screenRowGap)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= count || count <= 1)
		{
			return Vector2.Zero;
		}
		float num = Math.Clamp((MathF.Abs(proxyBaseScale.X) + MathF.Abs(proxyBaseScale.Y)) * 0.5f, 0.42f, 1.35f);
		float num2 = screenSpacing / num;
		float num3 = screenRowGap / num;
		if (count < 5)
		{
			return (Vector2)(count switch
			{
				2 => (index == 0) ? new Vector2((0f - num2) * 0.62f, 0f) : new Vector2(num2 * 0.62f, 0f), 
				3 => index switch
				{
					0 => new Vector2(0f - num2, num3 * 0.25f), 
					1 => new Vector2(0f, (0f - num3) * 0.35f), 
					_ => new Vector2(num2, num3 * 0.25f), 
				}, 
				4 => index switch
				{
					0 => new Vector2((0f - num2) * 0.72f, (0f - num3) * 0.45f), 
					1 => new Vector2(num2 * 0.72f, (0f - num3) * 0.45f), 
					2 => new Vector2((0f - num2) * 0.72f, num3 * 0.55f), 
					_ => new Vector2(num2 * 0.72f, num3 * 0.55f), 
				}, 
				_ => Vector2.Zero, 
			});
		}
		return (Vector2)(index switch
		{
			0 => new Vector2(0f - num2, (0f - num3) * 0.48f), 
			1 => new Vector2(0f, (0f - num3) * 0.6f), 
			2 => new Vector2(num2, (0f - num3) * 0.48f), 
			3 => new Vector2((0f - num2) * 0.55f, num3 * 0.58f), 
			_ => new Vector2(num2 * 0.55f, num3 * 0.58f), 
		});
	}

	private static Node2D? FirstValidAnchor(IReadOnlyList<Node2D>? anchors)
	{
		if (anchors == null)
		{
			return null;
		}
		foreach (Node2D anchor in anchors)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor))
			{
				return anchor;
			}
		}
		return null;
	}

	private static bool IsFinite(Transform2D transform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (IsFinite(transform.X) && IsFinite(transform.Y))
		{
			return IsFinite(transform.Origin);
		}
		return false;
	}

	private static bool IsFinite(Vector2 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsFinite(value.X))
		{
			return float.IsFinite(value.Y);
		}
		return false;
	}
}
