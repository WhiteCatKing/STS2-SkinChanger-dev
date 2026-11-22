using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed record RequiredFixTestSpec(
    [property: JsonPropertyName("suite")] string Suite,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("args")] string Args,
    [property: JsonPropertyName("requiresExternalPack")] bool RequiresExternalPack = false);

internal sealed record RequiredFixEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("since")] string Since,
    [property: JsonPropertyName("symbols")] IReadOnlyList<string> Symbols,
    [property: JsonPropertyName("tests")] IReadOnlyList<RequiredFixTestSpec> Tests,
    [property: JsonPropertyName("gap")] string? Gap = null);

internal enum GateTestOutcome
{
    NotRun,
    Passed,
    Failed
}

internal static class Gate
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly Dictionary<string, GateTestOutcome> Outcomes =
        new(StringComparer.OrdinalIgnoreCase);

    internal static void Run(string name, Action action)
    {
        try
        {
            action();
            RecordPass(name);
        }
        catch
        {
            Outcomes[name] = GateTestOutcome.Failed;
            throw;
        }
    }

    internal static async Task RunAsync(string name, Func<Task> action)
    {
        try
        {
            await action();
            RecordPass(name);
        }
        catch
        {
            Outcomes[name] = GateTestOutcome.Failed;
            throw;
        }
    }

    private static void RecordPass(string name)
    {
        if (!Outcomes.TryGetValue(name, out var existing) || existing != GateTestOutcome.Failed)
        {
            Outcomes[name] = GateTestOutcome.Passed;
        }
    }

    internal static string FindRequiredFixesManifestPath()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(start))
            {
                continue;
            }

            var current = new DirectoryInfo(Path.GetFullPath(start));
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "tests", "required-fixes.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var nested = Path.Combine(current.FullName, "STS2-SkinChanger_修改件", "tests", "required-fixes.json");
                if (File.Exists(nested))
                {
                    return nested;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException(
            $"找不到门禁清单 tests/required-fixes.json（CWD={Environment.CurrentDirectory}, BaseDir={AppContext.BaseDirectory}）。");
    }

    internal static IReadOnlyList<RequiredFixEntry> LoadRequiredFixes(out string manifestPath)
    {
        manifestPath = FindRequiredFixesManifestPath();
        var json = File.ReadAllText(manifestPath);
        var entries = JsonSerializer.Deserialize<List<RequiredFixEntry>>(json, JsonOptions);
        if (entries == null || entries.Count == 0)
        {
            throw new InvalidOperationException($"门禁清单 {manifestPath} 为空或无法解析。");
        }

        return entries;
    }

    internal static (int Pass, int Fail, int NotRun, int Separate, int Gap) PrintReport()
    {
        IReadOnlyList<RequiredFixEntry> entries;
        string manifestPath;
        try
        {
            entries = LoadRequiredFixes(out manifestPath);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[GATE] 无法读取门禁清单：{exception.Message}");
            return (0, 1, 0, 0, 0);
        }

        var pass = 0;
        var fail = 0;
        var notRun = 0;
        var separate = 0;
        var gap = 0;

        Console.WriteLine($"=== Required Fixes Gate ({entries.Count} items, {Path.GetFileName(manifestPath)}) ===");
        foreach (var entry in entries)
        {
            var (status, note) = EvaluateEntry(entry);
            switch (status)
            {
                case "PASS": pass++; break;
                case "FAIL": fail++; break;
                case "NOT RUN": notRun++; break;
                case "SEPARATE": separate++; break;
                case "GAP": gap++; break;
            }

            Console.WriteLine($"  [{status,-8}] {entry.Id} ({entry.Since}) - {entry.Title}{note}");
        }

        Console.WriteLine(
            $"Required fixes summary: total={entries.Count}, PASS={pass}, FAIL={fail}, NOT RUN={notRun}, SEPARATE={separate}, GAP={gap}");
        return (pass, fail, notRun, separate, gap);
    }

    private static (string Status, string Note) EvaluateEntry(RequiredFixEntry entry)
    {
        if (entry.Tests == null || entry.Tests.Count == 0)
        {
            var reason = string.IsNullOrWhiteSpace(entry.Gap) ? "暂无用例" : entry.Gap;
            return ("GAP", $" [{reason}]");
        }

        var runtimeInSuite = entry.Tests
            .Where(t => t.Suite.Equals("Runtime", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrWhiteSpace(t.Args))
            .ToArray();
        var separateOrLogic = entry.Tests
            .Where(t => !t.Suite.Equals("Runtime", StringComparison.OrdinalIgnoreCase) ||
                        !string.IsNullOrWhiteSpace(t.Args))
            .ToArray();

        if (runtimeInSuite.Length == 0)
        {
            var extras = string.Join(", ", separateOrLogic.Select(FormatTestSpec));
            return ("SEPARATE", $" [{extras}]");
        }

        if (runtimeInSuite.Any(t => Outcomes.GetValueOrDefault(t.Name) == GateTestOutcome.Failed))
        {
            return ("FAIL", string.Empty);
        }

        if (runtimeInSuite.Any(t => Outcomes.GetValueOrDefault(t.Name) != GateTestOutcome.Passed))
        {
            return ("NOT RUN", string.Empty);
        }

        var extraNote = separateOrLogic.Length > 0
            ? $" [+ {string.Join(", ", separateOrLogic.Select(FormatTestSpec))}]"
            : string.Empty;
        return ("PASS", extraNote);
    }

    private static string FormatTestSpec(RequiredFixTestSpec spec) =>
        string.IsNullOrWhiteSpace(spec.Args)
            ? $"{spec.Suite}:{spec.Name}"
            : $"{spec.Suite}:{spec.Name}({spec.Args})";
}
