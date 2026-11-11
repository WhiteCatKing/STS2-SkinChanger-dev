using CznCore.Settings;

namespace MeirinWatcherSkin;

internal static class UiText
{
	public static readonly SettingsLocalizedText SwitchCharacterBackground = SettingsLocalization.Text("switch_character_background", "Switch Character Background");

	public static readonly SettingsLocalizedText MeirinSettingsHeader = SettingsLocalization.Text("meirin_settings_header", "Mei Lin");

	public static readonly SettingsLocalizedText MeirinSkinTarget = SettingsLocalization.Text("meirin_skin_target", "Watcher");

	public static readonly SettingsLocalizedText MeirinEffectsEnabled = SettingsLocalization.Text("meirin_effects_enabled", "Combat Effects");

	public static readonly SettingsLocalizedText MeirinCombatSfxEnabled = SettingsLocalization.Text("meirin_combat_sfx_enabled", "Effect Sounds");

	public static readonly SettingsLocalizedText MeirinVoiceEnabled = SettingsLocalization.Text("meirin_voice_enabled", "Character Voice");

	public static readonly SettingsLocalizedText VoiceVolume = SettingsLocalization.Text("voice_volume", "Voice Volume");

	public static readonly SkinSettingsUiTextProfile SettingsUi = SharedSettingsText.Ui;
}
