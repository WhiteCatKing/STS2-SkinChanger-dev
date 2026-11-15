namespace STS2SkinChanger.Core.Features.Merchant;

internal static class MerchantFeaturePolicy
{
    internal static bool? ShouldBeActive(
        IEnumerable<(string GroupId, IEnumerable<string> OptionIds)>? groups,
        string baseOptionId)
    {
        if (groups == null)
        {
            return null;
        }

        return groups.Any(group =>
            (group.GroupId.Equals("merchant", StringComparison.OrdinalIgnoreCase) ||
             group.GroupId.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase)) &&
            group.OptionIds.Any(optionId =>
                !string.IsNullOrWhiteSpace(optionId) &&
                !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase)));
    }
}
