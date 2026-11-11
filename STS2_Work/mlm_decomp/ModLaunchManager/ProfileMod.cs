using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ProfileMod
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	[JsonIgnore]
	internal string? PreferredPhysicalKey { get; set; }
}
