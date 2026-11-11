using System.Collections.Generic;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinCombatPresentationAssets
{
	public const string BodyTexturePath = "res://MeirinWatcherSkin/animations/body/meirin_body.png";

	public const string BodySkeletonDataPath = "res://MeirinWatcherSkin/animations/body/meirin_body_skel_data.tres";

	public const string CombatEffectSceneRoot = "res://MeirinWatcherSkin/scenes/combat_effects/";

	private const string VoiceRoot = "res://MeirinWatcherSkin/audio/voice/";

	private const string SfxRoot = "res://MeirinWatcherSkin/audio/sfx/";

	public const string CombatAttackReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_ready.ogg";

	public const string CombatAttackPlay1SfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play1.ogg";

	public const string CombatAttackPlay2SfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play2.ogg";

	public const string CombatStrongAttackReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_strong_attack_ready.ogg";

	public const string CombatStrongAttackPlaySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_strong_attack_play.ogg";

	public const string CombatDefenseReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_defense_ready.ogg";

	public const string CombatSystemBuffShieldSfxPath = "res://CznCore/audio/sfx/system_buff_shield.ogg";

	public const string CombatU1BuffReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u1_buff_ready.ogg";

	public const string CombatU1BuffPlaySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u1_buff_play.ogg";

	public const string CombatU2AttackReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u2_attack_ready.ogg";

	public const string CombatU2AttackPlaySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u2_attack_play.ogg";

	public const string CombatU3BuffReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u3_buff_ready.ogg";

	public const string CombatU3BuffPlaySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u3_buff_play.ogg";

	public const string CombatU4BuffReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u4_buff_ready.ogg";

	public const string CombatDebuffReadySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_debuff_ready.ogg";

	public const string CombatDebuffPlaySfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_debuff_play.ogg";

	public const string CombatUgAttackSfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_ug_attack.ogg";

	public const string CombatUxBuffSfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_ux_buff.ogg";

	public const string CollapseSfxPath = "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_collapse.ogg";

	public const string CharacterSelectGachaVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_gacha_01.ogg";

	public const string CharacterSelectTeamJoinVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_team_join_01.ogg";

	public const string CharacterSelectLobbyVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_lobby_enter_01.ogg";

	public const string StageEnterVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_enter_01.ogg";

	public const string StageSuccessVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_success_01.ogg";

	public const string StageFailVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_fail_01.ogg";

	public const string DeathVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_lose_01.ogg";

	public const string DeathCollapseVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_death_collapse_01.ogg";

	public const string HitVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_hit_01.ogg";

	public const string CampVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_camp_01.ogg";

	public const string RestSiteHealVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_small_talk_01.ogg";

	private const string Buff01VoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_buff_01.ogg";

	private const string Buff02VoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_buff_02.ogg";

	private const string Emotion09VoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_emotion_09.ogg";

	private const string SkillCollapseVoicePath = "res://MeirinWatcherSkin/audio/voice/vo_1027_skill_collapse_01.ogg";

	public static readonly IReadOnlyList<string> AttackVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "res://MeirinWatcherSkin/audio/voice/vo_1027_attack_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_attack_02.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_attack_03.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_attack_04.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_attack_05.ogg" });

	public static readonly IReadOnlyList<string> DefenseVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_defense_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_defense_02.ogg" });

	public static readonly IReadOnlyList<string> BuffVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_buff_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_buff_02.ogg" });

	public static readonly IReadOnlyList<string> PotionUseSelfVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_buff_01.ogg");

	public static readonly IReadOnlyList<string> PotionUseTeammateVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_buff_02.ogg");

	public static readonly IReadOnlyList<string> PotionUseEnemyVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_skill_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_skill_02.ogg" });

	public static readonly IReadOnlyList<string> TeammateSupportReceivedVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_emotion_09.ogg");

	public static readonly IReadOnlyList<string> SkillCollapseVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_skill_collapse_01.ogg");

	public static readonly IReadOnlyList<string> U1BuffVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_u1.ogg");

	public static readonly IReadOnlyList<string> U2AttackVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_u2.ogg");

	public static readonly IReadOnlyList<string> U3BuffVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_u3.ogg");

	public static readonly IReadOnlyList<string> U4BuffVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_u4.ogg");

	public static readonly IReadOnlyList<string> UgAttackVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_ug.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_ug_02.ogg" });

	public static readonly IReadOnlyList<string> UxBuffVoiceCandidates = new _003C_003Ez__ReadOnlySingleElementList<string>("res://MeirinWatcherSkin/audio/voice/vo_1027_ux.ogg");

	public static readonly IReadOnlyList<string> GrowthVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_growth_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_growth_02.ogg" });

	public static readonly IReadOnlyList<string> ReadyIdleVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "res://MeirinWatcherSkin/audio/voice/vo_1027_idle_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_idle_02.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_buff_02.ogg" });

	public static readonly IReadOnlyList<string> StressVoiceCandidates = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://MeirinWatcherSkin/audio/voice/vo_1027_stress_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_stress_02.ogg" });

	public static readonly IReadOnlyList<string> PrewarmVoicePaths = new _003C_003Ez__ReadOnlyArray<string>(new string[19]
	{
		"res://MeirinWatcherSkin/audio/voice/vo_1027_gacha_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_team_join_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_lobby_enter_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_stage_enter_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_stage_success_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_stage_fail_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_lose_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_death_collapse_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_hit_01.ogg",
		"res://MeirinWatcherSkin/audio/voice/vo_1027_camp_01.ogg",
		ReadyIdleVoiceCandidates[0],
		ReadyIdleVoiceCandidates[1],
		ReadyIdleVoiceCandidates[2],
		PotionUseSelfVoiceCandidates[0],
		PotionUseEnemyVoiceCandidates[0],
		PotionUseEnemyVoiceCandidates[1],
		TeammateSupportReceivedVoiceCandidates[0],
		SkillCollapseVoiceCandidates[0],
		"res://MeirinWatcherSkin/audio/voice/vo_1027_small_talk_01.ogg"
	});
}
