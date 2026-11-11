using System;
using System.Collections.Generic;
using CznCore.Presentation;
using Godot;
using WatcherMod;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinSkinProfile
{
	public static readonly CharacterSkinProfile Profile;

	static MeirinSkinProfile()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Expected O, but got Unknown
		//IL_02c0: Expected O, but got Unknown
		//IL_02c0: Expected O, but got Unknown
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected O, but got Unknown
		Type? typeFromHandle = typeof(Watcher);
		Vector2 cznCombatPositionOffset = CharacterSkinSurfaceDefaults.CznCombatPositionOffset;
		CharacterSkinCombatBodyProfile val = new CharacterSkinCombatBodyProfile("idle", "b_idle", "b_idle_to_idle", new string[3] { "enter_ready", "enter_play", "enter_end" }, 0f, 0f, (Vector2[])(object)new Vector2[3]
		{
			new Vector2(-570f, -3.5f),
			new Vector2(-27.5f, 4.5f),
			Vector2.Zero
		}, new string[2] { "victory_ready", "victory" }, "victory", new string[2] { "death_ready", "death" }, 1f, 1f, (IReadOnlyDictionary<string, CharacterSkinRemoteNativeBodyAnimation>)new Dictionary<string, CharacterSkinRemoteNativeBodyAnimation>(StringComparer.OrdinalIgnoreCase)
		{
			["Idle"] = new CharacterSkinRemoteNativeBodyAnimation("b_idle", true, true, (IReadOnlyList<string>)null),
			["Relaxed"] = new CharacterSkinRemoteNativeBodyAnimation("idle", true, true, (IReadOnlyList<string>)null),
			["Attack"] = new CharacterSkinRemoteNativeBodyAnimation("attack_ready", false, true, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "attack_ready", "attack_play1", "attack_end" })),
			["heavyAttack"] = new CharacterSkinRemoteNativeBodyAnimation("f_strong_attack_ready", false, true, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "f_strong_attack_ready", "f_strong_attack_play", "f_strong_attack_end" })),
			["Cast"] = new CharacterSkinRemoteNativeBodyAnimation("defense_ready", false, true, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "defense_ready", "defense_play" })),
			["PowerUp"] = new CharacterSkinRemoteNativeBodyAnimation("buff_ready", false, true, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "buff_ready", "buff_play" })),
			["Hit"] = new CharacterSkinRemoteNativeBodyAnimation("hit", false, true, (IReadOnlyList<string>)null),
			["Dead"] = new CharacterSkinRemoteNativeBodyAnimation("death_ready", false, false, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "death_ready", "death" }))
		}, (IReadOnlyList<string>)Array.Empty<string>());
		val.set_AtomicPoseTransitions(true);
		Profile = new CharacterSkinProfile("meirin", "[MeirinWatcherSkin]", "Meirin", typeFromHandle, "res://MeirinWatcherSkin/animations/body/meirin_body.png", "res://MeirinWatcherSkin/animations/body/meirin_body_skel_data.tres", "MeirinWatcherSprite", 4.55f, cznCombatPositionOffset, val, new CharacterSkinFormVfxProfile(string.Empty, string.Empty, string.Empty), "res://MeirinWatcherSkin/materials/meirin_premultiplied_alpha.tres", string.Empty, MeirinSupportingProfiles.DisabledCombatEffects, new CharacterSkinFeatureSupportProfile(true, false, true, true, false, true, true, true, true, true), MeirinSupportingProfiles.SettingsText, MeirinSupportingProfiles.CardPresentation, (ICombatPresentationProfile)(object)new MeirinCombatPresentationProfile(MeirinSupportingProfiles.CardPresentation), MeirinBattleReadyOverlayProfile.Profile, MeirinCharacterIdentityProfile.CharacterSelect, MeirinCharacterSelectPresentationProfile.Profile, MeirinSupportingProfiles.RestSite, MeirinSupportingProfiles.Defeat, MeirinVoiceProfile.Profile, "res://CznCore/images/ui/btn_option.png", (Func<Type>)null);
	}
}
