using System;

namespace CznEnemySkin.Domain;

internal sealed class ActPresentationProfile
{
	public string Id { get; }

	public string SettingId { get; }

	public Sts2ActTarget Target { get; }

	public BattlefieldPresentationProfile Battlefield { get; }

	public bool EnabledByDefault { get; }

	public ActPresentationProfile(string id, Sts2ActTarget target, BattlefieldPresentationProfile battlefield, bool enabledByDefault, string? settingId = null)
	{
		Id = RequireIdentifier(id, "id");
		SettingId = RequireIdentifier(settingId ?? id, "settingId");
		Target = target ?? throw new ArgumentNullException("target");
		Battlefield = battlefield ?? throw new ArgumentNullException("battlefield");
		EnabledByDefault = enabledByDefault;
		RequireIdentifier(Target.ActId, "ActId");
	}

	private static string RequireIdentifier(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Identifier cannot be empty.", parameterName);
		}
		return value.Trim();
	}
}
