using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed class CharacterSkinBundle
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	internal string PresetKey => "__bundle_preset__:" + Id;

	public bool HideSources { get; set; } = true;

	public string Name { get; set; } = string.Empty;

	public string CharacterGroupId { get; set; } = string.Empty;

	public string CharacterOptionId { get; set; } = "__base__";

	public BundleContentMode CardMode { get; set; }

	public BundleContentMode MonsterMode { get; set; }

	public List<BundleModPriorityEntry> CardModPriority { get; set; } = new List<BundleModPriorityEntry>();

	public List<BundleModPriorityEntry> MonsterModPriority { get; set; } = new List<BundleModPriorityEntry>();

	public Dictionary<string, string> CardPresetNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> MonsterPresetNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
