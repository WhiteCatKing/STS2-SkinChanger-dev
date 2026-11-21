namespace STS2SkinChanger.Core;

internal static class DirectCharacterRuntimeTargetPolicy
{
    public static IReadOnlySet<string> ResolveTargets(
        IEnumerable<(string FieldName, string Value)> declarations,
        IEnumerable<string> knownCharacterGroupIds)
    {
        var knownGroups = knownCharacterGroupIds
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(value => value, value => value, StringComparer.OrdinalIgnoreCase);
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var declaration in declarations)
        {
            if (!IsTargetCharacterField(declaration.FieldName) ||
                string.IsNullOrWhiteSpace(declaration.Value) ||
                !knownGroups.TryGetValue(declaration.Value.Trim(), out var knownGroup))
            {
                continue;
            }

            targets.Add(knownGroup);
        }

        return targets;
    }

    internal static bool IsTargetCharacterField(string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        var normalized = new string(fieldName
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
        // 作者声明「本包替换哪个角色」的字段名各式各样（CharacterKey、TargetCharacterId、
        // SkinCharacter…）。这里只按名字含 character 收窄候选，真正的判定在
        // ResolveTargets：常量值必须正好等于已知角色组 id。名字里带 default/original/
        // fallback 的是「没选中时退回谁」的兼容字段，不能当成声明，否则完整皮肤会串进
        // 另一个角色的列表。
        if (normalized.Contains("default", StringComparison.Ordinal) ||
            normalized.Contains("original", StringComparison.Ordinal) ||
            normalized.Contains("fallback", StringComparison.Ordinal))
        {
            return false;
        }

        return normalized.Contains("character", StringComparison.Ordinal);
    }
}
