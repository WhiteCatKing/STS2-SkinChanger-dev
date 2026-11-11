using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class EnemyReplacementCatalog
{
	private readonly IReadOnlyCollection<EnemyReplacementProfile> _profiles;

	private readonly IReadOnlyDictionary<string, EnemyReplacementProfile> _profilesById;

	private readonly IReadOnlyDictionary<string, EnemyReplacementProfile> _profilesByMonsterId;

	public IReadOnlyCollection<EnemyReplacementProfile> Profiles => _profiles;

	public EnemyReplacementCatalog(IEnumerable<EnemyReplacementProfile> profiles)
	{
		EnemyReplacementProfile[] array = profiles?.ToArray() ?? throw new ArgumentNullException("profiles");
		if (array.Length == 0)
		{
			throw new ArgumentException("At least one enemy replacement profile is required.", "profiles");
		}
		_profiles = (IReadOnlyCollection<EnemyReplacementProfile>)(object)array;
		_profilesById = BuildUniqueIndex(array, (EnemyReplacementProfile profile) => profile.Id, "profile id");
		_profilesByMonsterId = BuildUniqueIndex(array, (EnemyReplacementProfile profile) => profile.Target.MonsterId, "target monster id");
	}

	public bool TryGetById(string profileId, out EnemyReplacementProfile profile)
	{
		return _profilesById.TryGetValue(profileId, out profile);
	}

	public bool TryGetByMonsterId(string monsterId, out EnemyReplacementProfile profile)
	{
		return _profilesByMonsterId.TryGetValue(monsterId, out profile);
	}

	private static IReadOnlyDictionary<string, EnemyReplacementProfile> BuildUniqueIndex(IEnumerable<EnemyReplacementProfile> profiles, Func<EnemyReplacementProfile, string> keySelector, string keyDescription)
	{
		Dictionary<string, EnemyReplacementProfile> dictionary = new Dictionary<string, EnemyReplacementProfile>(StringComparer.Ordinal);
		foreach (EnemyReplacementProfile profile in profiles)
		{
			string text = keySelector(profile);
			if (!dictionary.TryAdd(text, profile))
			{
				throw new InvalidOperationException($"Duplicate {keyDescription} '{text}' in enemy replacement catalog.");
			}
		}
		return dictionary;
	}
}
