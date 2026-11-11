using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class LaunchProfile
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("mods")]
	public List<ProfileMod> Mods { get; set; } = new List<ProfileMod>();

	[JsonPropertyName("workshopCollection")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public ulong? SourceCollection { get; set; }
}
