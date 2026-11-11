using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class WorkshopChangeNoteRecord
{
	[JsonPropertyName("localTime")]
	public uint LocalTime { get; set; }

	[JsonPropertyName("seenThrough")]
	public uint SeenThrough { get; set; }

	[JsonPropertyName("notes")]
	public List<WorkshopChangeNoteRecordEntry> Notes { get; set; } = new List<WorkshopChangeNoteRecordEntry>();
}
