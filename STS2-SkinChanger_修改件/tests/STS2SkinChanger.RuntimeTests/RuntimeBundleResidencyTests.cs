using System.Collections;
using System.Reflection;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class RuntimeBundleResidencyTests
{
    internal static void Run()
    {
        Require(SkinCatalog.HasStagedRuntimeVisuals(["res://pack/s1_model_data.tres", "res://pack/s2_model_data.tres"]),
            "Numbered stage roots were not recognized.");
        Require(SkinCatalog.HasStagedRuntimeVisuals(["res://pack/select/s1/model.spskel", "res://pack/select/s4/model.spskel"]),
            "Stage directories were not recognized.");
        Require(SkinCatalog.HasStagedRuntimeVisuals(["res://battlewear/rest_stage2.skel.import", "res://battlewear/rest_stage3.skel"]),
            "Imported stage siblings were not recognized.");
        Require(SkinCatalog.HasStagedRuntimeVisuals(["res://custom/healthy/figure.res", "res://custom/damaged/figure.res"]),
            "A new provider should be recognized by structural stage evidence, independent of its name.");
        Require(!SkinCatalog.HasStagedRuntimeVisuals(["res://plain/s1_model_data.tres", "res://plain/s1_model_data.tres.remap",
                "res://different/s2_model_data.tres"]),
            "A single stage and a redirect must stay on the original cache path.");

        var pool = new RuntimeBundleResidency<object>(30, 3);
        var first = new object();
        var second = new object();
        var third = new object();
        Require(pool.Store("a-1", "family-a", first, 10, _ => true) &&
                pool.Store("a-2", "family-a", second, 10, _ => true) &&
                pool.Store("a-3", "family-a", third, 10, _ => true), "Three loaded stages should fit.");

        pool.Prefer("family-a");
        Require(!pool.Store("remote", "remote-family", new object(), 10, _ => true) && pool.Count == 3,
            "A peer may not displace the local stage family when the local working set fills the pool.");
        pool.Prefer("family-b"); // A subsequent local selection can evict idle items.

        var service = typeof(SkinService);
        var normal = (IDictionary)AccessTools.Field(service, "RuntimeResourceBundles").GetValue(null)!;
        var stagePool = AccessTools.Field(service, "StageRuntimeBundles").GetValue(null)!;
        var stagePoolType = stagePool.GetType();
        var bundleType = AccessTools.Inner(service, "RuntimeResourceBundleState");
        var stageBundle = Activator.CreateInstance(bundleType, new Dictionary<string, Resource>(), 748L * 1024 * 1024)!;
        AccessTools.Method(stagePoolType, "Store").Invoke(stagePool, ["fixture-actual", "a", stageBundle, 748L * 1024 * 1024, (Func<long, bool>)(_ => true)]);
        try
        {
            for (var index = 0; index < 10; index++)
                normal.Add("fixture-" + index,
                    Activator.CreateInstance(bundleType, new Dictionary<string, Resource>(), 200L * 1024 * 1024)!);
            AccessTools.Method(typeof(SkinService), "TrimRuntimeResidency").Invoke(null, ["fixture-9"]);
            Require(ReferenceEquals(stageBundle, AccessTools.Method(stagePoolType, "Find").Invoke(stagePool, ["fixture-actual"])),
                "The production ordinary trim must not touch the stage pool.");
            Require(ReferenceEquals(first, pool.Find("a-1")) &&
                    ReferenceEquals(second, pool.Find("a-2")) &&
                    ReferenceEquals(third, pool.Find("a-3")),
                "Ordinary bundle churn must not evict the selected family's stages.");
        }
        finally
        {
            AccessTools.Method(stagePoolType, "Forget").Invoke(stagePool, ["fixture-actual"]);
            foreach (var key in normal.Keys.Cast<string>().Where(key => key.StartsWith("fixture-")).ToArray())
                normal.Remove(key);
        }

        using var local = pool.Hold("a-1")!;
        using var remote = pool.Hold("a-1")!;
        using var preview = pool.Hold("a-3")!;
        local.Dispose();
        Require(pool.Store("b-1", "family-b", new object(), 10, _ => true) &&
                pool.Find("a-2") == null && pool.Find("a-1") == first && pool.Find("a-3") == third &&
                pool.Bytes == 30 && pool.Count == 3,
            "Only idle bundles may return their budget; disposing one of two owners is insufficient.");
        using var party = pool.Hold("b-1")!;
        Require(!pool.Store("b-2", "family-b", new object(), 10, _ => true) && pool.Count == 3,
            "When all slots are held, the new stage may load without being retained.");
        pool.TrimForPressure();
        Require(pool.Count == 3, "Pressure cleanup may not drop held entries.");
        remote.Dispose();
        pool.TrimForPressure();
        Require(pool.Find("a-1") == null && pool.Find("a-3") == third,
            "The last owner frees only its own bundle for eviction.");

        pool.Forget("a-3");
        Require(pool.Find("a-3") == null && pool.Bytes == 20,
            "Invalidating a held bundle must stop serving it while still charging its live owner.");
        preview.Dispose();
        Require(pool.Bytes == 10, "The stale held bundle must leave the budget after the holder releases it.");

        CheckChangedStamp();
        Console.WriteLine("Runtime bundle residency passed: staged packs, ordinary pressure, independent owners and stale sources.");
    }

    private static void CheckChangedStamp()
    {
        var file = Path.GetTempFileName();
        var service = typeof(SkinService);
        var prepared = (IDictionary)AccessTools.Field(service, "PreparedRuntimeOverlays").GetValue(null)!;
        var last = (IDictionary)AccessTools.Field(service, "LastPreparedRuntimeOverlays").GetValue(null)!;
        var pool = AccessTools.Field(service, "StageRuntimeBundles").GetValue(null)!;
        var poolType = pool.GetType();
        const string key = "fixture-stage-source";
        string? residentKey = null;
        const string group = "fixture-damage";
        try
        {
            CardFileStamp stamp;
            using (var stream = File.OpenRead(file))
                stamp = CardFileStamp.From(stream);
            var source = Activator.CreateInstance(AccessTools.Inner(service, "StageBundleSource"), group,
                new[] { stamp })!;
            var overlayType = AccessTools.Inner(service, "PreparedRuntimeOverlay");
            var overlay = Activator.CreateInstance(overlayType, key, key, null,
                new Dictionary<string, string>(), new HashSet<string>(), new HashSet<string>(), 0, 0L, 0L,
                false, Array.Empty<string>(), new Dictionary<string, long>(), false)!;
            AccessTools.Property(overlayType, "StageSource").SetValue(overlay, source);
            prepared.Add(key, overlay);
            last.Add(group, overlay);
            residentKey = (string)AccessTools.Property(overlayType, "StageResidencyKey").GetValue(overlay)!;
            var bundle = Activator.CreateInstance(AccessTools.Inner(service, "RuntimeResourceBundleState"),
                new Dictionary<string, Resource>(), 128L)!;
            AccessTools.Method(poolType, "Store").Invoke(pool, [residentKey, group, bundle, 128L, (Func<long, bool>)(_ => true)]);
            using var held = (IDisposable)AccessTools.Method(poolType, "Hold").Invoke(pool, [residentKey])!;
            var check = AccessTools.Method(service, "RequireStageSources");
            check.Invoke(null, [source, true]);
            File.AppendAllText(file, "replacement");
            try
            {
                check.Invoke(null, [source, true]);
                throw new Exception("A changed provider snapshot remained eligible.");
            }
            catch (TargetInvocationException error) when (error.InnerException is AppearanceResourceUnavailableException) { }
            Require(!prepared.Contains(key) && !last.Contains(group) &&
                    AccessTools.Method(poolType, "Find").Invoke(pool, [residentKey]) == null,
                "Stale aliases and their decoded bundle must not be offered again.");
            Require((long)AccessTools.Property(poolType, "Bytes").GetValue(pool)! == 128,
                "A stale bundle is still charged while the old display holds it.");
        }
        finally
        {
            prepared.Remove(key);
            last.Remove(group);
            if (residentKey != null) AccessTools.Method(poolType, "Forget").Invoke(pool, [residentKey]);
            File.Delete(file);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
