using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed class CardReplacementConfig
{
	public List<NormalCardReplacement> NormalReplacements { get; set; } = new List<NormalCardReplacement>();

	public List<AncientCardReplacement> AncientReplacements { get; set; } = new List<AncientCardReplacement>();
}
