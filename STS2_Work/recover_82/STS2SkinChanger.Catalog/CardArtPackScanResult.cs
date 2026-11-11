using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record CardArtPackScanResult(IReadOnlyList<CardArtPack> Packs, IReadOnlyList<string> Warnings);
