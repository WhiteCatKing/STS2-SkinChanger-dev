namespace STS2SkinChanger.Core;

internal sealed record MonsterSkinPresetState(string Name, bool Active, string? BundleName = null)
{
	public string DisplayName => BundleName ?? Name;

	public bool IsBundlePreset => BundleName != null;
}
