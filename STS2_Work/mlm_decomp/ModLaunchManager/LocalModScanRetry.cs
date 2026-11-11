using System;

namespace ModLaunchManager;

internal sealed class LocalModScanRetry
{
	private static readonly TimeSpan[] Delays = new TimeSpan[3]
	{
		TimeSpan.FromSeconds(1L),
		TimeSpan.FromSeconds(3L),
		TimeSpan.FromSeconds(10L)
	};

	private int _failures;

	internal TimeSpan? Failed(out bool gaveUp)
	{
		gaveUp = _failures == Delays.Length;
		if (_failures <= Delays.Length)
		{
			_failures++;
		}
		if (_failures > Delays.Length)
		{
			return null;
		}
		return Delays[_failures - 1];
	}

	internal void Succeeded()
	{
		_failures = 0;
	}
}
