namespace CznCore.Settings;

internal static class SharedSettingsText
{
	public const string GlobalSettingsIconPath = "res://CznCore/images/ui/btn_option.png";

	private static readonly SettingsLocalizationStore Store = new SettingsLocalizationStore("res://CznCore/localization/{0}/settings.json");

	public static readonly SettingsLocalizedText CharacterSettings = Text("character_settings", "Character Settings");

	public static readonly SettingsLocalizedText OpenSection = Text("open_section", "Open");

	public static readonly SettingsLocalizedText Back = Text("back", "Back");

	public static readonly SettingsLocalizedText PresentationSettings = Text("presentation_settings", "Presentation");

	public static readonly SettingsLocalizedText Enabled = Text("state_enabled", "On");

	public static readonly SettingsLocalizedText Disabled = Text("state_disabled", "Off");

	public static readonly SettingsLocalizedText StandaloneCharacter = Text("standalone_character", "Standalone Character");

	public static readonly SettingsLocalizedText EffectsEnabled = Text("effects_enabled", "Combat Effects");

	public static readonly SettingsLocalizedText VoiceEnabled = Text("voice_enabled", "Character Voice");

	public static readonly SettingsLocalizedText SwitchBackground = Text("switch_character_background", "Switch Character Background");

	public static readonly SettingsLocalizedText VoiceDescription = Text("character_voice_description", "Enable or disable this character's voice.");

	public static readonly SettingsLocalizedText OpenOptions = Text("character_options_open", "CZN Options");

	public static readonly SettingsLocalizedText CloseOptions = Text("character_options_close", "Close CZN Options");

	public static readonly SkinSettingsUiTextProfile Ui = SkinSettingsUiTextFactory.Create(Text, Text("combat_sfx_enabled", "Effect Sounds"), Text("voice_volume", "Voice Volume"));

	public static readonly SkinSettingsUiTextRuntime Runtime = new SkinSettingsUiTextRuntime(Ui);

	public static SettingsLocalizedText Text(string key, string fallback)
	{
		return new SettingsLocalizedText(Store, key, fallback);
	}
}
