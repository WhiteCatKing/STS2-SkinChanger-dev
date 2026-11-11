using System.Collections.Generic;

namespace CznEnemySkin.Domain;

internal sealed record ActMapMusicProfile(string Id, string ActId, IReadOnlyList<string> DefaultTrackIds, bool EnabledByDefault);
