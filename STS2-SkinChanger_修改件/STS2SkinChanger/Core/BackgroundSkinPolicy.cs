using System.Text;

namespace STS2SkinChanger.Core;

/// <summary>
/// Pure rules for the battle-background selection: option encoding, the default
/// "follow category" state and readable battlefield names. Kept engine-free so the saved
/// configuration and the UI cannot drift apart.
/// </summary>
internal static class BackgroundSkinPolicy
{
    /// <summary>Follow the active providers' own per-encounter background behaviour.</summary>
    public const string FollowCategoryId = "__follow__";

    /// <summary>Same effect as <see cref="FollowCategoryId"/> but shown as a provider default.</summary>
    public const string ModDefaultId = "__mod_default__";

    private const string ProviderPrefix = "bg";
    private const string Separator = "::";

    public static string ProviderOptionId(string providerId, string battlefieldId) =>
        ProviderPrefix + Separator + providerId.Trim() + Separator + battlefieldId.Trim();

    public static bool TryParseProviderOptionId(
        string? optionId,
        out string providerId,
        out string battlefieldId)
    {
        providerId = string.Empty;
        battlefieldId = string.Empty;
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return false;
        }

        var parts = optionId.Split(Separator, StringSplitOptions.None);
        if (parts.Length != 3 ||
            !parts[0].Equals(ProviderPrefix, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(parts[1]) ||
            string.IsNullOrWhiteSpace(parts[2]))
        {
            return false;
        }

        providerId = parts[1].Trim();
        battlefieldId = parts[2].Trim();
        return true;
    }

    public static bool IsFollowCategory(string? selection) =>
        string.IsNullOrWhiteSpace(selection) ||
        selection.Equals(FollowCategoryId, StringComparison.OrdinalIgnoreCase);

    public static string DisplayName(string battlefieldId)
    {
        var token = battlefieldId.Trim();
        if (token.StartsWith("b_", StringComparison.OrdinalIgnoreCase))
        {
            token = token[2..];
        }

        var builder = new StringBuilder(token.Length);
        var capitalize = true;
        foreach (var character in token)
        {
            if (character is '_' or '-')
            {
                if (builder.Length > 0 && builder[^1] != ' ')
                {
                    builder.Append(' ');
                }

                capitalize = true;
                continue;
            }

            builder.Append(capitalize ? char.ToUpperInvariant(character) : character);
            capitalize = false;
        }

        return builder.ToString().Trim();
    }
}
