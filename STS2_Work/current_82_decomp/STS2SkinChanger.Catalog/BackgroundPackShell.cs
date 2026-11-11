using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record BackgroundPackShell(string Title, string ShellPath, IReadOnlyList<string> LayerPaths);
