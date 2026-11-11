using System;

namespace JoanofArcIroncladHpwear;

public static class HpWearRules
{
	public const int HighStage = 1;

	public const int LowStage = 2;

	public const int InvalidStage = 0;

	public static int StageFor(int currentHp, int maxHp)
	{
		if (maxHp <= 0)
		{
			return 0;
		}
		if (currentHp <= 0)
		{
			return 2;
		}
		if ((long)currentHp * 2L <= maxHp)
		{
			return 2;
		}
		return 1;
	}

	public static int NextStage(int current, int maxStage)
	{
		if (maxStage < 1)
		{
			return 1;
		}
		if (current < 1 || current >= maxStage)
		{
			return 1;
		}
		return current + 1;
	}

	public static bool IsIdleLike(string? animation)
	{
		if (string.IsNullOrWhiteSpace(animation))
		{
			return true;
		}
		if (!animation.Contains("idle", StringComparison.OrdinalIgnoreCase) && !animation.Contains("loop", StringComparison.OrdinalIgnoreCase) && !animation.Contains("relaxed", StringComparison.OrdinalIgnoreCase) && !animation.Contains("glory", StringComparison.OrdinalIgnoreCase))
		{
			return animation.Contains("animation", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static bool CanSwapDuring(string? animation)
	{
		return IsIdleLike(animation);
	}

	public static string SafeLoopAnimation(string group, string? current)
	{
		if (!IsIdleLike(current) || string.IsNullOrWhiteSpace(current))
		{
			return DefaultAnimationFor(group);
		}
		return current;
	}

	public static string DefaultAnimationFor(string group)
	{
		return group switch
		{
			"restsite" => "glory_loop", 
			"charselect" => "animation", 
			"ironclad_shop" => "idle_loop", 
			_ => "idle_loop", 
		};
	}

	public static string AnimationAfterSwap(string group, string? current)
	{
		if (!string.IsNullOrWhiteSpace(current))
		{
			return current;
		}
		return DefaultAnimationFor(group);
	}
}
