using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed class CardSkinGroup(string id, string displayName)
{
	public string Id { get; } = id;

	public string DisplayName { get; } = displayName;

	public List<CardSkinOption> Options { get; } = new List<CardSkinOption>();
}
