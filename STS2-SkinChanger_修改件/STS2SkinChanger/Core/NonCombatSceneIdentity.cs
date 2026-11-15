using System.Collections.Concurrent;

namespace STS2SkinChanger.Core;

// Keep isolated resource loading separate from the scene identity authors test in Ready.
internal static class NonCombatSceneIdentity
{
    private static readonly ConcurrentDictionary<string, string> Paths = new(StringComparer.Ordinal);

    internal static void Register(string alias, string canonical)
    {
        if (IsNonCombatScene(canonical) && alias.StartsWith("res://", StringComparison.Ordinal))
            Paths[alias] = canonical;
    }

    internal static string Resolve(string path) => Paths.GetValueOrDefault(path, path);

    private static bool IsNonCombatScene(string path) =>
        path.StartsWith("res://scenes/merchant/characters/", StringComparison.Ordinal) ||
        path.StartsWith("res://scenes/rest_site/characters/", StringComparison.Ordinal);
}
