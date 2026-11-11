using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class EncounterBattlefieldCatalog
{
	private readonly IReadOnlyCollection<EncounterBattlefieldProfile> _profiles;

	private readonly IReadOnlyDictionary<string, EncounterBattlefieldProfile> _profilesByTarget;

	public IReadOnlyCollection<EncounterBattlefieldProfile> Profiles => _profiles;

	public EncounterBattlefieldCatalog(IEnumerable<EncounterBattlefieldProfile> profiles)
	{
		EncounterBattlefieldProfile[] array = profiles?.ToArray() ?? throw new ArgumentNullException("profiles");
		if (array.Length == 0)
		{
			throw new ArgumentException("At least one encounter battlefield profile is required.", "profiles");
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		Dictionary<string, EncounterBattlefieldProfile> dictionary = new Dictionary<string, EncounterBattlefieldProfile>(StringComparer.Ordinal);
		EncounterBattlefieldProfile[] array2 = array;
		foreach (EncounterBattlefieldProfile encounterBattlefieldProfile in array2)
		{
			if (!hashSet.Add(encounterBattlefieldProfile.Id))
			{
				throw new InvalidOperationException("Duplicate encounter battlefield profile id '" + encounterBattlefieldProfile.Id + "'.");
			}
			if (!dictionary.TryAdd(encounterBattlefieldProfile.Target.EncounterId, encounterBattlefieldProfile))
			{
				throw new InvalidOperationException("Duplicate encounter battlefield target '" + encounterBattlefieldProfile.Target.EncounterId + "'.");
			}
		}
		_profiles = (IReadOnlyCollection<EncounterBattlefieldProfile>)(object)array;
		_profilesByTarget = dictionary;
	}

	public bool TryGetByTarget(string encounterId, out EncounterBattlefieldProfile profile)
	{
		return _profilesByTarget.TryGetValue(encounterId, out profile);
	}
}
