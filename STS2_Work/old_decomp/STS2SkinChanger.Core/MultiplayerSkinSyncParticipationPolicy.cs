namespace STS2SkinChanger.Core;

internal static class MultiplayerSkinSyncParticipationPolicy
{
	public static MultiplayerSkinSyncParticipationDecision Resolve(bool sendChanges, bool receiveChanges, bool isMultiplayer)
	{
		bool num = (sendChanges || receiveChanges) && isMultiplayer;
		return new MultiplayerSkinSyncParticipationDecision(num, num, num, receiveChanges && isMultiplayer, sendChanges && isMultiplayer);
	}
}
