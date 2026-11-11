namespace STS2SkinChanger.Core;

internal static class MonsterRuntimePresentationPolicy
{
	public static bool ProviderKeepsVisualCallback(bool providerOwnsPerMonsterProfiles, MonsterVisualCallback callback)
	{
		bool flag = providerOwnsPerMonsterProfiles;
		if (flag)
		{
			bool flag2 = (uint)(callback - 1) <= 2u;
			flag = flag2;
		}
		return flag;
	}

	public static bool DefersVisualReplacementToProvider(bool optionOwnsMonsterRuntimeProfile)
	{
		return optionOwnsMonsterRuntimeProfile;
	}
}
