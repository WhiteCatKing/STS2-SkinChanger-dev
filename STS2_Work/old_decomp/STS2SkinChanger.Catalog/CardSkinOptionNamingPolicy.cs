namespace STS2SkinChanger.Catalog;

internal static class CardSkinOptionNamingPolicy
{
	public static string Build(string modName, string? namedVariant, int ordinal, int optionCount)
	{
		if (!string.IsNullOrWhiteSpace(namedVariant))
		{
			return modName + " · " + namedVariant;
		}
		if (optionCount <= 1)
		{
			return modName;
		}
		return modName + " · " + ordinal;
	}
}
