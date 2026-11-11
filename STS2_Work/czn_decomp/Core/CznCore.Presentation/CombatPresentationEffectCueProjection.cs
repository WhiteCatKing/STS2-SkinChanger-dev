using System;
using Godot;

namespace CznCore.Presentation;

internal static class CombatPresentationEffectCueProjection
{
	public static CombatPresentationEffectCue ProjectToScreenTarget(CombatPresentationEffectCue cue, Node2D targetAnchor, Node2D screenAnchor, CombatPresentationEffectAnchor projectedAnchor = CombatPresentationEffectAnchor.Screen)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = screenAnchor.ToLocal(targetAnchor.GlobalPosition);
		if (cue.NeutralizeAnchorScale)
		{
			Vector2 globalScale = screenAnchor.GlobalScale;
			((Vector2)(ref val))._002Ector(val.X * ((Math.Abs(globalScale.X) > 0.001f) ? globalScale.X : 1f), val.Y * ((Math.Abs(globalScale.Y) > 0.001f) ? globalScale.Y : 1f));
		}
		return cue with
		{
			Anchor = projectedAnchor,
			ProjectedTargetDepth = ResolveOrdinaryTargetDepth(targetAnchor),
			OffsetX = cue.OffsetX + val.X,
			OffsetY = cue.OffsetY + val.Y
		};
	}

	internal static int? ResolveOrdinaryTargetDepth(Node2D anchor)
	{
		int num = 0;
		Node val = (Node)(object)anchor;
		while (val != null && !(val is CanvasLayer))
		{
			CanvasItem val2 = (CanvasItem)(object)((val is CanvasItem) ? val : null);
			if (val2 != null)
			{
				num += val2.ZIndex;
				if (!val2.ZAsRelative)
				{
					break;
				}
			}
			val = val.GetParent();
		}
		if (num >= 0)
		{
			return null;
		}
		return num;
	}
}
