namespace STS2SkinChanger.Core.Features.Character;

/// <summary>
/// 角色模块要不要在位：角色分组或随从分组里有非原版选项（装了角色 / 随从皮肤）才需要。
/// 只看目录、不看当前选择：按当前选择判断会被 2 秒的轮询间隔拖慢，切皮肤那一刻补丁可能还没装上。
/// </summary>
internal static class CharacterFeaturePolicy
{
    internal static bool? ShouldBeActive(
        IEnumerable<(string GroupId, bool IsCharacterGroup, bool IsCompanionGroup, IEnumerable<string> OptionIds)>? groups,
        string baseOptionId)
    {
        if (groups == null)
        {
            return null;
        }

        return groups.Any(group =>
            (group.IsCharacterGroup || group.IsCompanionGroup) &&
            group.OptionIds.Any(optionId =>
                !string.IsNullOrWhiteSpace(optionId) &&
                !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase)));
    }
}
