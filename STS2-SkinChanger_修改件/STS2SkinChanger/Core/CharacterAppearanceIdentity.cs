using Godot;

namespace STS2SkinChanger.Core;

// One identity for dropdown/background/preview/select icon/run icon/map/combat/rest/shop.
// Variants and manual state remain part of the key; an assembly/provider ID alone is insufficient.
internal readonly record struct CharacterAppearanceIdentity(string Group, string Selection, string Key)
{
    internal bool IsCurrent => this == SkinService.CaptureCharacterAppearance(Group);
    internal void Bind(GodotObject target)
    {
        if (!IsCurrent) throw new AppearanceResourceUnavailableException("过期的角色外观结果已丢弃。");
        target.SetMeta("sc_character_binding", Key);
        target.SetMeta("sc_character_selection", Selection);
    }
}

internal static partial class SkinService
{
    internal static CharacterAppearanceIdentity CaptureCharacterAppearance(string group)
    {
        lock (Sync)
        {
            var selection = GetVisualSelection(group);
            return new(group, selection, RuntimeResourceKey(group, selection, "__character_binding__"));
        }
    }
}
