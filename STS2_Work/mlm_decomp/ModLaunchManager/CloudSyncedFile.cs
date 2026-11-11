using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record CloudSyncedFile(string LocalName, string CloudName, IReadOnlyList<string> LocalOnlyKeys);
