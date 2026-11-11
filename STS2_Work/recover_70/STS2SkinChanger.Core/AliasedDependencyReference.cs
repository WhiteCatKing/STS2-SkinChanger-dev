using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal readonly record struct AliasedDependencyReference(bool IsRewritable, IReadOnlyCollection<string> ResourcePaths);
