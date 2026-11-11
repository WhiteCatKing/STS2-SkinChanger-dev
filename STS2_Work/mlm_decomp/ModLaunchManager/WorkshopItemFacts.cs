using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record WorkshopItemFacts(ulong Id, WorkshopItemKind Kind, string Title, IReadOnlyList<ulong> Children);
