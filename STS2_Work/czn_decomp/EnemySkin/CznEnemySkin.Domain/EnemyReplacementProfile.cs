using System;

namespace CznEnemySkin.Domain;

internal sealed class EnemyReplacementProfile
{
	public string Id { get; }

	public Sts2EnemyTarget Target { get; }

	public CznEnemySource Source { get; }

	public MonsterPresentation Monster { get; }

	public BestiaryPresentation Bestiary { get; }

	public bool EnabledByDefault { get; }

	public EnemyReplacementProfile(string id, Sts2EnemyTarget target, CznEnemySource source, MonsterPresentation monster, BestiaryPresentation bestiary, bool enabledByDefault)
	{
		Id = RequireIdentifier(id, "id");
		Target = target ?? throw new ArgumentNullException("target");
		Source = source ?? throw new ArgumentNullException("source");
		Monster = monster ?? throw new ArgumentNullException("monster");
		Bestiary = bestiary ?? throw new ArgumentNullException("bestiary");
		EnabledByDefault = enabledByDefault;
		RequireIdentifier(Target.MonsterId, "MonsterId");
		RequireIdentifier(Source.MonsterId, "MonsterId");
		RequireIdentifier(Source.ModelId, "ModelId");
		if (!RequireIdentifier(Source.OfficialNameKey, "OfficialNameKey").StartsWith("monster@name@", StringComparison.Ordinal))
		{
			throw new ArgumentException("Official enemy name key must start with 'monster@name@'.", "OfficialNameKey");
		}
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
