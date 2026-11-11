using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation;

internal readonly struct CombatCinematicFormationSnapshot
{
	private const float SideEpsilon = 0.5f;

	private readonly Vector2[] _targetViewportPositions;

	public bool HasActorAnchor { get; }

	public Vector2 ActorViewportPosition { get; }

	public IReadOnlyList<Vector2> TargetViewportPositions => _targetViewportPositions ?? Array.Empty<Vector2>();

	public int TargetCount
	{
		get
		{
			Vector2[] targetViewportPositions = _targetViewportPositions;
			if (targetViewportPositions == null)
			{
				return 0;
			}
			return targetViewportPositions.Length;
		}
	}

	public bool HasTargets => TargetCount > 0;

	public Vector2 TargetViewportCenter { get; }

	public Rect2 ViewportRect { get; }

	public CombatCinematicFormationSnapshot(bool hasActorAnchor, Vector2 actorViewportPosition, Vector2[] targetViewportPositions, Rect2 viewportRect)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		HasActorAnchor = hasActorAnchor;
		ActorViewportPosition = actorViewportPosition;
		_targetViewportPositions = targetViewportPositions ?? Array.Empty<Vector2>();
		ViewportRect = viewportRect;
		Vector2 val = Vector2.Zero;
		Vector2[] targetViewportPositions2 = _targetViewportPositions;
		foreach (Vector2 val2 in targetViewportPositions2)
		{
			val += val2;
		}
		TargetViewportCenter = ((_targetViewportPositions.Length == 0) ? Vector2.Zero : (val / (float)_targetViewportPositions.Length));
	}

	public Vector2 ResolveSingleTargetOnActorRight(out bool crossedActorSide)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		crossedActorSide = false;
		if (!HasActorAnchor || TargetCount != 1)
		{
			return TargetViewportCenter;
		}
		Vector2 val = _targetViewportPositions[0];
		float num = val.X - ActorViewportPosition.X;
		if (num >= -0.5f)
		{
			return val;
		}
		float num2 = ActorViewportPosition.X - num;
		Rect2 viewportRect = ViewportRect;
		if (((Rect2)(ref viewportRect)).Size.X > 0.5f)
		{
			viewportRect = ViewportRect;
			float x = ((Rect2)(ref viewportRect)).Position.X;
			viewportRect = ViewportRect;
			float max = x + ((Rect2)(ref viewportRect)).Size.X;
			num2 = Math.Clamp(num2, x, max);
		}
		if (num2 <= ActorViewportPosition.X + 0.5f)
		{
			return val;
		}
		crossedActorSide = true;
		return new Vector2(num2, val.Y);
	}
}
