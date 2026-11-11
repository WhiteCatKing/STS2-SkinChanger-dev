using System;

namespace CznEnemySkin.Domain;

internal static class CznEnemyVisualScalePolicy
{
	internal static float Resolve(float officialModelScale)
	{
		if (!float.IsFinite(officialModelScale) || officialModelScale <= 0f)
		{
			throw new ArgumentOutOfRangeException("officialModelScale", "Official CZN model scale must be finite and greater than zero.");
		}
		return officialModelScale;
	}

	internal static float ResolveSts2HostFit(float officialModelScale, float calibratedVisualScale)
	{
		Resolve(officialModelScale);
		if (!float.IsFinite(calibratedVisualScale) || calibratedVisualScale <= 0f)
		{
			throw new ArgumentOutOfRangeException("calibratedVisualScale", "STS2 host-fit visual scale must be finite and greater than zero.");
		}
		return calibratedVisualScale;
	}

	internal static float ResolveSts2ScaleCap(float officialModelScale, float maximumScale)
	{
		if (!float.IsFinite(maximumScale) || maximumScale <= 0f)
		{
			throw new ArgumentOutOfRangeException("maximumScale", "STS2 scale cap must be finite and greater than zero.");
		}
		return MathF.Min(Resolve(officialModelScale), maximumScale);
	}
}
