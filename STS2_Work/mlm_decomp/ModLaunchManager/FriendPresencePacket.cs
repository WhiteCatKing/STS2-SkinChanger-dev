using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record FriendPresencePacket(string Header, IReadOnlyList<string> Chunks);
