using System.Text.RegularExpressions;

internal static class RequiredFixesManifestTests
{
    internal static void Run()
    {
        var entries = Gate.LoadRequiredFixes(out var manifestPath);
        var testsDir = Path.GetDirectoryName(manifestPath)!;
        var programCsPath = Path.Combine(testsDir, "STS2SkinChanger.RuntimeTests", "Program.cs");
        if (!File.Exists(programCsPath))
        {
            throw new InvalidOperationException($"门禁清单自检找不到 Program.cs：{programCsPath}");
        }

        var programSource = File.ReadAllText(programCsPath);
        var runtimeTypeNames = typeof(RequiredFixesManifestTests).Assembly
            .GetTypes()
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                throw new InvalidOperationException("required-fixes.json 存在缺少 id 的条目。");
            }

            if (!seenIds.Add(entry.Id))
            {
                throw new InvalidOperationException($"required-fixes.json 存在重复 id：{entry.Id}。");
            }

            if (string.IsNullOrWhiteSpace(entry.Title) ||
                string.IsNullOrWhiteSpace(entry.Since) ||
                entry.Symbols == null ||
                entry.Symbols.Count == 0)
            {
                throw new InvalidOperationException(
                    $"required-fixes.json 条目 {entry.Id} 缺少必填字段（title/since/symbols）。");
            }

            if (entry.Tests == null || entry.Tests.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(entry.Gap))
                {
                    throw new InvalidOperationException(
                        $"required-fixes.json 条目 {entry.Id} 的 tests 为空，必须写明 gap 原因。");
                }

                continue;
            }

            foreach (var test in entry.Tests)
            {
                if (!test.Suite.Equals("Runtime", StringComparison.Ordinal) &&
                    !test.Suite.Equals("Logic", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"required-fixes.json 条目 {entry.Id} 的 suite 无效：{test.Suite}（仅允许 Runtime 或 Logic）。");
                }

                if (string.IsNullOrWhiteSpace(test.Name))
                {
                    throw new InvalidOperationException(
                        $"required-fixes.json 条目 {entry.Id} 存在空测试名。");
                }

                if (test.Suite.Equals("Runtime", StringComparison.Ordinal))
                {
                    if (!runtimeTypeNames.Contains(test.Name))
                    {
                        throw new InvalidOperationException(
                            $"required-fixes.json 条目 {entry.Id} 引用的 Runtime 测试类型不存在：{test.Name}。");
                    }

                    if (!string.IsNullOrWhiteSpace(test.Args))
                    {
                        var quotedArg = "\"" + test.Args + "\"";
                        if (!programSource.Contains(quotedArg, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                $"required-fixes.json 条目 {entry.Id} 引用的参数 {test.Args} 未在 RuntimeTests/Program.cs 中识别。");
                        }
                    }
                    else
                    {
                        var gateCallPattern = $@"Gate\.Run(?:Async)?\(\s*""{Regex.Escape(test.Name)}""";
                        if (!Regex.IsMatch(programSource, gateCallPattern, RegexOptions.CultureInvariant))
                        {
                            throw new InvalidOperationException(
                                $"required-fixes.json 条目 {entry.Id} 声明 {test.Name} 属于无参完整套件，但 Program.cs 未通过 Gate.Run(\"{test.Name}\", ...) 调用。");
                        }
                    }
                }
            }
        }

        Console.WriteLine($"Required fixes manifest self-check passed: {entries.Count} items verified.");
    }
}
