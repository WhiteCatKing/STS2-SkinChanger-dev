using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class MonsterSkinPreset
{
	public bool AllOriginal { get; set; }

	public string Name { get; set; } = string.Empty;

	public string CategoryId { get; set; } = string.Empty;

	public List<MonsterSkinPriorityEntry> Priority { get; set; } = new List<MonsterSkinPriorityEntry>();

	public Dictionary<string, string> Selections { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public List<string> FollowingGroupIds { get; set; } = new List<string>();

	internal MonsterSkinPreset Clone()
	{
		return new MonsterSkinPreset
		{
			Name = Name,
			CategoryId = CategoryId,
			AllOriginal = AllOriginal,
			Priority = Priority.ToList(),
			Selections = new Dictionary<string, string>(Selections, StringComparer.OrdinalIgnoreCase),
			FollowingGroupIds = FollowingGroupIds.ToList()
		};
	}
}
