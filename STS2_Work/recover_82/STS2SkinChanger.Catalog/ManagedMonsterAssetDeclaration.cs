using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ManagedMonsterAssetDeclaration(string ModelTypeName, IReadOnlyList<string> ResourcePaths);
