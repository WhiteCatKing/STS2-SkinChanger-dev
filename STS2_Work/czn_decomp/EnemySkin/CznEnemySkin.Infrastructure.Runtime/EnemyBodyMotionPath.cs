using System;
using Godot;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyBodyMotionPath
{
	public static Vector2 ParabolicArc(Vector2 start, Vector2 end, float arcHeight, float progress)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(progress, 0f, 1f);
		Vector2 result = start + (end - start) * num;
		result.Y -= 4f * MathF.Max(0f, arcHeight) * num * (1f - num);
		return result;
	}
}
