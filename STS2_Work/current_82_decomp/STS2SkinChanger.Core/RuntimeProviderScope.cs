using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record RuntimeProviderScope(IReadOnlyCollection<string> VisibleGroupIds, IReadOnlyCollection<string> RunEnvironmentProviderIds);
