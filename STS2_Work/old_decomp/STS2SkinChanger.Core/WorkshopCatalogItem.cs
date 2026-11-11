namespace STS2SkinChanger.Core;

internal sealed record WorkshopCatalogItem(ulong Id, WorkshopTarget[] Targets, bool RestartRequired = true);
