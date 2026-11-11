namespace STS2SkinChanger.Core;

internal sealed record CardSkinPresetState(string Name, bool Active, string? BundleName = null)
{
	public string DisplayName => BundleName ?? Name;

	public bool IsBundlePreset => BundleName != null;
}
