using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ManagedCharacterAssetReplacement(string TargetGroupId, IReadOnlyDictionary<string, string> CanonicalPathsByProviderPath);
