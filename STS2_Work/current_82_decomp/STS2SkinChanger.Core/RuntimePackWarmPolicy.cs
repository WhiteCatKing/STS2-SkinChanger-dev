namespace STS2SkinChanger.Core;

internal static class RuntimePackWarmPolicy
{
	internal const long MaximumWarmPackBytes = 67108864L;

	public static bool ShouldWarm(long sizeBytes, bool alreadyWarmed)
	{
		if (!alreadyWarmed && sizeBytes > 0)
		{
			return sizeBytes <= 67108864;
		}
		return false;
	}
}
