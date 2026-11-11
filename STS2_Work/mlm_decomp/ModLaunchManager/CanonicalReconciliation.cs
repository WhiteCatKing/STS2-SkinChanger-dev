using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class CanonicalReconciliation
{
	internal List<CanonicalSeedDescriptor> Seeds { get; } = new List<CanonicalSeedDescriptor>();

	internal List<CanonicalLaunchEntry> Dormant { get; } = new List<CanonicalLaunchEntry>();

	internal int WorkshopExact { get; set; }

	internal int PathExact { get; set; }

	internal int MovedFallback { get; set; }

	internal int NewCount { get; set; }

	internal int AmbiguousGroups { get; set; }
}
