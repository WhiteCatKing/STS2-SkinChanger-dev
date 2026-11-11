using CznCore.Presentation;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinVoiceProfile
{
	public static readonly CharacterVoiceProfile Profile;

	static MeirinVoiceProfile()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		CharacterVoiceProfile val = new CharacterVoiceProfile(MeirinCombatPresentationAssets.PrewarmVoicePaths, "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_enter_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_success_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_hit_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_lose_01.ogg");
		val.set_ReadyIdlePaths(MeirinCombatPresentationAssets.ReadyIdleVoiceCandidates);
		val.set_GrowthPaths(MeirinCombatPresentationAssets.GrowthVoiceCandidates);
		val.set_TreasureOpenPaths(MeirinCombatPresentationAssets.GrowthVoiceCandidates);
		val.set_PotionUseSelfPaths(MeirinCombatPresentationAssets.PotionUseSelfVoiceCandidates);
		val.set_PotionUseEnemyPaths(MeirinCombatPresentationAssets.PotionUseEnemyVoiceCandidates);
		val.set_TeammateSupportReceivedPaths(MeirinCombatPresentationAssets.TeammateSupportReceivedVoiceCandidates);
		val.set_StressPaths(MeirinCombatPresentationAssets.StressVoiceCandidates);
		val.set_CurseCardPlayedCue(new CharacterCurseCardCueProfile(MeirinCombatPresentationAssets.SkillCollapseVoiceCandidates, "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_collapse.ogg", "meirin.combat.cutin.collapse_curse", 1f));
		val.set_RestSiteHealPath("res://MeirinWatcherSkin/audio/voice/vo_1027_small_talk_01.ogg");
		Profile = val;
	}
}
