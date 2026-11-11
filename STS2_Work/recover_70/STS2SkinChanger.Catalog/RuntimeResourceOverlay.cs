using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record RuntimeResourceOverlay(IReadOnlyDictionary<string, string> ResourcePaths, IReadOnlyDictionary<string, byte[]> Files, IReadOnlyDictionary<string, string> SourceAliases, IReadOnlyDictionary<string, string> PayloadAliases, IReadOnlySet<string> CanonicalDependencyPaths, bool CanReuseExternalDependencies = false);
