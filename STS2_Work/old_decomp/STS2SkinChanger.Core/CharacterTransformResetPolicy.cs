using System;

namespace STS2SkinChanger.Core;

internal static class CharacterTransformResetPolicy
{
	private const float Epsilon = 0.0001f;

	public static bool NeedsModelReset(CharacterCombatTransform value)
	{
		if (!(MathF.Abs(value.Scale - 1f) > 0.0001f) && !(MathF.Abs(value.OffsetX) > 0.0001f))
		{
			return MathF.Abs(value.OffsetY) > 0.0001f;
		}
		return true;
	}

	public static CharacterCombatTransform ResetModel(CharacterCombatTransform value)
	{
		return value with
		{
			Scale = 1f,
			OffsetX = 0f,
			OffsetY = 0f
		};
	}
}
