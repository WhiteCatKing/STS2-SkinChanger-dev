using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class WorkshopChangeNoteDocument
{
	[JsonPropertyName("formatVersion")]
	public int FormatVersion { get; set; } = 1;

	[JsonPropertyName("items")]
	public Dictionary<ulong, WorkshopChangeNoteRecord> Items { get; set; } = new Dictionary<ulong, WorkshopChangeNoteRecord>();
}
