using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record BackgroundPackCandidate(string ProviderId, string DisplayName, IReadOnlyList<BackgroundPackShell> Shells);
