using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ProfileDocument
{
	[JsonPropertyName("version")]
	public int Version { get; set; } = 1;

	[JsonPropertyName("active")]
	public string? Active { get; set; }

	[JsonPropertyName("profiles")]
	public List<LaunchProfile> Profiles { get; set; } = new List<LaunchProfile>();
}
