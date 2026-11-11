using System;

namespace CznEnemySkin.Domain;

internal sealed class EncounterBattlefieldProfile
{
	public string Id { get; }

	public Sts2EncounterBattlefieldTarget Target { get; }

	public BattlefieldPresentationProfile Battlefield { get; }

	public bool EnabledByDefault { get; }

	public EncounterBattlefieldProfile(string id, Sts2EncounterBattlefieldTarget target, BattlefieldPresentationProfile battlefield, bool enabledByDefault)
	{
		Id = RequireIdentifier(id, "id");
		Target = target ?? throw new ArgumentNullException("target");
		Battlefield = battlefield ?? throw new ArgumentNullException("battlefield");
		EnabledByDefault = enabledByDefault;
		RequireIdentifier(Target.EncounterId, "EncounterId");
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
