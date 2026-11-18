namespace STS2SkinChanger.Core;

/// <summary>
/// Copies the monster section of a skin bundle (content mode + Mod priority: enabled flags and
/// order) to other heroes' bundles. Merchant skins are classified as monsters in this Mod, which
/// is why setting them up per hero means repeating the same list everywhere.
/// Only the monster fields are touched: the character skin, the card section and the preset names
/// of the target bundles stay exactly as they were.
/// </summary>
internal sealed record MonsterSkinSettingsCopy(
    BundleContentMode Mode,
    IReadOnlyList<BundleModPriorityEntry> Priority);

internal static class MonsterSettingsCopyPolicy
{
    internal static MonsterSkinSettingsCopy Capture(CharacterSkinBundle source) =>
        new(source.MonsterMode, source.MonsterModPriority.Select(entry => entry with { }).ToArray());

    /// <summary>Overwrites the target's monster settings with an independent copy of the source.</summary>
    internal static void Apply(CharacterSkinBundle target, MonsterSkinSettingsCopy settings)
    {
        target.MonsterMode = settings.Mode;
        target.MonsterModPriority = settings.Priority.Select(entry => entry with { }).ToList();
    }

    /// <summary>
    /// Applies the settings to every provided bundle and returns how many were changed. Heroes
    /// without a skin bundle are simply not in the sequence: no bundle is invented for them, so a
    /// copy never adds a <c>[P]</c> option to a hero the player has not configured.
    /// </summary>
    internal static int ApplyToAll(IEnumerable<CharacterSkinBundle> targets, MonsterSkinSettingsCopy settings)
    {
        var changed = 0;
        foreach (var target in targets)
        {
            Apply(target, settings);
            changed++;
        }

        return changed;
    }
}
