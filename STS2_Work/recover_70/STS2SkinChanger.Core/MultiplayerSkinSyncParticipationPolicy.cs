namespace STS2SkinChanger.Core;

internal static class MultiplayerSkinSyncParticipationPolicy
{
	public static MultiplayerSkinSyncParticipationDecision Resolve(MultiplayerSkinMode mode, bool isMultiplayer)
	{
		return new MultiplayerSkinSyncParticipationDecision(isMultiplayer, isMultiplayer, isMultiplayer, MultiplayerSkinModePolicy.AppliesRemoteAppearance(mode, isMultiplayer), MultiplayerSkinModePolicy.SendsLocalAppearance(mode, isMultiplayer));
	}
}
