using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record SkinPresetCategory(string Id, string DisplayName, IReadOnlyList<string> PresetNames);
