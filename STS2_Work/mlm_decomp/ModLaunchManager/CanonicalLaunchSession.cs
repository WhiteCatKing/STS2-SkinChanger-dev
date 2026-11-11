using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record CanonicalLaunchSession(CanonicalLaunchDocument Document, IReadOnlyList<LaunchPlanSeed> Seeds, IReadOnlyList<CanonicalLaunchEntry> Dormant, CanonicalReconciliation Reconciliation, CanonicalLoadState LoadState, string? LoadError);
