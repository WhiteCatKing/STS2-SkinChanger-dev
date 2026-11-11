using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ManagedMonsterRuntimeProfile(string TargetScenePath, IReadOnlyList<string> ProviderResourcePaths);
