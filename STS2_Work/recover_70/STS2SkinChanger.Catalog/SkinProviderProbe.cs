using System.Collections.Generic;

namespace STS2SkinChanger.Catalog;

internal sealed record SkinProviderProbe(string Id, string? RootPath, int VisualGroupCount, int CardAssetCount, int CardPresentationCount, int RuntimeImageCount, int ManagedScriptCount, bool HasInteractiveScenes, string? ManifestId = null, bool HasResourceBackedCosmetics = false, int PackResourceCount = 0, int ServedResourceCount = 0, IReadOnlyList<string>? ServedGroupIds = null, IReadOnlyList<string>? ServedSourcePaths = null, int BaselinePartGroupCount = 0, IReadOnlyList<string>? BaselinePartGroupIds = null)
{
	public string ResourceNamespaceId => ManifestId ?? Id;

	public bool IsBaselinePartTakeover
	{
		get
		{
			if (VisualGroupCount == 0)
			{
				return BaselinePartGroupCount > 0;
			}
			return false;
		}
	}
}
