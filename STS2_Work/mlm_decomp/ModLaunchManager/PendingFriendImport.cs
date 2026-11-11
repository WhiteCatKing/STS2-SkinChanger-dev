using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class PendingFriendImport
{
	[JsonPropertyName("version")]
	public int Version { get; set; } = 1;

	[JsonPropertyName("steamId")]
	public ulong SteamId { get; set; }

	[JsonPropertyName("friend")]
	public string FriendName { get; set; } = string.Empty;

	[JsonPropertyName("createdUtc")]
	public string CreatedUtc { get; set; } = string.Empty;

	[JsonPropertyName("list")]
	public FriendModList List { get; set; } = new FriendModList();

	[JsonPropertyName("gameplayOnly")]
	public bool GameplayOnly { get; set; }
}
