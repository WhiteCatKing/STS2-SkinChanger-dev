namespace STS2SkinChanger.Catalog;

internal sealed record CardCatalogEntry(string TypeName, string PortraitPath, string PoolGroupId, string CatalogGroupId, string FilterGroupId)
{
	public bool IsCharacterPool { get; init; }
}
