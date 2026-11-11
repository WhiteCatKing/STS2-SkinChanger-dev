using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record CollectionSnapshot(ulong CollectionId, string Title, IReadOnlyList<CollectionMember> Members);
