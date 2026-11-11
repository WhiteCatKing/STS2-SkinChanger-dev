using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class ActPresentationCatalog
{
	private readonly IReadOnlyCollection<ActPresentationProfile> _profiles;

	private readonly IReadOnlyDictionary<string, ActPresentationProfile> _profilesById;

	private readonly IReadOnlyDictionary<(string ActId, ActRoomScope RoomScope), ActPresentationProfile> _profilesByTarget;

	public IReadOnlyCollection<ActPresentationProfile> Profiles => _profiles;

	public ActPresentationCatalog(IEnumerable<ActPresentationProfile> profiles)
	{
		ActPresentationProfile[] array = profiles?.ToArray() ?? throw new ArgumentNullException("profiles");
		if (array.Length == 0)
		{
			throw new ArgumentException("At least one act presentation profile is required.", "profiles");
		}
		Dictionary<string, ActPresentationProfile> dictionary = new Dictionary<string, ActPresentationProfile>(StringComparer.Ordinal);
		Dictionary<(string, ActRoomScope), ActPresentationProfile> dictionary2 = new Dictionary<(string, ActRoomScope), ActPresentationProfile>();
		ActPresentationProfile[] array2 = array;
		foreach (ActPresentationProfile actPresentationProfile in array2)
		{
			if (!dictionary.TryAdd(actPresentationProfile.Id, actPresentationProfile))
			{
				throw new InvalidOperationException("Duplicate act presentation profile id '" + actPresentationProfile.Id + "'.");
			}
			(string, ActRoomScope) key = (actPresentationProfile.Target.ActId, actPresentationProfile.Target.RoomScope);
			if (!dictionary2.TryAdd(key, actPresentationProfile))
			{
				throw new InvalidOperationException("Duplicate act presentation target " + $"'{key.Item1}/{key.Item2}'.");
			}
		}
		_profiles = (IReadOnlyCollection<ActPresentationProfile>)(object)array;
		_profilesById = dictionary;
		_profilesByTarget = dictionary2;
	}

	public bool TryGetByTarget(string actId, ActRoomScope roomScope, out ActPresentationProfile profile)
	{
		return _profilesByTarget.TryGetValue((actId, roomScope), out profile);
	}
}
