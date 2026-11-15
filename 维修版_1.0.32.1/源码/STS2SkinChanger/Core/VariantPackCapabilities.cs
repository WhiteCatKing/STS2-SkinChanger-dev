using System.Reflection;
using Godot;

namespace STS2SkinChanger.Core;

/// <summary>
/// What a skin package can do to a character on its own, discovered from its assembly instead of from
/// hard-coded type names.
///
/// This exists because a package's own code only runs when it owns the assembly identity (.NET keeps one
/// copy per identity). Two re-exports of the same pack — the Joan <c>0.3.5-S</c> and <c>0.3.6</c> case —
/// therefore differ: whoever loses the identity keeps its art but silently loses its gate answers, its
/// per-variant palettes, its portraits and its click-to-cycle. The Skin Changer has to supply those
/// behaviours for the losing copy, and it has to know *which* entry points exist in the winner so it can
/// (a) reuse them on the loser's behalf and (b) keep the winner from acting on the loser's selection.
///
/// Detection is signature based with loose name hints, so an unrelated pack simply reports nothing:
/// <list type="bullet">
///   <item><b>menu gate</b> — a type with a static <c>MenuAllowed()</c>/<c>SceneAllowed()</c>/
///         <c>PlayerAllowed(...)</c>-shaped boolean gate. The packs use it to ask the Skin Changer
///         "are you showing my skin?" before touching the character-select or in-run presentation.</item>
///   <item><b>click cycle</b> — a static method taking a <c>Control</c> (the character background) whose
///         name reads like <c>Attach</c>/<c>OnInput</c>/<c>OnClick</c>/<c>Cycle</c>.</item>
///   <item><b>palette refresh</b> — a static <c>Refresh*(SceneTree)</c> or <c>Apply*(Node)</c> hook that
///         re-applies the pack's palette after a variant/stage change.</item>
///   <item><b>hotkey matcher</b> — a static <c>bool Matches(string, InputEventKey)</c> used by the pack to
///         read its own appearance hotkey.</item>
/// </list>
/// </summary>
internal sealed record VariantPackCapabilities(
    Type? MenuGate,
    IReadOnlyList<MethodInfo> GateMethods,
    Type? ClickCycle,
    IReadOnlyList<MethodInfo> ClickMethods,
    Type? Palette,
    IReadOnlyList<MethodInfo> PaletteMethods,
    MethodInfo? HotkeyMatcher)
{
    internal bool HasAny =>
        GateMethods.Count > 0 || ClickMethods.Count > 0 || PaletteMethods.Count > 0 || HotkeyMatcher != null;

    internal string Describe()
    {
        static string Names(IReadOnlyList<MethodInfo> methods) =>
            methods.Count == 0 ? "-" : string.Join("/", methods.Select(m => m.Name));

        return $"门控={MenuGate?.Name ?? "-"}({Names(GateMethods)}) " +
               $"点击={ClickCycle?.Name ?? "-"}({Names(ClickMethods)}) " +
               $"调色板={Palette?.Name ?? "-"}({Names(PaletteMethods)}) " +
               $"快捷键匹配={(HotkeyMatcher == null ? "-" : HotkeyMatcher.DeclaringType?.Name + "." + HotkeyMatcher.Name)}";
    }
}

