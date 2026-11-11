using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class CanonicalLaunchEntry
{
	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;

	[JsonPropertyName("workshop_item_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? WorkshopItemId { get; set; }

	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	internal CanonicalLaunchEntry Clone()
	{
		return new CanonicalLaunchEntry
		{
			Source = Source,
			Id = Id,
			Path = Path,
			WorkshopItemId = WorkshopItemId,
			Enabled = Enabled
		};
	}
}
