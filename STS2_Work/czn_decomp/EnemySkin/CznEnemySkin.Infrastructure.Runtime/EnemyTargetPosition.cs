using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyTargetPosition
{
	internal static Vector2 Resolve(NCreature target, EnemyTargetOrigin origin)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (origin != EnemyTargetOrigin.Body)
		{
			return ResolveFloor(target);
		}
		return target.VfxSpawnPosition;
	}

	internal static Vector2 ResolveFloor(NCreature target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2 bottomOfHitbox = target.GetBottomOfHitbox();
		return new Vector2(target.VfxSpawnPosition.X, bottomOfHitbox.Y);
	}

	internal static Vector2 ResolveSideFloor(IEnumerable<NCreature> targets)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = float.NegativeInfinity;
		int num3 = 0;
		foreach (NCreature target in targets)
		{
			Vector2 val = ResolveFloor(target);
			num += val.X;
			num2 = Math.Max(num2, val.Y);
			num3++;
		}
		if (num3 == 0)
		{
			throw new ArgumentException("Target floor requires a target", "targets");
		}
		return new Vector2(num / (float)num3, num2);
	}
}
