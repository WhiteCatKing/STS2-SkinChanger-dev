namespace ModLaunchManager;

internal sealed record CollectionReadResult(CollectionSnapshot? Snapshot, CollectionReadError Error);
