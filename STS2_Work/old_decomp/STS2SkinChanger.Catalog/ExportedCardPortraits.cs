using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record ExportedCardPortraits(IReadOnlyDictionary<string, string> Normal, IReadOnlyDictionary<string, AncientCardPortrait> Ancient, IReadOnlyList<ExportedCardPortraitMode> Modes);
