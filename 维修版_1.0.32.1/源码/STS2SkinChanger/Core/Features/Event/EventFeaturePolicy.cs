namespace STS2SkinChanger.Core.Features.Event;

/// <summary>
/// 事件模块要不要在位：装了任何事件皮肤（含只改文字的事件包）才需要。
/// 假商人事件的皮肤挂在 <c>fake_merchant_monster</c> 分组上（见 <see cref="EventSkinPolicy.GroupId"/>），也算。
/// </summary>
internal static class EventFeaturePolicy
{
    internal const string FakeMerchantGroupId = "fake_merchant_monster";

    internal static bool? ShouldBeActive(
        IEnumerable<(string GroupId, IEnumerable<string> OptionIds)>? groups,
        string baseOptionId)
    {
        if (groups == null)
        {
            return null;
        }

        return groups.Any(group =>
            (EventSkinPolicy.IsEventGroup(group.GroupId) ||
             group.GroupId.Equals(FakeMerchantGroupId, StringComparison.OrdinalIgnoreCase)) &&
            group.OptionIds.Any(optionId =>
                !string.IsNullOrWhiteSpace(optionId) &&
                !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase)));
    }
}
