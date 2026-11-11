using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModOptionEntry
{
	[JsonPropertyName("values")]
	public Dictionary<string, JsonNode?> Values { get; set; } = new Dictionary<string, JsonNode>(StringComparer.Ordinal);

	[JsonPropertyName("baseline")]
	public Dictionary<string, JsonNode?> Baseline { get; set; } = new Dictionary<string, JsonNode>(StringComparer.Ordinal);

	[JsonIgnore]
	public bool IsEmpty
	{
		get
		{
			if (Values.Count == 0)
			{
				return Baseline.Count == 0;
			}
			return false;
		}
	}
}
