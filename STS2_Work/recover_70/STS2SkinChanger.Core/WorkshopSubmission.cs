namespace STS2SkinChanger.Core;

internal sealed record WorkshopSubmission(int Format, uint App, string Scanner, string Game, WorkshopCatalogItem[] Items);
