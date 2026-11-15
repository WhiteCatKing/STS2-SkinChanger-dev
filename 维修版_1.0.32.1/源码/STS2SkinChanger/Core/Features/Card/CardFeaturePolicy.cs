namespace STS2SkinChanger.Core.Features.Card;

internal static class CardFeaturePolicy
{
    internal static bool? ShouldBeActive(
        IEnumerable<(string OptionId, bool HasCardArt)>? options,
        string baseOptionId)
    {
        if (options == null)
        {
            return null;
        }

        return options.Any(option =>
            option.HasCardArt &&
            !string.IsNullOrWhiteSpace(option.OptionId) &&
            !option.OptionId.Equals(baseOptionId, StringComparison.OrdinalIgnoreCase));
    }
}
