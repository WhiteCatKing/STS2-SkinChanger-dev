namespace STS2SkinChanger.Core;

internal sealed record WorkshopScanResult(WorkshopCatalogItem? Item, string Name, string Error, string ProviderId = "", string ProviderName = "");
