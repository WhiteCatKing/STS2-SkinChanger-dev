using System.Collections.Concurrent;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class UiInjectionGuard
{
    private static readonly ConcurrentDictionary<string, int> FailureCounts = new(StringComparer.Ordinal);

    internal static Action<string>? ErrorLoggerForTests { get; set; }

    internal static void Run(string point, Action action)
    {
        if (action == null)
        {
            return;
        }

        try
        {
            action();
        }
        catch (Exception exception)
        {
            RecordFailure(point, exception);
        }
    }

    internal static T Run<T>(string point, Func<T> action, T fallback)
    {
        if (action == null)
        {
            return fallback;
        }

        try
        {
            return action();
        }
        catch (Exception exception)
        {
            RecordFailure(point, exception);
            return fallback;
        }
    }

    internal static void RunWhenInTree(Node? node, string point, Action action)
    {
        if (node == null || action == null)
        {
            return;
        }

        try
        {
            if (!GodotObject.IsInstanceValid(node))
            {
                return;
            }

            if (node.IsInsideTree())
            {
                Run(point, action);
                return;
            }

            var invoked = false;
            Action? onTreeEntered = null;
            onTreeEntered = () =>
            {
                if (invoked)
                {
                    return;
                }

                invoked = true;
                if (GodotObject.IsInstanceValid(node))
                {
                    node.TreeEntered -= onTreeEntered;
                }

                if (!GodotObject.IsInstanceValid(node) || !node.IsInsideTree())
                {
                    return;
                }

                Run(point, action);
            };
            node.TreeEntered += onTreeEntered;
        }
        catch (Exception exception)
        {
            RecordFailure(point, exception);
        }
    }

    internal static int GetFailureCount(string point) =>
        FailureCounts.TryGetValue(NormalizePoint(point), out var count) ? count : 0;

    internal static IReadOnlyDictionary<string, int> GetFailureCountsSnapshot() =>
        new Dictionary<string, int>(FailureCounts, StringComparer.Ordinal);

    internal static void ResetForTests()
    {
        FailureCounts.Clear();
        ErrorLoggerForTests = null;
    }

    private static void RecordFailure(string? point, Exception exception)
    {
        var key = NormalizePoint(point);
        var count = FailureCounts.AddOrUpdate(key, 1, static (_, existing) => existing + 1);
        if (count != 1)
        {
            return;
        }

        var summary = $"[界面注入] {key} 失败: {exception.GetType().Name}: {exception.Message}";
        var detail = exception.ToString();
        WriteError(string.IsNullOrWhiteSpace(detail) ? summary : summary + System.Environment.NewLine + detail);
    }

    private static string NormalizePoint(string? point) =>
        string.IsNullOrWhiteSpace(point) ? "(unknown)" : point;

    private static void WriteError(string message)
    {
        var testLogger = ErrorLoggerForTests;
        if (testLogger != null)
        {
            testLogger(message);
            return;
        }

        ModLog.Error(message);
    }
}
