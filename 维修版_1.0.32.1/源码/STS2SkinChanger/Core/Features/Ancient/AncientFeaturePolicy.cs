namespace STS2SkinChanger.Core.Features.Ancient;

/// <summary>
/// 先古模块要不要在位：先古分组里有非原版选项（装了先古皮肤）才需要。
/// 先古分组是逐个先古建的，分组 id 就是先古 id（见 <c>SkinCatalog.KnownAncientIds</c>）。
/// </summary>
internal static class AncientFeaturePolicy
{
    internal static bool? ShouldBeActive(
        IEnumerable<(string GroupId, IEnumerable<string> OptionIds)>? groups,
        IEnumerable<string> ancientGroupIds,
        string baseOptionId)
    {
        if (groups == null)
        {
            return null;
        }

        var ancient = new HashSet<string>(ancientGroupIds, StringComparer.OrdinalIgnoreCase);
        return groups.Any(group =>
            ancient.Contains(group.GroupId) &&
            group.OptionIds.Any(optionId =>
                !string.IsNullOrWhiteSpace(optionId) &&
                !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase)));
    }
}
