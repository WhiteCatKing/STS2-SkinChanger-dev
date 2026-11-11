using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class CanonicalLaunchDocument
{
	[JsonPropertyName("version")]
	public int Version { get; set; } = 1;

	[JsonPropertyName("game_version")]
	public string? GameVersion { get; set; }

	[JsonPropertyName("written_utc")]
	public DateTimeOffset WrittenUtc { get; set; }

	[JsonPropertyName("entries")]
	public List<CanonicalLaunchEntry> Entries { get; set; } = new List<CanonicalLaunchEntry>();
}
