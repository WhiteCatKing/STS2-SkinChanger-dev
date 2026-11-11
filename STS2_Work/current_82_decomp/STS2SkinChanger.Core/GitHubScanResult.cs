namespace STS2SkinChanger.Core;

internal readonly record struct GitHubScanResult(WorkshopCatalogItem? Item, string[] Codes, string Error);
