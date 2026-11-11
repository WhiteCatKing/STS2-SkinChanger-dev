using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ExportedCardPortraitMode(string IdSuffix, string NameMarker, IReadOnlyDictionary<string, string> Portraits, bool UseAncientLayout);
