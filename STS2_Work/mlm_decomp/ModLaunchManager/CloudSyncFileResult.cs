namespace ModLaunchManager;

internal sealed record CloudSyncFileResult(string LocalName, CloudSyncDirection Direction, string? Problem);
