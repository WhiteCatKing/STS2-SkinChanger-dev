namespace STS2SkinChanger.Core;

internal static class FrameworkProviderInitializerPolicy
{
	public static bool RunsAuthorInitializer(bool hasDeclarativeCharacterAssetReplacement, bool isNativeFrameworkProvider)
	{
		if (hasDeclarativeCharacterAssetReplacement)
		{
			return !isNativeFrameworkProvider;
		}
		return true;
	}

	public static bool KeepsSessionWhenAuthorInitializerFails(bool hasDeclarativeCharacterAssetReplacement)
	{
		return hasDeclarativeCharacterAssetReplacement;
	}
}
