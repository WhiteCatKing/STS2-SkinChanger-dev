using System;

namespace STS2SkinChanger.Ui;

internal sealed class CreatureIntentDepthState
{
	private (int Z, bool Relative)? _baseline;

	private (int Z, bool Relative)? _applied;

	internal (int Z, bool Relative) Resolve(int currentZ, bool relative, int parentZ, int visualZ, bool detachedVisual)
	{
		(int, bool) tuple = (currentZ, relative);
		if (_baseline.HasValue)
		{
			(int, bool)? applied = _applied;
			(int, bool) tuple2 = tuple;
			bool hasValue = applied.HasValue;
			if (hasValue)
			{
				if (hasValue)
				{
					(int, bool) valueOrDefault = applied.GetValueOrDefault();
					if (valueOrDefault.Item1 != tuple2.Item1 || valueOrDefault.Item2 != tuple2.Item2)
					{
						goto IL_0068;
					}
				}
				goto IL_0074;
			}
		}
		goto IL_0068;
		IL_0068:
		_baseline = tuple;
		goto IL_0074;
		IL_0074:
		(int, bool) value = _baseline.Value;
		int num = Math.Max(value.Item1 + (value.Item2 ? parentZ : 0), visualZ + (detachedVisual ? 1 : 0));
		_applied = (Math.Clamp(num - (value.Item2 ? parentZ : 0), -4096, 4096), value.Item2);
		return _applied.Value;
	}
}
