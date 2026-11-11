using System;
using System.Collections.Generic;
using CznCore.Presentation;
using Godot;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinSupportingProfiles
{
	private const string RestSiteChairSlotName = "chair_(-6,32)";

	public static readonly CharacterSkinCombatEffectsProfile DisabledCombatEffects = new CharacterSkinCombatEffectsProfile(Vector2.One, Vector2.Zero, Colors.White, Colors.White, "Back", "Front", "Back", new Vector2I(720, 720), new Vector2(360f, 520f), new CombatEffectMaterialPolicyProfile(Colors.White, 1f, 1f, 1f, Colors.White, 1f, 1f, 1f, 1f, new CombatEffectViewportMaterialProfile(1f, 1f, 1f, 1f), new CombatEffectViewportMaterialProfile(1f, 1f, 1f, 1f)), 1f);

	public static readonly CharacterSkinSettingsTextProfile SettingsText = new CharacterSkinSettingsTextProfile((ILocalizedText)(object)UiText.MeirinSettingsHeader, (ILocalizedText)(object)UiText.MeirinSkinTarget, (ILocalizedText)(object)UiText.MeirinEffectsEnabled, (ILocalizedText)(object)UiText.MeirinVoiceEnabled);

	public static readonly CombatCardPresentationProfile CardPresentation = new CombatCardPresentationProfile((IReadOnlyList<CombatCardPresentationOverride>)Array.Empty<CombatCardPresentationOverride>(), (IReadOnlyDictionary<CombatPresentationCardRoute, IReadOnlyList<string>>)new Dictionary<CombatPresentationCardRoute, IReadOnlyList<string>>(), (IReadOnlyList<string>)Array.Empty<string>(), (IReadOnlyList<string>)Array.Empty<string>());

	public static readonly RestSitePresentationProfile RestSite = new RestSitePresentationProfile("camping", (IReadOnlyList<string>)new _003C_003Ez__ReadOnlySingleElementList<string>("idle"), "res://MeirinWatcherSkin/audio/voice/vo_1027_camp_01.ogg", 2500, CharacterSkinSurfaceDefaults.CznRestSiteBodyOffset, "meirin_rest_site_body_base_position", (IReadOnlyList<RestSiteCampingLayerProfile>)Array.Empty<RestSiteCampingLayerProfile>(), CharacterSkinSurfaceDefaults.CznRestSiteSurface, (IReadOnlyList<string>)new _003C_003Ez__ReadOnlySingleElementList<string>("chair_(-6,32)"));

	public static readonly DefeatPresentationProfile Defeat = MeirinDefeatPresentationProfile.Profile;
}
