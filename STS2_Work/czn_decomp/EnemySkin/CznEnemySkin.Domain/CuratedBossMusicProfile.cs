using System.Collections.Generic;

namespace CznEnemySkin.Domain;

internal sealed record CuratedBossMusicProfile(string Id, string EncounterId, IReadOnlyList<string> PhaseTrackIds);
