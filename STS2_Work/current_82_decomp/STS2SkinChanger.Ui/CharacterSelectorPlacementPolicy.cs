namespace STS2SkinChanger.Ui;

internal static class CharacterSelectorPlacementPolicy
{
	internal static CharacterSelectorPlacement Resolve(bool useTopRight)
	{
		if (!useTopRight)
		{
			return new CharacterSelectorPlacement(CharacterSelectorHost.InfoPanel, 0.5f, 0f, 0.5f, 0f, -226f, -80f, 226f, -36f);
		}
		return new CharacterSelectorPlacement(CharacterSelectorHost.Screen, 1f, 0f, 1f, 0f, -500f, 92f, -48f, 136f);
	}
}
