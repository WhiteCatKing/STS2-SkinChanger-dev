using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModOptionDocument
{
	[JsonPropertyName("version")]
	public int Version { get; set; } = 1;

	[JsonPropertyName("mods")]
	public Dictionary<string, ModOptionEntry> Mods { get; set; } = new Dictionary<string, ModOptionEntry>(StringComparer.OrdinalIgnoreCase);
}
