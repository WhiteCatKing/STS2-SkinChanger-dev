using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record SessionCharacterSelection(string CharacterId, string GroupId, string OptionId, IReadOnlyDictionary<string, string> SelectionOverrides, IReadOnlyDictionary<string, CharacterCombatTransform> Transforms, bool OwnerAppearanceLoaded);
