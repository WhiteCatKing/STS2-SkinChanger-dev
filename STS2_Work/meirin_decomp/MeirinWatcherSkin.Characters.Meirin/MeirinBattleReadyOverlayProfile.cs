using System.Collections.Generic;
using CznCore.Presentation;
using Godot;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinBattleReadyOverlayProfile
{
	private const string ScenePath = "res://MeirinWatcherSkin/scenes/battle_ready/meirin_battle_ready.tscn";

	private const string SkeletonDataPath = "res://MeirinWatcherSkin/animations/battle_ready/meirin_battle_ready_skel_data.tres";

	private const string IdleAnimation = "b_idle";

	private const string AttackAnimation = "card_attack";

	private const string CastingAnimation = "card_casting";

	private static readonly BattleReadyOverlayMotionAdapter MotionAdapter = BattleReadyOverlayMotionAdapters.CznSts2FallbackSlide();

	public static readonly BattleReadyOverlayProfile Profile;

	static MeirinBattleReadyOverlayProfile()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		BattleReadyOverlayProfile val = new BattleReadyOverlayProfile("MeirinBattleReady", "res://MeirinWatcherSkin/scenes/battle_ready/meirin_battle_ready.tscn", "res://MeirinWatcherSkin/animations/battle_ready/meirin_battle_ready_skel_data.tres", new BattleReadyOverlayAnimations(string.Empty, "b_idle", string.Empty, "card_attack", "card_casting"), MotionAdapter.OutDelaySeconds, MotionAdapter.CancelOutDelaySeconds, MotionAdapter.SlideOffsetX, MotionAdapter.SlideDurationSeconds, (BattleReadyOverlayIdleEffectPlan)null, (IReadOnlyDictionary<CombatPresentationCardRoute, string>)new Dictionary<CombatPresentationCardRoute, string>
		{
			[(CombatPresentationCardRoute)1] = "card_attack",
			[(CombatPresentationCardRoute)2] = "card_casting",
			[(CombatPresentationCardRoute)3] = "card_casting",
			[(CombatPresentationCardRoute)4] = "card_attack",
			[(CombatPresentationCardRoute)5] = "card_casting",
			[(CombatPresentationCardRoute)6] = "card_casting",
			[(CombatPresentationCardRoute)0] = "card_casting"
		}, -1f, MotionAdapter.CardUseSlideDurationSeconds);
		val.set_LayoutOffset(new Vector2(0f, 56f));
		Profile = val;
	}
}
