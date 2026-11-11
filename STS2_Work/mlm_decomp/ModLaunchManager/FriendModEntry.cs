using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class FriendModEntry
{
	[JsonPropertyName("i")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("n")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Name { get; set; }

	[JsonPropertyName("v")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Version { get; set; } = string.Empty;

	[JsonPropertyName("w")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public ulong WorkshopId { get; set; }

	[JsonPropertyName("s")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Source { get; set; } = string.Empty;

	[JsonPropertyName("g")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool AffectsGameplay { get; set; }

	[JsonPropertyName("d")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Dependencies { get; set; }

	[JsonPropertyName("h")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string ContentHash { get; set; } = string.Empty;

	[JsonIgnore]
	internal string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Name))
			{
				return Name;
			}
			return Id;
		}
	}
}
