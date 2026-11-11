using System;

namespace JoanofArcIroncladHpwear;

public sealed class WearSettings
{
	public bool ShowUnderwear { get; set; } = true;

	public int LowHealthIdlePercent { get; set; } = 25;

	public bool WearEnabled { get; set; } = true;

	public bool UseAbsoluteHp { get; set; }

	public int PercentThreshold { get; set; } = 50;

	public int AbsoluteThreshold { get; set; } = 40;

	public bool ForceDeepestOnDeath { get; set; } = true;

	public string SafetyHotkey { get; set; } = "F6";

	public double VisualScale { get; set; } = 1.0;

	public int Appearance { get; set; } = 1;

	public string AppearanceHotkey { get; set; } = "=";

	public bool HeavyAttackEnabled { get; set; } = true;

	public int HeavyAttackThreshold { get; set; } = 20;

	public int ResolveStage(int hp, int maxHp)
	{
		if (maxHp <= 0)
		{
			return 0;
		}
		if (!WearEnabled)
		{
			return 1;
		}
		if (UseAbsoluteHp)
		{
			if (hp <= Math.Clamp(AbsoluteThreshold, 0, 999))
			{
				return 2;
			}
			return 1;
		}
		if ((long)hp * 100L <= (long)maxHp * (long)Math.Clamp(PercentThreshold, 0, 100))
		{
			return 2;
		}
		return 1;
	}

	public int ResolveDeathStage(int hp, int maxHp)
	{
		if (!WearEnabled)
		{
			return 1;
		}
		if (!ForceDeepestOnDeath)
		{
			return ResolveStage(hp, maxHp);
		}
		return 2;
	}

	public void Reset()
	{
		ShowUnderwear = true;
		LowHealthIdlePercent = 25;
		WearEnabled = true;
		UseAbsoluteHp = false;
		PercentThreshold = 50;
		AbsoluteThreshold = 40;
		ForceDeepestOnDeath = true;
		SafetyHotkey = "F6";
		VisualScale = 1.0;
		Appearance = 1;
		AppearanceHotkey = "=";
		HeavyAttackEnabled = true;
		HeavyAttackThreshold = 20;
	}
}
