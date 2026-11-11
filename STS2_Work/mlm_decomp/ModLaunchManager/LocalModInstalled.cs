namespace ModLaunchManager;

internal sealed record LocalModInstalled(string Id, string Name, string? Version, string? LocalFolder, bool InUse);
