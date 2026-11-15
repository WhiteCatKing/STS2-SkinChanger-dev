namespace STS2SkinChanger.Core;

/// <summary>
/// The fake merchant event picture belongs to an event-only pack that is mounted on top of the
/// game's own canonical paths (the tent's Spine layers and the event portrait). Every per-selection
/// runtime pack copies the game's baseline files into private aliases and also writes canonical
/// redirects for them; a redirect for any of these paths wins over the event pack because the
/// runtime pack is mounted later, which is what made the fight draw the vanilla tent while the
/// event right before it showed the redrawn one. The rules below keep the canonical paths of the
/// redrawn art out of those redirects.
/// </summary>
internal static class FakeMerchantEventArtPolicy
{
    /// <summary>Group that owns the fake merchant figure (the tent itself comes from the event).</summary>
    internal const string MonsterGroupId = "fake_merchant_monster";

    internal const string EncounterId = "fake_merchant_event_encounter";

    /// <summary>The encounter's own battle background scene, i.e. the tent the fight happens in.</summary>
    internal const string EncounterBackgroundScene =
        "res://scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscn";

    /// <summary>Full-frame event illustration authored by the event art pack.</summary>
    internal const string EventImage = "res://images/events/fake_merchant.png";

    private const string EncounterBackgroundRoot =
        "res://scenes/backgrounds/fake_merchant_event_encounter/";

    private const string TentRoot = "res://animations/backgrounds/fake_merchant_room/bottom/";

    /// <summary>
    /// True for every resource the fake merchant event art pack redraws: the tent's Spine layers
    /// (bottom room art) and the encounter background scene that plays them.
    /// </summary>
    public static bool IsRedrawnArtPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');
        return normalized.StartsWith(EncounterBackgroundRoot, StringComparison.OrdinalIgnoreCase) ||
               normalized.StartsWith(TentRoot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether a canonical redirect (a <c>.remap</c>/<c>.import</c> bridge written by a runtime
    /// pack) must be skipped for <paramref name="path"/>. The redrawn art has to keep resolving
    /// through the game's canonical paths so the mounted event pack stays visible.
    /// </summary>
    public static bool SuppressesCanonicalRedirect(string? path) => IsRedrawnArtPath(path);
}
