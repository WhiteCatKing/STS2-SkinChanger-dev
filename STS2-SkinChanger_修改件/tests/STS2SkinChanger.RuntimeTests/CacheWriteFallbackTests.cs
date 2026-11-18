using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CacheWriteFallbackTests
{
    private static SkinChangerStorage _storage = null!;

    internal static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "sc-write-fallback-" + Guid.NewGuid().ToString("N"));
        _storage = new SkinChangerStorage(root, root, "test");
        var harmony = new Harmony("sc.tests.cache-write-fallback");
        var previous = PckArchive.WriteOutput;
        try
        {
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), "Storage"),
                prefix: new HarmonyMethod(typeof(CacheWriteFallbackTests), nameof(Storage)));
            foreach (var name in new[] { "Info", "Warn", "Error" })
                harmony.Patch(AccessTools.Method(typeof(ModLog), name),
                    prefix: new HarmonyMethod(typeof(CacheWriteFallbackTests), nameof(Skip)));
            PckArchive.WriteOutput = null;
            var source = Path.Combine(root, "source.pck");
            PckArchive.Write(source, new Dictionary<string, byte[]>
            {
                ["res://test.bin"] = new byte[100], ["res://other.bin"] = new byte[1]
            });
            using var archive = PckArchive.Open(source);
            var budget = new CacheWriteBudget([Path.Combine(root, "overlay-cache")], availableBytes: _ => 0);
            PckArchive.WriteOutput = budget.Write;
            var rejected = false;
            try
            {
                AccessTools.Method(typeof(SkinService), "MountArchiveOverlay").Invoke(null,
                    [new Dictionary<string, ResourceFile> { ["res://test.bin"] = new(archive, "res://test.bin") },
                        "low-space-" + Guid.NewGuid().ToString("N"), "test failure"]);
            }
            catch (TargetInvocationException error) when (error.InnerException is CacheWriteRejectedException)
            {
                rejected = true;
            }
            if (!rejected || Directory.EnumerateFiles(root, "*.pck", SearchOption.AllDirectories).Count() != 1)
                throw new InvalidOperationException("Low-space overlay must reject before output or native mounting.");
            if (!AccessTools.Method(typeof(SkinService), "MountOverlayCore").GetMethodBody()!.ExceptionHandlingClauses
                .Any(clause => clause.Flags == ExceptionHandlingClauseOptions.Clause && clause.CatchType == typeof(CacheWriteRejectedException)))
                throw new InvalidOperationException("Overlay rejection must reach the selection rollback transaction.");
            Console.WriteLine("Cache write fallback passed: real overlay rejects before native mount and preserves rollback exception.");
        }
        finally
        {
            PckArchive.WriteOutput = previous;
            harmony.UnpatchAll(harmony.Id);
            _storage.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static bool Storage(ref SkinChangerStorage __result) { __result = _storage; return false; }
    private static bool Skip() => false;
}
