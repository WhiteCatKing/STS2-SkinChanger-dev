using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModOrganizationEntry
{
	[JsonPropertyName("alias")]
	public string Alias { get; set; } = string.Empty;

	[JsonPropertyName("group")]
	public string Group { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("ignorePriority")]
	public bool IgnorePriority { get; set; }
}
