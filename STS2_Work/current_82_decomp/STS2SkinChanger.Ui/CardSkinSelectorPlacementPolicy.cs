using System;

namespace STS2SkinChanger.Ui;

internal static class CardSkinSelectorPlacementPolicy
{
	public const float SelectorWidth = 336f;

	public const float SelectorHeight = 48f;

	public const float ReferenceViewportHeight = 1200f;

	public static readonly NormalizedControlPosition DefaultPosition = new NormalizedControlPosition(0.5f, 0.08166666f);

	public static NormalizedControlPosition ResolveStored(float? storedX, float? storedY)
	{
		if (storedX.HasValue)
		{
			float valueOrDefault = storedX.GetValueOrDefault();
			if (storedY.HasValue)
			{
				float valueOrDefault2 = storedY.GetValueOrDefault();
				if (float.IsFinite(valueOrDefault) && float.IsFinite(valueOrDefault2))
				{
					return new NormalizedControlPosition(Math.Clamp(valueOrDefault, 0f, 1f), Math.Clamp(valueOrDefault2, 0f, 1f));
				}
			}
		}
		return DefaultPosition;
	}

	public static NormalizedControlPosition ClampNormalized(float requestedX, float requestedY, float viewportWidth, float viewportHeight)
	{
		if (!float.IsFinite(requestedX) || !float.IsFinite(requestedY) || !float.IsFinite(viewportWidth) || !float.IsFinite(viewportHeight) || viewportWidth <= 0f || viewportHeight <= 0f)
		{
			return DefaultPosition;
		}
		float num = Math.Min(0.5f, 168f / viewportWidth);
		float num2 = Math.Min(0.5f, 24f / viewportHeight);
		return new NormalizedControlPosition(Math.Clamp(requestedX, num, 1f - num), Math.Clamp(requestedY, num2, 1f - num2));
	}
}
