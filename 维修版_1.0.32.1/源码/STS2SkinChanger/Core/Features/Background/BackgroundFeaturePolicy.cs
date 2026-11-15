namespace STS2SkinChanger.Core.Features.Background;

internal static class BackgroundFeaturePolicy
{
    internal static bool? ShouldBeActive(IEnumerable<string>? optionIds, string baseOptionId)
    {
        if (optionIds == null)
        {
            return null;
        }

        return optionIds.Any(optionId =>
            !BackgroundSkinPolicy.IsFollowCategory(optionId) &&
            !optionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase) &&
            !optionId.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase));
    }
}
