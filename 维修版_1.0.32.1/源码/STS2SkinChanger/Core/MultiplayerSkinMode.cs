namespace STS2SkinChanger.Core;

/// <summary>
/// What a player sees on the other players' characters in the multiplayer **character-select
/// screen**, plus the fallback a run starts with. A run freezes this choice (and this client's own
/// selections) when it begins, so switching a skin mid-run — by this player or by a teammate —
/// never re-binds what is on screen. Only a manual pick in the in-run Appearance screen changes a
/// teammate's look again.
/// </summary>
internal enum MultiplayerSkinMode
{
    /// <summary>Teammates only ever show the vanilla appearance.</summary>
    Vanilla = 0,

    /// <summary>Teammates show their own skin only when this client also has that skin installed.</summary>
    Shared = 1,

    /// <summary>Every character, teammates included, uses this client's skin selection.</summary>
    Mine = 2
}

internal static class MultiplayerSkinModePolicy
{
    internal const MultiplayerSkinMode Default = MultiplayerSkinMode.Mine;

    internal static MultiplayerSkinMode Normalize(int raw) =>
        Enum.IsDefined(typeof(MultiplayerSkinMode), raw)
            ? (MultiplayerSkinMode)raw
            : Default;

    // The old UI had two independent checkboxes. A player who had switched both off expected the
    // vanilla look, and a player who only switched sending off was still fine with shared skins,
    // so migrate the pair instead of resetting everyone to the default.
    internal static MultiplayerSkinMode FromLegacyToggles(bool sendChanges, bool receiveChanges) =>
        !receiveChanges
            ? MultiplayerSkinMode.Vanilla
            : sendChanges
                ? MultiplayerSkinMode.Mine
                : MultiplayerSkinMode.Shared;

    internal static bool SendsLocalAppearance(MultiplayerSkinMode mode, bool isMultiplayer) =>
        isMultiplayer;

    internal static bool AppliesRemoteAppearance(MultiplayerSkinMode mode, bool isMultiplayer) =>
        isMultiplayer && mode == MultiplayerSkinMode.Shared;

    internal static bool UsesVanillaForRemotePlayers(MultiplayerSkinMode mode) =>
        mode == MultiplayerSkinMode.Vanilla;
}
