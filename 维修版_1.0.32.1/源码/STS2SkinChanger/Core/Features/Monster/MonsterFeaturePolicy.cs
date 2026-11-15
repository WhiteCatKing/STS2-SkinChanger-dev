namespace STS2SkinChanger.Core.Features.Monster;

/// <summary>
/// 怪物模块要不要在位：某个怪物分组里有非原版选项（装了怪物皮肤）才需要。
/// 怪物分组两个来源，任一命中即算：
/// ① <c>Config.MonsterSkinCategoryGroups</c> 里已登记的分组快照（普通 PCK 骨骼/贴图皮肤就在这一类）；
/// ② 选项带托管怪物场景 / 运行时怪物外观（DLL 型皮肤，目录里这两个字段才有值）。
/// 真商人和假商人归商人模块，不算进来。
/// </summary>
internal static class MonsterFeaturePolicy
{
    internal static bool? ShouldBeActive(
        IEnumerable<(string GroupId, IEnumerable<string> OptionIds, bool HasMonsterVisual)>? groups,
        IEnumerable<string>? registeredMonsterGroupIds,
        string baseOptionId)
    {
        if (groups == null)
        {
            return null;
        }

        var registered = new HashSet<string>(
            registeredMonsterGroupIds ?? [], StringComparer.OrdinalIgnoreCase);
        return groups.Any(group =>
            (registered.Contains(group.GroupId) || group.HasMonsterVisual) &&
            !group.GroupId.Equals("merchant", StringComparison.OrdinalIgnoreCase) &&
            !group.GroupId.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase) &&
            group.OptionIds.Any(optionId =>
                !string.IsNullOrWhiteSpace(optionId) &&
                !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase)));
    }
}
