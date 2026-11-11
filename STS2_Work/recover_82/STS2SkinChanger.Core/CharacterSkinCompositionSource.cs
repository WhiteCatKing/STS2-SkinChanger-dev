using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record CharacterSkinCompositionSource<T>(string OptionId, bool HasDynamicBehavior, IReadOnlyDictionary<string, T> Assets);
