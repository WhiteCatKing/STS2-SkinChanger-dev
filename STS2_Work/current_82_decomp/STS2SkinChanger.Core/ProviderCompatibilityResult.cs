using System.IO;

namespace STS2SkinChanger.Core;

internal sealed record ProviderCompatibilityResult(MemoryStream? Assembly, ProviderCompatibilityReport Report);
