using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Catalog;

internal sealed record FrameworkModelSkinContract(string TargetModelName, IReadOnlyDictionary<string, string> Resources, IReadOnlyDictionary<string, IReadOnlyList<string>> ResourceLists, IReadOnlyDictionary<string, string> Values)
{
	public IEnumerable<string> ResourcePaths => Resources.Values.Concat(ResourceLists.Values.SelectMany((IReadOnlyList<string> paths) => paths));
}
