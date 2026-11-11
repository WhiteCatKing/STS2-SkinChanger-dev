using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CznCore.Presentation;

public static class PresentationResourceCatalog
{
	public static IEnumerable<string> Enumerate(JsonElement value, string resourceRoot, bool includeAudio = false)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		Collect(value, resourceRoot, includeAudio, hashSet);
		return hashSet;
	}

	private static void Collect(JsonElement value, string prefix, bool audio, HashSet<string> paths)
	{
		switch (value.ValueKind)
		{
		case JsonValueKind.Object:
		{
			foreach (JsonProperty item in value.EnumerateObject())
			{
				Collect(item.Value, prefix, audio, paths);
			}
			break;
		}
		case JsonValueKind.Array:
		{
			foreach (JsonElement item2 in value.EnumerateArray())
			{
				Collect(item2, prefix, audio, paths);
			}
			break;
		}
		case JsonValueKind.String:
		{
			string @string = value.GetString();
			if (@string != null && @string.StartsWith(prefix, StringComparison.Ordinal) && (@string.EndsWith(".tscn", StringComparison.Ordinal) || @string.EndsWith(".tres", StringComparison.Ordinal) || @string.EndsWith(".png", StringComparison.Ordinal) || (audio && @string.EndsWith(".ogg", StringComparison.Ordinal))))
			{
				paths.Add(@string);
			}
			break;
		}
		}
	}
}
