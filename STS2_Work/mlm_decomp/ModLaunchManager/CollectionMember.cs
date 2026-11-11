namespace ModLaunchManager;

internal sealed record CollectionMember(ulong ItemId, string Title, WorkshopItemKind Kind, CollectionMemberRole Role, string? Via, ulong ViaId);
