using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record RuntimeProviderCandidate(string ProviderId, IReadOnlyCollection<string> GroupIds, bool IsRunWideMonsterProvider);
