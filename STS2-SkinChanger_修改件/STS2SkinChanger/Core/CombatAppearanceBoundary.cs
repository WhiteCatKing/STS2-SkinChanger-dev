using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

// The game owns the original call and its exceptions. Only wrap work done by our
// prefixes/postfixes so a cosmetic failure cannot interrupt the room's native setup.
internal sealed class CombatAppearanceBoundary : IDisposable
{
    [ThreadStatic] private static CombatAppearanceBoundary? _current;
    private static readonly ConditionalWeakTable<NCreatureVisuals, object> FallbackVisuals = new();
    private static readonly Dictionary<string, long> NextWarning = new(StringComparer.Ordinal);
    private static readonly object WarningLock = new();

    private readonly CombatAppearanceBoundary? _previous;
    private IDisposable? _resourceScope;
    private bool _skip;
    private bool _disposed;

    private CombatAppearanceBoundary(bool skip)
    {
        _previous = _current;
        _skip = skip || SkipAppearance;
        _current = this;
    }

    internal static bool SkipAppearance => _current?._skip == true;
    internal bool HasResourceScope => _resourceScope != null;

    internal static CombatAppearanceBoundary? BeginIfNeeded() =>
        _current == null ? new CombatAppearanceBoundary(skip: false) : null;

    internal static CombatAppearanceBoundary Begin(Func<IDisposable?> open, bool skip = false)
    {
        var boundary = new CombatAppearanceBoundary(skip);
        if (!boundary._skip)
        {
            try { boundary._resourceScope = open(); }
            catch (Exception error) { SkipAfterFailure(error); }
        }
        return boundary;
    }

    internal static void SkipAfterFailure(Exception error)
    {
        if (_current is { } current)
        {
            current._skip = true;
            current.ReleaseScope();
        }
        Warn("战斗模型", error);
    }

    internal static void MarkFallback(NCreatureVisuals visuals) =>
        FallbackVisuals.GetValue(visuals, static _ => new object());

    internal static bool IsFallback(NCreatureVisuals? visuals) =>
        visuals != null && FallbackVisuals.TryGetValue(visuals, out _);

    internal static NCreatureVisuals FinishVisual(NCreatureVisuals original, NCreatureVisuals? replacement)
    {
        if (SkipAppearance)
        {
            MarkFallback(original);
            if (replacement != null && !ReferenceEquals(replacement, original)) ReleaseNode(replacement);
            return original;
        }

        if (replacement == null || ReferenceEquals(replacement, original)) return original;
        ReleaseNode(original);
        return replacement;
    }

    internal static void ReleaseNode(Node? node)
    {
        try { if (node != null && GodotObject.IsInstanceValid(node)) node.QueueFree(); }
        catch (Exception error) { Warn("模型释放", error); }
    }

    internal static void Warn(string stage, Exception error)
    {
        var now = System.Environment.TickCount64;
        lock (WarningLock)
        {
            if (NextWarning.TryGetValue(stage, out var next) && now < next) return;
            NextWarning[stage] = now + 30_000;
        }

        try { ModLog.Warn($"{stage}降级，保留游戏当前资源：{error.GetBaseException().Message}"); }
        catch (Exception) { } // Logging must not replace the original game result or exception.
    }

    private void ReleaseScope()
    {
        var scope = _resourceScope;
        _resourceScope = null;
        try { scope?.Dispose(); }
        catch (Exception error) { Warn("外观范围清理", error); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { ReleaseScope(); }
        finally { _current = _previous; }
    }
}
