namespace STS2SkinChanger.Core;

internal readonly record struct MultiplayerSkinSyncParticipationDecision(bool AttachTransport, bool WriteCapabilityTrailer, bool ReadCapabilityTrailer, bool ApplyRemoteAppearance, bool SendLocalAppearance);
