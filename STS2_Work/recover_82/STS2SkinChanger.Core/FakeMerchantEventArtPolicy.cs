using System;

namespace STS2SkinChanger.Core;

internal static class FakeMerchantEventArtPolicy
{
	internal const string MonsterGroupId = "fake_merchant_monster";

	internal const string EncounterId = "fake_merchant_event_encounter";

	internal const string EncounterBackgroundScene = "res://scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscn";

	internal const string EventImage = "res://images/events/fake_merchant.png";

	private const string EncounterBackgroundRoot = "res://scenes/backgrounds/fake_merchant_event_encounter/";

	private const string TentRoot = "res://animations/backgrounds/fake_merchant_room/bottom/";

	public static bool IsRedrawnArtPath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		string text = path.Replace('\\', '/');
		if (!text.StartsWith("res://scenes/backgrounds/fake_merchant_event_encounter/", StringComparison.OrdinalIgnoreCase))
		{
			return text.StartsWith("res://animations/backgrounds/fake_merchant_room/bottom/", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static bool SuppressesCanonicalRedirect(string? path)
	{
		return IsRedrawnArtPath(path);
	}
}
