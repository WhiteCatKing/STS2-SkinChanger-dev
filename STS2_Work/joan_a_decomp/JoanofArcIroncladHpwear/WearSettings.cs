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

	public string SafetyHotkey => "";

	public double VisualScale { get; set; } = 1.0;

	public int Appearance { get; set; } = 1;

	public string AppearanceHotkey { get; set; } = "=";

	public bool HeavyAttackEnabled { get; set; } = true;

	public int HeavyAttackThreshold { get; set; } = 20;

	public int ResolveStage(int hp, int maxHp)
	{
		return 1;
	}

	public int ResolveDeathStage(int hp, int maxHp)
	{
		return 1;
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
		VisualScale = 1.0;
		Appearance = 1;
		AppearanceHotkey = "=";
		HeavyAttackEnabled = true;
		HeavyAttackThreshold = 20;
	}
}
