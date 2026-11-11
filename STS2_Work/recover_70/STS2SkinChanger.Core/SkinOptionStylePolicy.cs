namespace STS2SkinChanger.Core;

internal static class SkinOptionStylePolicy
{
	public static bool IsAccented(string optionId)
	{
		bool flag;
		switch (optionId)
		{
		case "__inherit__":
		case "__monster_category__":
		case "__event_category__":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		string name;
		if (!flag && !(optionId == "__base__") && !(optionId == "__mod_default__") && WorkshopCatalogPolicy.IsSkinChoice(optionId) && !RandomCharacterSkinPolicy.IsRandom(optionId))
		{
			return CharacterSkinBundlePolicy.TryGetSelectionBundleName(optionId, out name);
		}
		return true;
	}
}
