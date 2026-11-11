using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record VisualResourceBinding(string SourcePath, IReadOnlyList<string> Files);
