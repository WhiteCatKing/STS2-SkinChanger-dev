using System;

namespace STS2SkinChanger.Catalog;

internal sealed record CharacterVisualPatchCacheEntry(long Length, DateTime LastWriteTimeUtc, bool ReplacesCharacterVisuals);
