namespace STS2SkinChanger.Core;

internal readonly record struct MultiplayerSkinSyncParticipationDecision(
    bool AttachTransport,
    bool WriteCapabilityTrailer,
    bool ReadCapabilityTrailer,
    bool ApplyRemoteAppearance,
    bool SendLocalAppearance);

internal static class MultiplayerSkinSyncParticipationPolicy
{
    // The transport and the capability trailer are always used in a multiplayer session: this
    // client still announces its own appearance (that is what every mode asks for), and the
    // peers' announcements are what the shared/mine modes disagree about applying.
    public static MultiplayerSkinSyncParticipationDecision Resolve(
        MultiplayerSkinMode mode,
        bool isMultiplayer) =>
        new(
            AttachTransport: isMultiplayer,
            WriteCapabilityTrailer: isMultiplayer,
            ReadCapabilityTrailer: isMultiplayer,
            ApplyRemoteAppearance: MultiplayerSkinModePolicy.AppliesRemoteAppearance(mode, isMultiplayer),
            SendLocalAppearance: MultiplayerSkinModePolicy.SendsLocalAppearance(mode, isMultiplayer));
}
