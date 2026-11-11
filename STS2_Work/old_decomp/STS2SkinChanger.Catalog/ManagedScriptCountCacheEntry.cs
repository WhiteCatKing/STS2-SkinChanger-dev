using System;

namespace STS2SkinChanger.Catalog;

internal sealed record ManagedScriptCountCacheEntry(long Length, DateTime LastWriteTimeUtc, int Count);
