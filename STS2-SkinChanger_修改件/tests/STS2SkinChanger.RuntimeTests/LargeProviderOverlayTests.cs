using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class LargeProviderOverlayTests
{
    private static SkinChangerStorage _storage = null!;
    private static string _selected = "first";
    private static readonly Dictionary<string, string> Packs = new();
    private static readonly Dictionary<string, byte> Visible = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> Mounts = [];

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-large-provider-");
        using var storage = new SkinChangerStorage(root.FullName, root.FullName, "test");
        _storage = storage;
        var harmony = new Harmony("sc.tests.large-provider");
        var logging = AuditLogSilencer.Install();
        var writer = PckArchive.WriteOutput;
        var service = typeof(SkinService);
        var mutableFields = new[] { "MountedOverlayOwners", "MountedLargeRuntimeProviderPacks",
            "PromotedPackOrder", "MountedOverlayCache", "_mountedOverlayLayers", "_mountedOverlayBytes",
            "_nextMountedOverlayWarningBytes" };
        var restore = mutableFields.Select(name => Capture(AccessTools.Field(service, name))).ToArray();
        try
        {
            void Patch(Type type, string method, string prefix) => harmony.Patch(
                AccessTools.Method(type, method), prefix: new HarmonyMethod(typeof(LargeProviderOverlayTests), prefix));
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), "Storage"),
                prefix: new HarmonyMethod(typeof(LargeProviderOverlayTests), nameof(Storage)));
            Patch(service, "GetVisualSelection", nameof(Selection));
            Patch(typeof(SkinCatalog), "GetProviderResourcePackPaths", nameof(PackPaths));
            Patch(typeof(SkinCatalog), "ProviderRequiresCoherentRuntimePackage", nameof(Coherent));
            Patch(typeof(SkinChangerPaths), "LoadResourcePack", nameof(Mount));
            PckArchive.WriteOutput = null;

            var shared = Enumerable.Range(0, 24).Select(i => $"res://fixture/common-{i}.bin").ToArray();
            var privatePaths = Enumerable.Range(0, 1000).Select(i => $"res://fixture/private-{i}.bin").ToArray();
            string MakePack(string name, IEnumerable<string> paths, byte marker)
            {
                var path = Path.Combine(root.FullName, name + ".pck");
                PckArchive.Write(path, paths.ToDictionary(key => key, _ => new[] { marker }));
                return path;
            }
            Packs["first"] = MakePack("first", shared.Concat(privatePaths), 1).Replace('\\', '/');
            Packs["second"] = MakePack("second", shared, 2);
            var basePath = MakePack("game", shared.Append("res://fixture/unrelated.bin"), 3);
            using var game = PckArchive.Open(basePath);
            using var primary = PckArchive.Open(Packs["first"]);
            using var alternate = PckArchive.Open(Packs["second"]);
            using var catalog = SkinCatalog.Build(basePath, []);
            var group = new SkinGroup("regent", "Regent");
            group.Options.Add(new("first", "First", new Dictionary<string, ResourceAsset>(), IsRuntimeProvider: true));
            group.Options.Add(new("second", "Second", new Dictionary<string, ResourceAsset>(), IsRuntimeProvider: true));
            ((List<SkinGroup>)catalog.Groups).Add(group);
            var affected = new HashSet<string> { group.Id };
            var promote = AccessTools.Method(service, "MountSelectedLargeRuntimeProviderPacks");
            var mountDelta = AccessTools.Method(service, "MountOverlayDelta");
            var writes = new List<string>();
            PckArchive.WriteOutput = (path, _, write) => { writes.Add(path); write(); };
            var firstPlan = primary.Paths.ToDictionary(path => path, path => new ResourceFile(primary, path));
            // The selected provider owns its private files, but foreign canonical paths need restoring.
            foreach (var path in shared) firstPlan[path] = new ResourceFile(game, path);
            int Delta(IReadOnlyDictionary<string, ResourceFile> plan) =>
                (int)mountDelta.Invoke(null, [plan, "large-provider-fixture", "fixture failed"])!;
            void Promote(string selected)
            {
                _selected = selected;
                promote.Invoke(null, [catalog, affected, false, (Action)(() => { })]);
            }

            Promote("first");
            Require(Delta(firstPlan) == shared.Length && writes.Count == 1,
                "A promoted pack must write only real corrections, excluding its 1000 unchanged paths.");
            using (var correction = PckArchive.Open(writes.Single()))
                Require(correction.Paths.ToHashSet().SetEquals(shared), "Unexpected correction PCK entries.");
            Require(shared.All(path => Visible[path] == 3) && privatePaths.All(path => Visible[path] == 1),
                "The correction must restore every foreign canonical path.");

            Promote("second");
            Require(Delta(alternate.Paths.ToDictionary(path => path, path => new ResourceFile(alternate, path))) == 0 &&
                    shared.All(path => Visible[path] == 2), "The new provider must own the shared paths.");
            Promote("first");
            Require(Delta(firstPlan) == shared.Length && writes.Count == 1 && Mounts[^1] == writes[0],
                "Returning to the same correction must hit the same signature and remount without a write.");

            // Start from the second pack, whose visible bytes differ from the game on every shared path.
            Promote("second");
            Promote(SkinCatalog.BaseOptionId);
            Require(Delta(shared.ToDictionary(path => path, path => new ResourceFile(game, path))) == shared.Length &&
                    shared.All(path => Visible[path] == 3), "Switching to base must restore the shared paths.");
        }
        finally
        {
            PckArchive.WriteOutput = writer;
            harmony.UnpatchAll(harmony.Id);
            logging.UnpatchAll(logging.Id);
            foreach (var action in restore) action();
            Packs.Clear();
            Visible.Clear();
            Mounts.Clear();
            storage.Dispose();
            root.Delete(recursive: true);
        }
        Console.WriteLine("Large provider overlay passed: 24 corrections for 1024 paths, same-pack reuse and switch-away restoration.");
    }

    private static Action Capture(FieldInfo field)
    {
        var value = field.GetValue(null);
        if (value is System.Collections.IDictionary map)
        {
            var snapshot = new List<System.Collections.DictionaryEntry>();
            foreach (System.Collections.DictionaryEntry pair in map) snapshot.Add(pair);
            map.Clear();
            return () => { map.Clear(); foreach (var pair in snapshot) map.Add(pair.Key, pair.Value); };
        }
        if (value is List<string> order)
        {
            var snapshot = order.ToArray();
            order.Clear();
            return () => { order.Clear(); order.AddRange(snapshot); };
        }
        return () => field.SetValue(null, value);
    }

    private static bool Storage(ref SkinChangerStorage __result) { __result = _storage; return false; }
    private static bool Selection(ref string __result) { __result = _selected; return false; }
    private static bool PackPaths(string optionId, ref IReadOnlyList<string> __result)
    {
        __result = [Packs[optionId]];
        return false;
    }
    private static bool Coherent(ref bool __result) { __result = true; return false; }
    private static bool Mount(string path, bool replaceFiles, ref bool __result)
    {
        using var archive = PckArchive.Open(path);
        foreach (var key in archive.Paths)
            if (replaceFiles || !Visible.ContainsKey(key)) Visible[key] = archive.ReadFile(key)[0];
        Mounts.Add(path);
        __result = true;
        return false;
    }
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
