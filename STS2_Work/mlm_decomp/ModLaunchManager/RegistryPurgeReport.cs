using System.Collections.Generic;

namespace ModLaunchManager;

internal readonly record struct RegistryPurgeReport(int ClearedRoots, IReadOnlyList<string> Failures);
