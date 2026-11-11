using System.Collections.Generic;
using CznCore.Presentation;
using Godot;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinCharacterSelectPresentationProfile
{
	public static readonly CharacterSelectPresentationProfile Profile = new CharacterSelectPresentationProfile("MeirinLive", "AnimatedBg", "MeirinCustomBg", "MeirinBgBtn", "res://scenes/screens/char_select/meirin_static_portrait_bg.tscn", "res://scenes/screens/char_select/meirin_live_bg.tscn", "lobby", new Vector2(184f, 48f), new Vector2(150f, 842f), (IReadOnlyList<CharacterSelectBackgroundPresentationProfile>)new _003C_003Ez__ReadOnlyArray<CharacterSelectBackgroundPresentationProfile>((CharacterSelectBackgroundPresentationProfile[])(object)new CharacterSelectBackgroundPresentationProfile[3]
	{
		new CharacterSelectBackgroundPresentationProfile((CharacterSelectBackgroundVariant)0, "res://scenes/screens/char_select/meirin_static_portrait_bg.tscn", false),
		new CharacterSelectBackgroundPresentationProfile((CharacterSelectBackgroundVariant)2, "res://scenes/screens/char_select/meirin_dynamic_portrait_bg.tscn", false),
		new CharacterSelectBackgroundPresentationProfile((CharacterSelectBackgroundVariant)1, "res://scenes/screens/char_select/meirin_live_bg.tscn", true)
	}), "res://CznCore/images/ui/icon_large_battle_ui_48.png", "res://CznCore/images/ui/icon_contents_lobby_mode_live_event_ui_48.png", "res://MeirinWatcherSkin/images/ui/character_icon_meirin.png", "res://MeirinWatcherSkin/images/ui/map_marker_meirin.png", "res://MeirinWatcherSkin/images/character_select/meirin_static_portrait.png", "bgm/story_memorial");
}
