using System.Buffers.Binary;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class RuntimeSafetyTests
{
    internal static void Run()
    {
        var texture = new byte[52];
        "GST2"u8.CopyTo(texture);
        BinaryPrimitives.WriteUInt16LittleEndian(texture.AsSpan(40), 4096);
        BinaryPrimitives.WriteUInt16LittleEndian(texture.AsSpan(42), 2048);
        Require(RuntimeLoadSafety.EstimateTextureBytes(new Dictionary<string, byte[]> { ["a.ctex"] = texture }) == 4096L * 2048 * 8,
            "Budget must use decoded dimensions, not compressed file length.");
        var png = new byte[24]; png[0] = 0x89; "PNG"u8.CopyTo(png.AsSpan(1)); "IHDR"u8.CopyTo(png.AsSpan(12));
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(16), 1024); BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(20), 512);
        Require(RuntimeLoadSafety.EstimateTextureBytes(new Dictionary<string, byte[]> { ["raw.png"] = png }) == 1024L * 512 * 8,
            "Raw provider PNG bypassed the memory estimate.");
        var overlay = new Dictionary<string, byte[]>
        {
            ["res://raw.png"] = png,
            ["res://raw.png.import"] = System.Text.Encoding.UTF8.GetBytes("[remap]\npath=\"res://decoded.ctex\"\n"),
            ["res://decoded.ctex"] = texture
        };
        Require(RuntimeLoadSafety.EstimateOverlayTextureBytes(overlay) == 4096L * 2048 * 8 &&
                RuntimeLoadSafety.EstimateOverlayTextureBytes(overlay, ["res://raw.png", "res://decoded.ctex"]) == 4096L * 2048 * 8,
            "The same imported texture must not be charged again for its source PNG.");
        overlay["res://raw.png.import"] = System.Text.Encoding.UTF8.GetBytes("[remap]\npath=\"res://missing.ctex\"\n");
        Require(RuntimeLoadSafety.EstimateOverlayTextureBytes(overlay) == (4096L * 2048 + 1024L * 512) * 8,
            "Unknown or missing imports must not exempt source bytes.");
        try
        {
            RuntimeLoadSafety.EstimateTextureBytes(new Dictionary<string, byte[]> { ["broken.ctex"] = "GST2"u8.ToArray() });
            throw new Exception("Truncated texture was accepted.");
        }
        catch (InvalidDataException) { }
        Require(!RuntimeLoadSafety.HasHeadroom(400L * 1024 * 1024, 1), "Native reserve missing.");
        Require(!RuntimeLoadSafety.HasHeadroom(600L * 1024 * 1024, 200L * 1024 * 1024), "Expected decode must fit after reserve.");
        Require(RuntimeLoadSafety.HasHeadroom(1024L * 1024 * 1024, 32L * 1024 * 1024), "Normal loading blocked.");
        const long mib = 1024 * 1024;
        Require(RuntimeLoadSafety.HasPreparationHeadroom(520 * mib, 1024 * mib) &&
                !RuntimeLoadSafety.HasHeadroom(520 * mib, 64 * mib),
            "Overlay staging can use available commit while native decoding still needs physical memory.");
        Require(!RuntimeLoadSafety.HasPreparationHeadroom(511 * mib, 1024 * mib) &&
                !RuntimeLoadSafety.HasPreparationHeadroom(1024 * mib, 575 * mib) &&
                RuntimeLoadSafety.HasPreparationHeadroom(512 * mib, 576 * mib),
            "Staging must keep the physical floor and enough commit for both reserve and managed buffers.");
        var circuit = new ResourceFailureCircuit();
        var now = DateTime.UtcNow;
        circuit.Fail("broken", now);
        Require(circuit.IsBlocked("broken", now.AddSeconds(29)) && !circuit.IsBlocked("broken", now.AddSeconds(31)), "Failure retry interval wrong.");
        for (var i = 0; i < 1000; i++) circuit.Fail("bad-" + i, now);
        Require(((IDictionary)AccessTools.Field(typeof(ResourceFailureCircuit), "_failures").GetValue(circuit)!).Count <= 256,
            "Failure ledger is unbounded.");
        var first = new Link();
        var second = new Link();
        first.Next = second; second.Next = first;
        Require(BoundedTraversal.Walk(first, node => node.Next == null ? [] : new[] { node.Next }).Count() == 2,
            "Traversal failed to break a cycle.");
        var tail = first;
        for (var i = 0; i < 20000; i++) { tail.Next = new Link(); tail = tail.Next; }
        try
        {
            _ = BoundedTraversal.Walk(first, node => node.Next == null ? [] : new[] { node.Next }).Count();
            throw new Exception("Deep graph ignored its limit.");
        }
        catch (AppearanceResourceUnavailableException) { }

        var bundles = (IDictionary)AccessTools.Field(typeof(SkinService), "RuntimeResourceBundles").GetValue(null)!;
        var cache = (IDictionary)AccessTools.Field(typeof(SkinService), "RuntimeResourceCache").GetValue(null)!;
        var bundleType = typeof(SkinService).GetNestedType("RuntimeResourceBundleState", BindingFlags.NonPublic)!;
        for (var i = 0; i < 12; i++)
        {
            var resource = (Resource)RuntimeHelpers.GetUninitializedObject(typeof(Resource));
            GC.SuppressFinalize(resource);
            cache.Add("test-" + i, resource);
            bundles.Add("test-" + i, Activator.CreateInstance(bundleType,
                [new Dictionary<string, Resource> { ["root"] = resource }, 256L * 1024 * 1024])!);
        }
        AccessTools.Method(typeof(SkinService), "TrimRuntimeResidency").Invoke(null, ["test-11"]);
        Require(bundles.Contains("test-11") && bundles.Count == 2 && cache.Count == 2,
            "Byte budget must evict old bundle and secondary references while keeping current root.");
        bundles.Clear();
        cache.Clear();

        var guard = new Harmony("sc.tests.runtime-safety");
        try
        {
            guard.Patch(AccessTools.Method(typeof(RuntimeLoadSafety), "RequireHeadroom"), prefix: new HarmonyMethod(typeof(RuntimeSafetyTests), nameof(Skip)));
            var load = typeof(ResourceLoader).GetMethods().Single(m => m.Name == "Load" && m.IsGenericMethodDefinition);
            guard.Patch(load.MakeGenericMethod(typeof(Resource)), prefix: new HarmonyMethod(typeof(RuntimeSafetyTests), nameof(MissingResource)));
            try
            {
                AccessTools.Method(typeof(SkinService), "LoadCheckedRuntimeResource").Invoke(null, ["res://missing.tres", false, 0L]);
                throw new Exception("Missing resource continued to native model creation.");
            }
            catch (TargetInvocationException error) when (error.InnerException is InvalidOperationException) { }
            var refresh = AccessTools.Method(typeof(STS2SkinChanger.Ui.ContextualSkinControls), "RebuildCharacterDisplay");
            Require(refresh.GetMethodBody()!.ExceptionHandlingClauses.Any(clause => clause.Flags == ExceptionHandlingClauseOptions.Clause &&
                clause.CatchType == typeof(AppearanceResourceUnavailableException)), "Native failure must not fall through to a second load path.");
            Require((double)AccessTools.Field(typeof(SkinPreloader), "StartupGateTimeoutSeconds").GetRawConstantValue()! == 60 &&
                (int)AccessTools.Field(typeof(SkinPreloader), "MaxInFlight").GetRawConstantValue()! == 1,
                "Startup timeout or native request serialization changed.");
            var file = Path.Combine(Path.GetTempPath(), "sc-prefetch-" + Guid.NewGuid().ToString("N") + ".pck");
            try
            {
                File.WriteAllBytes(file, new byte[128 * 1024]);
                CharacterPackPrefetch.Schedule([file]);
                for (var i = 0; i < 200 && !CharacterPackPrefetch.IsWarm(file); i++) Thread.Sleep(10);
                Require(CharacterPackPrefetch.IsWarm(file), "Background prefetch did not finish bounded file work.");
                File.AppendAllText(file, "changed");
                Require(!CharacterPackPrefetch.IsWarm(file), "Changed package incorrectly kept its warm hint.");
                CharacterPackPrefetch.Cancel();
                CharacterPackPrefetch.Schedule([file]);
                Require(!CharacterPackPrefetch.IsWarm(file), "Exit cancellation allowed new background work.");
            }
            finally { File.Delete(file); }
        }
        finally { guard.UnpatchAll(guard.Id); }
        QuitBoundaryTests.Run();
        CacheWriteFallbackTests.Run();
        Console.WriteLine("Runtime safety passed: decoded texture budgets, reserve, bounded circuit, weighted eviction and missing-resource fail-fast.");
    }

    private static bool Skip() => false;
    private sealed class Link { internal Link? Next; }
    private static bool MissingResource(ref Resource? __result) { __result = null; return false; }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
