namespace STS2SkinChanger.Core;

internal sealed record WorkshopVerifiedSubmission(WorkshopCatalogItem Item, string Name, string Group)
{
	public int? Reply { get; init; }
}
