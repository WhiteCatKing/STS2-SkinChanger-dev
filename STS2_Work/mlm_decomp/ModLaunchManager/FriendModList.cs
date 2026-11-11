using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class FriendModList
{
	[JsonPropertyName("p")]
	public int ProtocolVersion { get; set; } = 1;

	[JsonPropertyName("g")]
	public string GameVersion { get; set; } = string.Empty;

	[JsonPropertyName("l")]
	public string ManagerVersion { get; set; } = string.Empty;

	[JsonPropertyName("m")]
	public List<FriendModEntry> Mods { get; set; } = new List<FriendModEntry>();
}
