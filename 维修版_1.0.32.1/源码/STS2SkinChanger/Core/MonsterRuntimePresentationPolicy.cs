namespace STS2SkinChanger.Core;

/// <summary>
/// The presentation entry point a provider patch can own. Providers that ship one runtime
/// replacement profile per monster draw the enemy themselves instead of remapping a canonical
/// packed scene, so their visual-creation callback is the drawing step and not a duplicate of
/// Skin Changer's resource selection.
/// </summary>
internal enum MonsterVisualCallback
{
    None,
    MonsterVisualCreation,
    CreatureReady,
    BossMapPointReady,
    OstyPositioning
}

/// <summary>
/// Decides which side owns one monster's presentation for providers that expose a per-monster
/// runtime profile (for example a pack that attaches its own skeleton to the vanilla host scene).
/// Such a provider keeps its own visual callbacks while it is selected, and Skin Changer must
/// leave the created result alone; replacing it with the canonical scene puts the vanilla
/// monster back and makes the whole enemy pack look like it does nothing.
/// </summary>
internal static class MonsterRuntimePresentationPolicy
{
    public static bool ProviderKeepsVisualCallback(
        bool providerOwnsPerMonsterProfiles,
        MonsterVisualCallback callback) =>
        providerOwnsPerMonsterProfiles &&
        callback is MonsterVisualCallback.MonsterVisualCreation or
            MonsterVisualCallback.CreatureReady or
            MonsterVisualCallback.BossMapPointReady or
            MonsterVisualCallback.OstyPositioning;

    /// <summary>
    /// A selection is provider-owned when the provider constructs the replacement body from its
    /// own skeleton resources at runtime. Skin Changer then only mounts the resources for it.
    /// </summary>
    public static bool DefersVisualReplacementToProvider(bool optionOwnsMonsterRuntimeProfile) =>
        optionOwnsMonsterRuntimeProfile;
}
