using System.Text.Json;
using STS2SkinChanger;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class GlobalWritePackAudit
{
    private const string Local = "mods_directory";
    private const string Workshop = "steam_workshop";

    internal static void Run(string gamePck, string modsRoot, string workshopRoot, string settingsPath)
    {
        using var settings = ReadJson(settingsPath);
        var modSettings = settings.RootElement.GetProperty("mod_settings");
        var globalEnabled = modSettings.GetProperty("mods_enabled").GetBoolean();
        var gameEntries = modSettings.GetProperty("mod_list").EnumerateArray()
            .GroupBy(item => Identity(item.GetProperty("source").GetString()!, item.GetProperty("id").GetString()!),
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key,
                group => group.Select(item => globalEnabled && item.GetProperty("is_enabled").GetBoolean())
                    .Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);

        var manifests = FindManifests(modsRoot, Local).Concat(FindManifests(workshopRoot, Workshop)).ToArray();
        var log = AuditLogSilencer.Install();
        try
        {
            var recognized = SkinCatalog.ProbeSkinProviders(manifests.Select(item => item.Descriptor), gamePck);
            var recognizedIds = recognized.Select(item => (item.RootPath ?? "") + "\n" + (item.ManifestId ?? item.Id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            using var catalog = SkinCatalog.Build(gamePck, []);
            var protection = catalog.GlobalWriteProtection;
            var packs = FindPacks(modsRoot, Local).Concat(FindPacks(workshopRoot, Workshop)).ToArray();
            var manifestsByRoot = manifests.ToLookup(item => item.Descriptor.RootPath!, StringComparer.OrdinalIgnoreCase);
            var rows = new List<AuditRow>();
            foreach (var (source, pack) in packs)
            {
                var root = Path.GetDirectoryName(pack)!;
                var stem = Path.GetFileNameWithoutExtension(pack);
                var owners = manifestsByRoot[root].Where(item => item.Source == source).ToArray();
                var owner = owners.FirstOrDefault(item =>
                    string.Equals(item.Descriptor.PckPath, pack, StringComparison.OrdinalIgnoreCase) ||
                    stem.Equals(item.Descriptor.Id, StringComparison.OrdinalIgnoreCase) ||
                    stem.StartsWith(item.Descriptor.Id + ".", StringComparison.OrdinalIgnoreCase)) ??
                    (owners.Length == 1 ? owners[0] : null);
                var id = owner?.Descriptor.Id ?? stem;
                var isRecognized = recognizedIds.Contains(root + "\n" + id);
                var status = pack.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => segment.Contains(".sts2test_disabled", StringComparison.OrdinalIgnoreCase))
                    ? "disabled-copy" : gameEntries.GetValueOrDefault(Identity(source, id)) switch
                    {
                        [true] => "enabled",
                        [false] => "disabled",
                        [_, _] => "mixed",
                        _ => "unlisted"
                    };
                try
                {
                    using var archive = PckArchive.OpenForAudit(pack);
                    var plan = GlobalResourceWrites.InspectHighPriorityPack(archive, protection);
                    rows.Add(new(pack, id, source, status, isRecognized, archive.Paths.Count, plan.Stripped,
                        plan.OriginalCount, null));
                }
                catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or OverflowException)
                {
                    rows.Add(new(pack, id, source, status, isRecognized, 0,
                        new Dictionary<string, ForbiddenGlobalWrite>(), 0, exception.Message));
                }
            }

            Console.WriteLine("status\trecognized\tid\tsource\tentries\tstripped\toriginals\trejected\tpack");
            foreach (var row in rows.OrderBy(row => row.Status).ThenByDescending(row => row.Recognized)
                         .ThenBy(row => row.Id, StringComparer.OrdinalIgnoreCase).ThenBy(row => row.Path, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{row.Status}\t{row.Recognized}\t{row.Id}\t{row.Source}\t{row.Entries}\t" +
                    $"{row.Stripped}\t{row.Originals}\t{(row.Rejection == null ? 0 : 1)}\t{row.Path}");
                if (row.Rejection != null) Console.WriteLine($"Rejected {row.Path} — {row.Rejection}");
            }
            var recognizedRows = rows.Where(row => row.Recognized).ToArray();
            foreach (var group in recognizedRows.GroupBy(row => row.Status).OrderBy(group => group.Key))
                PrintSummary(group.Key, group);
            PrintSummary("recognized", recognizedRows);
            PrintSummary("all", rows);
            foreach (var row in recognizedRows.OrderByDescending(row => row.Stripped).ThenBy(row => row.Id).Take(10))
                Console.WriteLine($"TOP\t{row.Stripped}\t{row.Originals}\t{row.Id}\t{row.Path}");
            foreach (var row in rows)
                foreach (var (path, rule) in row.StrippedPaths)
                    Console.WriteLine($"DETAIL\t{row.Id}\t{rule}\t{path}\t{row.Path}");
            if (recognizedRows.Length == 0 || rows.Any(row => row.Rejection != null))
                throw new InvalidOperationException("The full pack audit requires at least one recognized pack and zero pack rejections.");
            Console.WriteLine("Global write pack audit passed.");
        }
        finally
        {
            log.UnpatchAll(log.Id);
        }
    }

    private static void PrintSummary(string name, IEnumerable<AuditRow> rows) =>
        Console.WriteLine($"SUMMARY\t{name}\tpacks={rows.Count()}\tstripped={rows.Sum(row => row.Stripped)}\t" +
                          $"originals={rows.Sum(row => row.Originals)}\trejected={rows.Count(row => row.Rejection != null)}");

    internal static void InspectOverlay(string gamePck, string pack, string id, string group, string scene)
    {
        var log = AuditLogSilencer.Install();
        try
        {
            var root = Path.GetDirectoryName(pack)!;
            var descriptor = new SkinModDescriptor(id, id, pack, false, root,
                File.Exists(SkinPackagePaths.Resolve(root, id, ".dll")));
            using var catalog = SkinCatalog.Build(gamePck, [descriptor]);
            var target = catalog.Groups.Single(candidate => candidate.Id.Equals(group, StringComparison.OrdinalIgnoreCase));
            var option = target.Options.First(candidate => candidate.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            foreach (var selection in new[] { option.Id, SkinCatalog.BaseOptionId })
            {
                var overlay = catalog.BuildRuntimeResourceOverlay(group, selection, [scene], "offline-audit",
                    includeProviderDependencies: selection != SkinCatalog.BaseOptionId);
                var protection = catalog.GlobalWriteProtection;
                var projected = SkinService.StripRuntimeSharedWrites(protection, overlay, group, selection);
                if (!projected.ResourcePaths.TryGetValue(scene, out var alias) ||
                    (!projected.Files.ContainsKey(alias) && !projected.Files.ContainsKey(alias + ".remap") &&
                     !projected.Files.ContainsKey(alias + ".import")))
                    throw new InvalidOperationException($"Runtime overlay lost its private scene: {scene}");
                var originals = projected.Files.Where(pair => protection.Decide(pair.Key, fromGame: false) != ForbiddenGlobalWrite.None &&
                    protection.MatchesGameEntry(pair.Key, pair.Value)).Select(pair => pair.Key).ToArray();
                Console.WriteLine($"OVERLAY\t{id}\t{group}\t{selection}\tfiles={projected.Files.Count}" +
                    $"\taliases={projected.SourceAliases.Count}\tstripped={overlay.Files.Count - projected.Files.Count}\toriginals={originals.Length}");
                foreach (var path in originals) Console.WriteLine($"ORIGINAL\t{path}");
                foreach (var path in overlay.Files.Keys.Except(projected.Files.Keys, StringComparer.OrdinalIgnoreCase))
                    Console.WriteLine($"STRIPPED\t{path}");
            }
        }
        finally
        {
            log.UnpatchAll(log.Id);
        }
    }

    private static IEnumerable<(string Source, string Path)> FindPacks(string root, string source) =>
        EligibleFiles(root, "*.pck").Select(path => (source, path));

    private static IEnumerable<Manifest> FindManifests(string root, string source)
    {
        foreach (var path in EligibleFiles(root, "*.json"))
        {
            if (new FileInfo(path).Length > 1024 * 1024) continue;
            Manifest? manifest;
            try
            {
                using var document = ReadJson(path);
                var json = document.RootElement;
                if (json.ValueKind != JsonValueKind.Object || !json.TryGetProperty("id", out var property) ||
                    property.ValueKind != JsonValueKind.String || property.GetString() is not { Length: > 0 } id ||
                    Entry.IsSelfModId(id) || !json.TryGetProperty("has_pck", out var hasPck))
                    continue;
                var folder = Path.GetDirectoryName(path)!;
                var pack = hasPck.ValueKind == JsonValueKind.True ? SkinPackagePaths.Resolve(folder, id, ".pck") : null;
                var gameplay = json.TryGetProperty("affects_gameplay", out var affectsGameplay) &&
                    affectsGameplay.ValueKind == JsonValueKind.True;
                var hasDll = json.TryGetProperty("has_dll", out var dll) && dll.ValueKind == JsonValueKind.True;
                manifest = new Manifest(source, new SkinModDescriptor(id, id, pack, gameplay, folder, hasDll));
            }
            catch (Exception exception) when (exception is JsonException or ArgumentException)
            {
                continue; // A settings/configuration JSON file is not a game manifest.
            }
            yield return manifest;
        }
    }

    private static IEnumerable<string> EligibleFiles(string root, string glob) =>
        Directory.EnumerateFiles(Path.GetFullPath(root), glob, SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar)
                .Contains("mods_isolated_20261117", StringComparer.OrdinalIgnoreCase));

    private static JsonDocument ReadJson(string path) => JsonDocument.Parse(File.ReadAllText(path).TrimStart('\uFEFF'));
    private static string Identity(string source, string id) => source + "\n" + id;
    private sealed record Manifest(string Source, SkinModDescriptor Descriptor);
    private sealed record AuditRow(string Path, string Id, string Source, string Status, bool Recognized,
        int Entries, IReadOnlyDictionary<string, ForbiddenGlobalWrite> StrippedPaths, int Originals, string? Rejection)
    {
        internal int Stripped => StrippedPaths.Count;
    }
}
