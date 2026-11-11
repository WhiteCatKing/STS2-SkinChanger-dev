using System;
using CznCore.Settings;

namespace MeirinWatcherSkin;

internal static class SettingsLocalization
{
	private const string SettingsLocalizationPathFormat = "res://MeirinWatcherSkin/localization/{0}/settings.json";

	private static readonly SettingsLocalizationStore Store = new SettingsLocalizationStore("res://MeirinWatcherSkin/localization/{0}/settings.json", (Action<string, int>)SkinLog.Warn);

	public static SettingsLocalizedText Text(string key, string fallback)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		return new SettingsLocalizedText(Store, key, fallback);
	}
}
