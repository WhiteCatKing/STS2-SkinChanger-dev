using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record LocalModPairing<TGone, TArrival>(IReadOnlyList<(TGone Gone, TArrival Arrival)> Moves, IReadOnlyList<TGone> Removed, IReadOnlyList<TArrival> Added);
