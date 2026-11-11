using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record MonsterSkinPresetSnapshot(string CategoryId, IReadOnlyList<MonsterSkinPresetPriorityState> Priority, IReadOnlyDictionary<string, string> Selections);
