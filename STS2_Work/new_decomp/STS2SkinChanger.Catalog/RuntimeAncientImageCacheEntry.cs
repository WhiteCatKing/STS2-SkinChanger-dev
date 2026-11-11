using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record RuntimeAncientImageCacheEntry(DateTime RootWriteTimeUtc, IReadOnlyList<RuntimeAncientImage> Images);
