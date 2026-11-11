using System;

namespace STS2SkinChanger.Core;

internal sealed class ModThemeSession
{
	private ModThemeSettings _revertBaseline = saved.Normalize();

	public ModThemeSettings Current { get; private set; } = saved.Normalize();

	public event Action? Changed;

	public ModThemeSession(ModThemeSettings saved)
	{
	}

	public void Preview(ModThemeSettings settings)
	{
		ModThemeSettings modThemeSettings = settings.Normalize();
		if (!(modThemeSettings == Current))
		{
			Current = modThemeSettings;
			this.Changed?.Invoke();
		}
	}

	public void ApplyPreset(ModThemeSettings settings)
	{
		_revertBaseline = settings.Normalize();
		Preview(_revertBaseline);
	}

	public void Revert()
	{
		Preview(_revertBaseline);
	}

	public void Save(string path)
	{
		ModThemeStore.Save(path, Current);
	}
}
