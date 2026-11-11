using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModListItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }
}
