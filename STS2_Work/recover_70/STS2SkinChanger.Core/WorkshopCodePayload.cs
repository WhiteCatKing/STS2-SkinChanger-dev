namespace STS2SkinChanger.Core;

internal sealed record WorkshopCodePayload(int Format, uint App, string Scanner, string Game, WorkshopCatalogItem Item);
