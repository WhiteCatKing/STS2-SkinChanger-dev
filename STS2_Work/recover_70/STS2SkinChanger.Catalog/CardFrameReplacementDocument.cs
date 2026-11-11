using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed class CardFrameReplacementDocument
{
	public List<CardFrameReplacementEntry> Entries { get; set; } = new List<CardFrameReplacementEntry>();
}
