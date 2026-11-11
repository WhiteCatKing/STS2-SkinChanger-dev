namespace STS2SkinChanger.Catalog;

internal sealed record SkinModDescriptor(string Id, string Name, string? PckPath, bool AffectsGameplay, string? RootPath = null, bool HasDll = false, string? ManifestId = null)
{
	public string ResourceNamespaceId => ManifestId ?? Id;
}
