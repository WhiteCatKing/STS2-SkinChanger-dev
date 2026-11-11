namespace STS2SkinChanger.Catalog;

internal static class SkinProviderPatchEvidencePolicy
{
	internal static bool IsSkinEvidence(bool replacesCharacterVisualCreation, bool hooksCreatureLifecycle, bool patchesCharacterSelectPresentation)
	{
		if (!replacesCharacterVisualCreation)
		{
			return hooksCreatureLifecycle && patchesCharacterSelectPresentation;
		}
		return true;
	}

	internal static bool BringsOwnAppearance(int packResourceCount, int runtimeImageCount, bool replacesCharacterVisualCreation, bool hasAppearanceFile)
	{
		return packResourceCount > 0 || runtimeImageCount > 0 || replacesCharacterVisualCreation || hasAppearanceFile;
	}
}
