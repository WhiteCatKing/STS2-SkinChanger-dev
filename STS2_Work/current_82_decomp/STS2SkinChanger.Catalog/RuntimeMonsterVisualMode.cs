using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record RuntimeMonsterVisualMode(string ProviderId, string AssemblyPath, string ServiceTypeName, string EnumTypeName, string SetterName, string ModeName, string DisplayName)
{
	public IReadOnlyList<string> ResourcePaths { get; init; } = Array.Empty<string>();
}
