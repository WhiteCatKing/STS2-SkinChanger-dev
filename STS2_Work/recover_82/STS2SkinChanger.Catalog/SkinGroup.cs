using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed class SkinGroup(string id, string displayName)
{
	public string Id { get; } = id;

	public string DisplayName { get; } = displayName;

	public List<SkinOption> Options { get; } = new List<SkinOption>();
}
