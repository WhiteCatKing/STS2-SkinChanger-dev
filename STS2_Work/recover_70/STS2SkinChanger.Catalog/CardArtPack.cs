using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record CardArtPack(string Id, string Name, string Path, string Root, IReadOnlyList<CardArtPackCard> Cards);
