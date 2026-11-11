using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record LocalModImportScan(IReadOnlyList<LocalModImportUnit> Units, IReadOnlyList<LocalModImportIssue> Issues);
