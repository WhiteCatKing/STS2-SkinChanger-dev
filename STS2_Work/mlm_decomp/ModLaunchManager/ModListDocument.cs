using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModListDocument
{
	internal const int CurrentFormatVersion = 1;

	internal const string DocumentKind = "mod-launch-manager.mod-list";

	[JsonPropertyName("kind")]
	public string Kind { get; set; } = "mod-launch-manager.mod-list";

	[JsonPropertyName("formatVersion")]
	public int FormatVersion { get; set; } = 1;

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("exportedAt")]
	public string ExportedAt { get; set; } = string.Empty;

	[JsonPropertyName("gameVersion")]
	public string GameVersion { get; set; } = string.Empty;

	[JsonPropertyName("managerVersion")]
	public string ManagerVersion { get; set; } = string.Empty;

	[JsonPropertyName("mods")]
	public List<ModListItem> Mods { get; set; } = new List<ModListItem>();
}
