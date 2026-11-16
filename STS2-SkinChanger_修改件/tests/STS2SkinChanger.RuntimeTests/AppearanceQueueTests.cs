using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class AppearanceQueueTests
{
    internal static void Run()
    {
        CheckCanonicalBinding();
        var graph = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        { ["scene"] = ["skeleton", "texture"], ["skeleton"] = ["texture"], ["texture"] = [] };
        var order = RuntimeDependencyPlan.Order(graph);
        Require(Array.IndexOf(order, "texture") < Array.IndexOf(order, "skeleton") &&
                Array.IndexOf(order, "skeleton") < Array.IndexOf(order, "scene"), "Dependencies must precede consumers.");
        graph["texture"] = ["scene"];
        try { RuntimeDependencyPlan.Order(graph); throw new Exception("Cycle accepted."); }
        catch (AppearanceResourceUnavailableException) { }
        var now = Stopwatch.GetTimestamp();
        Require(CharacterPackPrefetch.CanSpeculate(now, 0, 0) &&
                !CharacterPackPrefetch.CanSpeculate(now, now, 0) &&
                !CharacterPackPrefetch.CanSpeculate(now, 0, 1), "Idle and foreground admission.");
        var motion = (InputEventMouseMotion)RuntimeHelpers.GetUninitializedObject(typeof(InputEventMouseMotion));
        GC.SuppressFinalize(motion);
        Require(!SkinLoadingInputObserver.CountsAsAction(motion), "Mouse movement must not suspend prefetch.");
        var osty = typeof(SkinService).Assembly.GetType("STS2SkinChanger.Ui.OstyModelPreviewControls", true)!;
        var show = AccessTools.Method(osty, "ShouldShowForCharacter");
        foreach (var id in new[] { "ironclad", "silent", "regent", "defect", "watcher", "necrobinder" })
            Require((bool)show.Invoke(null, [id])! == (id == "necrobinder"), "Osty ownership: " + id);

        var root = Path.Combine(Path.GetTempPath(), "SC_queue_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var a = Path.Combine(root, "a.pck"); var f = Path.Combine(root, "f.pck");
            File.WriteAllBytes(a, new byte[1024]); File.WriteAllBytes(f, new byte[1024]);
            Task urgent;
            using (CharacterPackPrefetch.HoldForeground())
            {
                CharacterPackPrefetch.Schedule([a]);
                urgent = CharacterPackPrefetch.WarmAsync([f]);
                Thread.Sleep(60);
                Require(!urgent.IsCompleted && !CharacterPackPrefetch.IsWarm(a), "No speculative I/O during foreground work.");
                CharacterPackPrefetch.NoteAction();
            }
            Require(urgent.Wait(TimeSpan.FromSeconds(5)), "Demand should complete despite recent activity.");
            Require(CharacterPackPrefetch.IsWarm(f) && !CharacterPackPrefetch.IsWarm(a), "Selected F must jump ahead of speculative A.");
            using var cancellation = new CancellationTokenSource();
            Task canceled;
            using (CharacterPackPrefetch.HoldForeground())
            {
                canceled = CharacterPackPrefetch.WarmAsync([a], cancellation.Token);
                cancellation.Cancel();
                try { canceled.GetAwaiter().GetResult(); throw new Exception("Cancellation ignored."); }
                catch (OperationCanceledException) { }
            }
        }
        finally { CharacterPackPrefetch.Cancel(); Directory.Delete(root, true); }
        Console.WriteLine("Appearance binding / dependency cycle / idle queue / urgent promotion / cancellation passed.");
    }
    private static readonly Dictionary<string, Resource> Cache = new();
    private static readonly Dictionary<Resource, string> Paths = new(ReferenceEqualityComparer.Instance);
    private static void CheckCanonicalBinding()
    {
        var harmony = new Harmony("sc.tests.appearance-cache");
        try
        {
            harmony.Patch(AccessTools.Method(typeof(ResourceLoader), "HasCached"), prefix: new(typeof(AppearanceQueueTests), nameof(HasCached)));
            var load = typeof(ResourceLoader).GetMethods().Single(m => m.Name == "Load" && m.IsGenericMethodDefinition).MakeGenericMethod(typeof(Resource));
            harmony.Patch(load, prefix: new(typeof(AppearanceQueueTests), nameof(Load)));
            harmony.Patch(AccessTools.Method(typeof(Resource), "TakeOverPath"), prefix: new(typeof(AppearanceQueueTests), nameof(TakeOver)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(Resource), "ResourcePath"), prefix: new(typeof(AppearanceQueueTests), nameof(ResourcePath)));
            harmony.Patch(AccessTools.Method(typeof(GodotObject), "IsInstanceValid"), prefix: new(typeof(AppearanceQueueTests), nameof(Valid)));
            var entry = typeof(SkinService).GetNestedType("CanonicalRuntimeResourceOwnershipEntry", BindingFlags.NonPublic)!;
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(entry))!;
            var bind = AccessTools.Method(typeof(SkinService), "BindRuntimeDependency");
            var reuse = AccessTools.Method(typeof(SkinService), "TryGetBoundRuntimeDependency");
            var a = FakeResource(); var b = FakeResource();
            foreach (var merchantPath in new[] { PrivateMerchantResourceIdentity.Body, PrivateMerchantResourceIdentity.Hand })
            {
                var liveMerchant = FakeResource(); var nextMerchant = FakeResource();
                var livePath = "private/live/" + merchantPath[6..];
                bind.Invoke(null, [merchantPath, livePath, liveMerchant, list]);
                bind.Invoke(null, [merchantPath, "private/next/" + merchantPath[6..], nextMerchant, list]);
                Require(liveMerchant.ResourcePath == livePath &&
                        PrivateMerchantResourceIdentity.ReadPath(liveMerchant) == merchantPath,
                    "Actual cache displacement must preserve the author's merchant target identity without changing its alias.");
            }
            const string canonical = "res://same.spskel";
            a.TakeOverPath(canonical);
            Require(reuse.Invoke(null, [canonical, "private/a"]) == null, "An untagged public cache must not be borrowed.");
            bind.Invoke(null, [canonical, "private/a", a, list]);
            Require(ReferenceEquals(reuse.Invoke(null, [canonical, "private/a"]), a), "Same immutable source should be reused.");
            Require(reuse.Invoke(null, [canonical, "private/b"]) == null, "Skin B must never borrow skin A's skeleton.");
            bind.Invoke(null, [canonical, "private/b", b, list]);
            Require(ReferenceEquals(Cache[canonical], b) && reuse.Invoke(null, [canonical, "private/a"]) == null,
                "Canonical ownership must follow selected source B.");
            Require(a.ResourcePath == "private/a", "Preview loading must not blank the live background's skeleton path.");
            var scopeType = typeof(SkinService).GetNestedType("CanonicalRuntimeResourceOwnershipScope", BindingFlags.NonPublic)!;
            var liveAliasOwner = FakeResource();
            liveAliasOwner.TakeOverPath("private/b");
            var temporary = FakeResource();
            var temporaryBindings = Activator.CreateInstance(typeof(List<>).MakeGenericType(entry))!;
            bind.Invoke(null, [canonical, "private/remote", temporary, temporaryBindings]);
            Require(liveAliasOwner.ResourcePath == "private/b" && b.ResourcePath.EndsWith("/private/b"),
                "Moving an old canonical resource must not steal an occupied private alias.");
            var remoteAliasOwner = FakeResource();
            remoteAliasOwner.TakeOverPath("private/remote");
            var newer = FakeResource();
            bind.Invoke(null, [canonical, "private/newer", newer, Activator.CreateInstance(typeof(List<>).MakeGenericType(entry))!]);
            var scope = (IDisposable)Activator.CreateInstance(scopeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, [temporaryBindings], null)!;
            scope.Dispose(); scope.Dispose();
            Require(b.ResourcePath == canonical && temporary.ResourcePath.EndsWith("/private/remote") &&
                    remoteAliasOwner.ResourcePath == "private/remote" && a.ResourcePath == "private/a" && newer.ResourcePath == "private/newer",
                "Remote scope must restore canonical ownership without blanking any live resource.");
            var live = new List<Resource> { b };
            for (var index = 0; index < 200; index++)
            {
                var next = FakeResource();
                var alias = $"res://sts2_skin_runtime/test/{index}/joanofarcironcladhpwear/s1_charselect.spskel";
                bind.Invoke(null, [canonical, alias, next, Activator.CreateInstance(typeof(List<>).MakeGenericType(entry))!]);
                live.Add(next);
                Require(live.All(resource => !string.IsNullOrWhiteSpace(resource.ResourcePath)),
                    "Repeated background/preview replacement erased a live path.");
            }
            Console.WriteLine("Native cache identity passed: displacement clears paths, live sources retained across 200 replacements and remote restoration.");
        }
        finally { harmony.UnpatchAll(harmony.Id); Cache.Clear(); Paths.Clear(); }
    }
    private static Resource FakeResource()
    {
        var value = (Resource)RuntimeHelpers.GetUninitializedObject(typeof(Resource));
        GC.SuppressFinalize(value); return value;
    }
    private static bool HasCached(string path, ref bool __result) { __result = Cache.ContainsKey(path); return false; }
    private static bool Load(string path, ref Resource? __result) { __result = Cache.GetValueOrDefault(path); return false; }
    private static bool ResourcePath(Resource __instance, ref string __result) { __result = Paths.GetValueOrDefault(__instance, ""); return false; }
    private static bool Valid(ref bool __result) { __result = true; return false; }
    private static bool TakeOver(Resource __instance, string path)
    {
        if (Paths.TryGetValue(__instance, out var old) && Cache.GetValueOrDefault(old) == __instance) Cache.Remove(old);
        if (Cache.TryGetValue(path, out var displaced) && !ReferenceEquals(displaced, __instance)) Paths[displaced] = "";
        Cache[path] = __instance; Paths[__instance] = path; return false;
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
