using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using project_export_mod;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CardPortraitSessionTests
{
    private static readonly Dictionary<string, int> Loads = new(StringComparer.Ordinal);
    private static readonly ConditionalWeakTable<Texture2D, TextureSize> Sizes = new();
    private static readonly HashSet<Texture2D> Released = new(ReferenceEqualityComparer.Instance);
    private static readonly List<ResourceLoader.CacheMode> Modes = [];
    private static int _gameThread;
    private static int _wrongThreadReleases;

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-card-portrait-session-");
        var harmony = new Harmony("sc.tests.card-portrait-session");
        var logging = AuditLogSilencer.Install();
        var service = typeof(SkinService);
        var catalogProperty = AccessTools.Property(service, nameof(SkinService.Catalog));
        var previousCatalog = catalogProperty.GetValue(null);
        var overlayStates = (IDictionary)AccessTools.Field(service, "IsolatedCardOverlayCache").GetValue(null)!;
        const string provider = "AI_silent_NSFW_MOD";
        const string stateKey = $"silent\n{provider}\nprovider";
        var oldState = overlayStates[stateKey];
        var writer = PckArchive.WriteOutput;
        _gameThread = System.Environment.CurrentManagedThreadId;
        try
        {
            void Patch(MethodInfo method, string prefix) => harmony.Patch(
                method, prefix: new HarmonyMethod(typeof(CardPortraitSessionTests), prefix));
            var load = typeof(ResourceLoader).GetMethods().Single(method => method.Name == "Load" && method.IsGenericMethodDefinition);
            Patch(load.MakeGenericMethod(typeof(Texture2D)), nameof(Load));
            Patch(AccessTools.Method(typeof(Texture2D), nameof(Texture2D.GetWidth)), nameof(Size));
            Patch(AccessTools.Method(typeof(Texture2D), nameof(Texture2D.GetHeight)), nameof(Size));
            Patch(AccessTools.Method(typeof(GodotObject), nameof(GodotObject.GetInstanceId)), nameof(Id));
            Patch(AccessTools.Method(typeof(GodotObject), nameof(GodotObject.IsInstanceValid)), nameof(Valid));
            Patch(AccessTools.Method(typeof(GodotObject), "Dispose", [typeof(bool)]), nameof(Dispose));
            Patch(AccessTools.Method(typeof(RuntimeLoadSafety), "RequireHeadroom"), nameof(Skip));
            Patch(AccessTools.Method(typeof(CardPortraitSessionCache), "EnsureReleasePump"), nameof(Skip));
            Patch(AccessTools.PropertyGetter(typeof(AtlasTexture), nameof(AtlasTexture.Atlas)), nameof(SkipAtlas));
            Patch(AccessTools.PropertySetter(typeof(TextureRect), nameof(TextureRect.Texture)), nameof(SetTexture));

            var enterTree = AccessTools.Method(typeof(NCard), nameof(NCard._EnterTree));
            var updateVisuals = AccessTools.Method(typeof(NCard), nameof(NCard.UpdateVisuals));
            foreach (var (target, callback) in new[]
                     {
                         (enterTree, nameof(PortraitPatch.EnterTreePostfix)),
                         (updateVisuals, nameof(PortraitPatch.UpdateVisualsPostfix))
                     })
            {
                Patch(target, nameof(Skip));
                harmony.Patch(target, postfix: new HarmonyMethod(typeof(PortraitPatch), callback));
            }
            CheckManagedPortraitPatch(enterTree, updateVisuals);

            PckArchive.WriteOutput = null;
            var gamePath = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(gamePath, new Dictionary<string, byte[]> { ["res://fixture.bin"] = [0] });
            using var catalog = SkinCatalog.Build(gamePath, []);
            catalogProperty.SetValue(null, catalog);
            var overlayType = AccessTools.Inner(service, "IsolatedCardOverlayState");
            var overlay = Activator.CreateInstance(overlayType, nonPublic: true)!;
            AccessTools.Property(overlayType, "CanReuseExternalDependencies").SetValue(overlay, false);
            var paths = (Dictionary<string, string>)AccessTools.Property(overlayType, "ResourcePaths").GetValue(overlay)!;
            overlayStates[stateKey] = overlay;
            var requestType = AccessTools.Inner(service, "CardPortraitRequest");
            var getPortrait = AccessTools.Method(service, "GetOrLoadCardPortrait");
            Texture2D? Request(int index, string? requestKey = null)
            {
                var path = $"res://card-{index}.tres";
                paths[path] = $"res://isolated/card-{index}.tres";
                var request = Activator.CreateInstance(requestType, "silent", provider,
                    path, requestKey ?? $"silent\n{index}", true, false)!;
                return (Texture2D?)getPortrait.Invoke(null, [request]);
            }

            var retiredTexture = RunAcrossCleanup(Request, enterTree, updateVisuals);
            var drain = AccessTools.Method(typeof(CardPortraitSessionCache), "DrainRetiredTextures");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Require(!retiredTexture.IsAlive && _wrongThreadReleases == 0,
                "An unreachable portrait must be queued by managed GC without calling Godot on the finalizer thread.");
            for (var frame = 0; frame < 4; frame++)
                drain.Invoke(null, null);
            Require(Released.Count == 68 && _wrongThreadReleases == 0,
                "After the external reference is dropped, the frame pump must explicitly release every retired texture.");

            // #229：连续浏览多个卡牌总览页签超过 512 MiB 常驻预算时，必须按 LRU 淘汰旧项并继续加载新卡图，
            // 不能卡死返回 null 导致后续猎宝/愚者等卡面全部回退原版。
            var releasedBeforeSweep = Released.Count;
            var session = (CardPortraitSessionCache)AccessTools.Field(service, "SessionCardPortraits").GetValue(null)!;
            for (var index = 1000; index < 1020; index++)
            {
                // 2048x2048x8 = 32 MiB，20 张合计 640 MiB > 512 MiB 预算。
                Require(Request(index) != null,
                    $"第 {index - 999} 张正常高清卡图不应因累计缓存满 512 MiB 而被永久拒绝。");
                Require(session.EstimatedBytes <= RuntimeLoadSafety.ResidentCacheBudget,
                    "LRU 淘汰后常驻估算字节不得超过 512 MiB 上限。");
            }
            Require(Released.Count == releasedBeforeSweep,
                "会话内 LRU 淘汰不能强行 Dispose 可能仍在屏幕上显示的纹理。");
            // 已被淘汰的早期卡图不得被永久负缓存，切回时可正常重新加载。
            Require(Request(1000, "regent\nrevisit-1000") != null &&
                    Loads["res://isolated/card-1000.tres"] == 2,
                "因预算被 LRU 淘汰的早期卡图再次访问时必须能正常重新加载。");
            SkinService.ResetCardPortraitSession();
            Require(session.Count == 0 && session.EstimatedBytes == 0 &&
                    _wrongThreadReleases == 0,
                "会话结束必须清空驻留预算，禁止在终结器线程释放淘汰或退役的贴图。");
        }
        finally
        {
            PortraitPatch.Clear();
            SkinService.ResetCardPortraitSession();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            for (var frame = 0; frame < 4; frame++)
                AccessTools.Method(typeof(CardPortraitSessionCache), "DrainRetiredTextures").Invoke(null, null);
            if (oldState == null) overlayStates.Remove(stateKey);
            else overlayStates[stateKey] = oldState;
            catalogProperty.SetValue(null, previousCatalog);
            PckArchive.WriteOutput = writer;
            harmony.UnpatchAll(harmony.Id);
            logging.UnpatchAll(logging.Id);
            Loads.Clear();
            Sizes.Clear();
            Released.Clear();
            Modes.Clear();
            _wrongThreadReleases = 0;
            root.Delete(recursive: true);
        }
        Console.WriteLine("Card portrait session passed: rejoin after a third-party cache, game-thread disposal, LRU and alias reuse.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RunAcrossCleanup(
        Func<int, string?, Texture2D?> request, MethodInfo enterTree, MethodInfo updateVisuals)
    {
        var first = request(0, null) as AtlasTexture ?? throw new InvalidOperationException("Expected atlas portrait.");
        for (var index = 1; index <= 65; index++)
            Require(request(index, null) != null, "Fixture failed to fill the 64-entry request cache.");
        Require(ReferenceEquals(first, request(0, null)) && ReferenceEquals(first, request(0, "silent\nother-request")) &&
                Loads["res://isolated/card-0.tres"] == 1 && Released.Count == 0,
            "A fast-cache eviction must still reuse the same isolated texture.");
        AccessTools.Method(typeof(SkinService), "ClearCardPortraitCache").Invoke(null, ["silent"]);
        Require(ReferenceEquals(first, request(0, null)) && Loads["res://isolated/card-0.tres"] == 1,
            "Changing a group selection must only invalidate request lookups.");

        Require(request(999, null) == null && request(999, "silent\nlarge-another-request") == null &&
                Loads["res://isolated/card-999.tres"] == 1 && Released.Count == 1,
            "A never-published over-budget texture must be released on the game thread exactly once.");
        Require(Modes.All(mode => mode == ResourceLoader.CacheMode.IgnoreDeep),
            "Isolated loads must retain the existing cache mode for their dependencies.");

        PortraitPatch.Remember(first);
        var initialNode = (NCard)RuntimeHelpers.GetUninitializedObject(typeof(NCard));
        GC.SuppressFinalize(initialNode);
        enterTree.Invoke(initialNode, null);

        // Invoke the production RunManager.CleanUp postfix with a previously active run.
        AccessTools.Method(typeof(CardPortraitSessionCleanupPatch), "Postfix").Invoke(null, [true]);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        AccessTools.Method(typeof(CardPortraitSessionCache), "DrainRetiredTextures").Invoke(null, null);
        Require(!Released.Contains(first), "A third-party dictionary still owns the managed wrapper.");

        var rejoinedNode = (NCard)RuntimeHelpers.GetUninitializedObject(typeof(NCard));
        GC.SuppressFinalize(rejoinedNode);
        enterTree.Invoke(rejoinedNode, null);
        var visualsArgs = updateVisuals.GetParameters().Select(parameter => parameter.ParameterType.IsValueType
            ? Activator.CreateInstance(parameter.ParameterType)
            : null).ToArray();
        updateVisuals.Invoke(rejoinedNode, visualsArgs);
        Require(request(0, null) is { } reloaded && !ReferenceEquals(reloaded, first) &&
                Loads["res://isolated/card-0.tres"] == 2, "The new run must load its own session entry.");

        SkinService.ResetCardPortraitSession();
        Require(!Released.Contains(first), "Resetting a second time still cannot invalidate the provider reference.");
        PortraitPatch.Clear();
        return new WeakReference(first);
    }

    private static void CheckManagedPortraitPatch(MethodInfo enterTree, MethodInfo updateVisuals)
    {
        var loader = typeof(ManagedSkinModLoader);
        var type = AccessTools.Inner(loader, "ProviderPatch");
        var kinds = AccessTools.Inner(loader, "ProviderPatchKind");
        var policy = AccessTools.Method(loader, "IsManagedCardTexturePatch");
        var provider = new SkinProviderProbe("AI_silent_NSFW_MOD", "fixture", 0, 89, 0, 0, 0, false,
            HasResourceBackedCosmetics: true);

        bool Owns(MethodInfo target, string callback, SkinProviderProbe probe)
        {
            var patch = Activator.CreateInstance(type, target, AccessTools.Method(typeof(PortraitPatch), callback),
                Enum.Parse(kinds, "Postfix"), Priority.Normal, Array.Empty<string>(), Array.Empty<string>(), false);
            return (bool)policy.Invoke(null, [patch, probe])!;
        }

        Require(Owns(enterTree, nameof(PortraitPatch.EnterTreePostfix), provider) &&
                Owns(updateVisuals, nameof(PortraitPatch.UpdateVisualsPostfix), provider),
            "The managed provider's two known texture writers must be stripped.");
        Require(!Owns(updateVisuals, nameof(PortraitPatch.OtherPostfix), provider) &&
                !Owns(enterTree, nameof(PortraitPatch.EnterTreePostfix), provider with { CardAssetCount = 0 }) &&
                !Owns(enterTree, nameof(PortraitPatch.EnterTreePostfix), provider with { Id = "other" }),
            "Other behavior, other providers and providers without known artwork must keep their callbacks.");
    }

    private static bool Load(string path, ResourceLoader.CacheMode cacheMode, ref Texture2D __result)
    {
        Loads[path] = Loads.GetValueOrDefault(path) + 1;
        Modes.Add(cacheMode);
        __result = (Texture2D)RuntimeHelpers.GetUninitializedObject(path.EndsWith("card-0.tres", StringComparison.Ordinal)
            ? typeof(AtlasTexture)
            : typeof(CompressedTexture2D));
        Sizes.Add(__result, new TextureSize(path.EndsWith("999.tres", StringComparison.Ordinal)
            ? 8192
            : path.Contains("card-10", StringComparison.Ordinal)
                ? 2048
                : 32));
        return false;
    }
    private static bool Size(Texture2D __instance, ref int __result) { __result = Sizes.GetValue(__instance, _ => throw new InvalidOperationException("Missing texture size.")).Pixels; return false; }
    private static bool Id(GodotObject __instance, ref ulong __result)
    {
        __result = (uint)RuntimeHelpers.GetHashCode(__instance);
        return false;
    }
    private static bool Valid(GodotObject? instance, ref bool __result)
    {
        __result = instance is Texture2D texture && Sizes.TryGetValue(texture, out _) && !Released.Contains(texture);
        return false;
    }
    private static bool Dispose(GodotObject __instance, bool disposing)
    {
        if (__instance is not Texture2D texture) return true;
        if (!disposing || System.Environment.CurrentManagedThreadId != _gameThread)
            Interlocked.Increment(ref _wrongThreadReleases);
        else
            Require(Released.Add(texture), "Owned texture released twice.");
        return false;
    }

    private static bool SkipAtlas(ref Texture2D? __result) { __result = null; return false; }

    private static bool SetTexture(Texture2D __0)
    {
        ObjectDisposedException.ThrowIf(Released.Contains(__0), __0);
        return false;
    }

    private sealed record TextureSize(int Pixels);

    private static bool Skip() => false;
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}

namespace project_export_mod
{
    // Models the reported native-first provider, whose cache outlives both card nodes and runs.
    internal static class PortraitPatch
    {
        private static readonly Dictionary<string, Texture2D> OriginalPortraits = new();
        private static TextureRect? _portraitNode;

        internal static void Remember(Texture2D texture)
        {
            OriginalPortraits["CARD.STRIKE_DEFECT"] = texture;
            _portraitNode = (TextureRect)RuntimeHelpers.GetUninitializedObject(typeof(TextureRect));
            GC.SuppressFinalize(_portraitNode);
        }

        internal static void Clear()
        {
            OriginalPortraits.Clear();
            _portraitNode = null;
        }

        internal static void EnterTreePostfix() => RestoreDefaultState();
        internal static void UpdateVisualsPostfix() => RestoreDefaultState();
        internal static void OtherPostfix() { }

        private static void RestoreDefaultState() => _portraitNode!.Texture = OriginalPortraits["CARD.STRIKE_DEFECT"];
    }
}