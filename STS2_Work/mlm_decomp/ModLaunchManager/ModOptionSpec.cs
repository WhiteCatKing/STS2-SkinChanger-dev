using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ModLaunchManager;

internal sealed class ModOptionSpec
{
	internal required string Key { get; init; }

	internal required ModOptionType Type { get; init; }

	internal string? Name { get; init; }

	internal string? Description { get; init; }

	internal string? Section { get; init; }

	internal JsonNode? DeclaredDefault { get; init; }

	internal double? Minimum { get; init; }

	internal double? Maximum { get; init; }

	internal IReadOnlyList<string> Choices { get; init; } = Array.Empty<string>();

	internal bool Advanced { get; init; }

	internal bool Declared { get; init; }

	internal string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Name))
			{
				return Name;
			}
			return Key;
		}
	}
}