internal static class VariantPackCapabilitiesScanner
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, VariantPackCapabilities?> Cache =
        new(StringComparer.OrdinalIgnoreCase);

    internal static VariantPackCapabilities? ForAssembly(Assembly? assembly)
    {
        if (assembly == null)
        {
            return null;
        }

        string key;
        try
        {
            key = assembly.FullName ?? assembly.GetName().Name ?? string.Empty;
            key += "|" + (System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(assembly)?.Name ?? "default");
        }
        catch
        {
            return null;
        }

        lock (Sync)
        {
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }
        }

        VariantPackCapabilities? result;
        try
        {
            result = Scan(assembly);
        }
        catch
        {
            result = null;
        }

        lock (Sync)
        {
            Cache[key] = result;
        }

        return result;
    }

    private static VariantPackCapabilities? Scan(Assembly assembly)
    {
        var types = LoadableTypes(assembly).Where(type => type != null).Cast<Type>().ToArray();

        // Menu gate: the packs ask "am I the one on screen?" through a few static booleans.
        var gateType = types.FirstOrDefault(type => type.IsAbstract && type.IsSealed &&
            type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Count(method => IsGateMethod(method)) >= 2);
        var gateMethods = gateType == null
            ? []
            : gateType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(IsGateMethod)
                .ToArray();

        // Click cycle: a static handler that receives the character background Control. Prefer a type
        // whose name reads like a cycle/handler; otherwise take the one with the fewest matches so an
        // unrelated helper cannot win.
        var clickType = types
            .Select(type => (Type: type, Methods: Matching(type, IsClickMethod)))
            .Where(pair => pair.Methods.Count > 0)
            .OrderByDescending(pair => pair.Type.Name.Contains("Cycle", StringComparison.OrdinalIgnoreCase) ||
                                       pair.Type.Name.Contains("Click", StringComparison.OrdinalIgnoreCase))
            .ThenBy(pair => pair.Methods.Count)
            .Select(pair => pair.Type)
            .FirstOrDefault();
        var clickMethods = clickType == null ? [] : Matching(clickType, IsClickMethod).ToArray();

        // Palette: a static re-apply hook, either Refresh*(SceneTree) or Apply*(Node). Prefer the type
        // whose name reads like a palette; otherwise the one with the fewest matches.
        var paletteType = types
            .Select(type => (Type: type, Methods: Matching(type, IsPaletteMethod)))
            .Where(pair => pair.Methods.Count > 0)
            .OrderByDescending(pair => pair.Type.Name.Contains("Palette", StringComparison.OrdinalIgnoreCase) ||
                                       pair.Type.Name.Contains("Tint", StringComparison.OrdinalIgnoreCase) ||
                                       pair.Type.Name.Contains("Color", StringComparison.OrdinalIgnoreCase))
            .ThenBy(pair => pair.Methods.Count)
            .Select(pair => pair.Type)
            .FirstOrDefault();
        var paletteMethods = paletteType == null ? [] : Matching(paletteType, IsPaletteMethod).ToArray();

        // Hotkey matcher: bool Matches(string, InputEventKey).
        MethodInfo? matcher = null;
        foreach (var type in types)
        {
            matcher = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method =>
                {
                    if (method.ReturnType != typeof(bool) || method.Name != "Matches")
                    {
                        return false;
                    }

                    var parameters = method.GetParameters();
                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType.IsAssignableTo(typeof(InputEventKey));
                });
            if (matcher != null)
            {
                break;
            }
        }

        var capabilities = new VariantPackCapabilities(
            gateType, gateMethods, clickType, clickMethods, paletteType, paletteMethods, matcher);
        return capabilities.HasAny ? capabilities : null;
    }

    private static List<MethodInfo> Matching(Type type, Func<MethodInfo, bool> predicate) =>
        type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(predicate)
            .ToList();

    private static bool IsGateMethod(MethodInfo method)
    {
        if (method.ReturnType != typeof(bool) || method.IsAbstract)
        {
            return false;
        }

        var name = method.Name;
        var looksLikeGate =
            name.Contains("Menu", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Scene", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Allow", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Permit", StringComparison.OrdinalIgnoreCase);
        return looksLikeGate && method.GetParameters().Length <= 1;
    }

    private static bool IsClickMethod(MethodInfo method)
    {
        if (method.IsAbstract)
        {
            return false;
        }

        var parameters = method.GetParameters();
        if (parameters.Length is < 1 or > 2 || !parameters[0].ParameterType.IsAssignableTo(typeof(Control)))
        {
            return false;
        }

        var name = method.Name;
        return name.Contains("Attach", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Click", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Input", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Cycle", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPaletteMethod(MethodInfo method)
    {
        if (method.IsAbstract || method.GetParameters() is not [{ ParameterType: var parameter }])
        {
            return false;
        }

        var takesSceneTree = parameter == typeof(SceneTree);
        var takesNode = parameter.IsAssignableTo(typeof(Node));
        if (!takesSceneTree && !takesNode)
        {
            return false;
        }

        return method.Name.StartsWith("Refresh", StringComparison.Ordinal) ||
               method.Name.StartsWith("Apply", StringComparison.Ordinal);
    }

    private static IEnumerable<Type?> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types;
        }
        catch
        {
            return [];
        }
    }
}
