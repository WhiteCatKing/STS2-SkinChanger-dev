using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record ResolvedCharacterSkinComposition<T>(IReadOnlyList<string> SourceOptionIds, IReadOnlyDictionary<string, T> Assets, string? DynamicSourceId);
