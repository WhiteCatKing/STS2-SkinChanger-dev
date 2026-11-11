using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class WorkshopChangeNoteRecordEntry
{
	[JsonPropertyName("time")]
	public uint Time { get; set; }

	[JsonPropertyName("text")]
	public string Text { get; set; } = string.Empty;

	[JsonPropertyName("branches")]
	public List<string> Branches { get; set; } = new List<string>();

	[JsonPropertyName("translations")]
	public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
