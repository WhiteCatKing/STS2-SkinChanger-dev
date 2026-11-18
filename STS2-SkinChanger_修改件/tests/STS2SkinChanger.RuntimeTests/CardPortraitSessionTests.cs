using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CardPortraitSessionTests
{
    private static readonly Dictionary<string, int> Loads = new(StringComparer.Ordinal);
    private static readonly Dictionary<Texture2D, int> Sizes = new(ReferenceEqualityComparer.Instance);
    private static readonly HashSet<Texture2D> Released = new(ReferenceEqualityComparer.Instance);
    private static readonly List<ResourceLoader.CacheMode> Modes = [];

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-card-portrait-session-");
        var harmony = new Harmony("sc.tests.card-portrait-session");
        var logging = AuditLogSilencer.Install();
        var service = typeof(SkinService);
        var catalogProperty = AccessTools.Property(service, nameof(SkinService.Catalog));
        var previousCatalog = catalogProperty.GetValue(null);
        var overlayStates = (IDictionary)AccessTools.Field(service, "IsolatedCardOverlayCache").GetValue(null)!;
        const string stateKey = "__base__\nbase";
        var oldState = overlayStates[stateKey];
        var writer = PckArchive.WriteOutput;
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

            PckArchive.WriteOutput = null;
            var gamePath = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(gamePath, new Dictionary<string, byte[]> { ["res://fixture.bin"] = [0] });
            using var catalog = SkinCatalog.Build(gamePath, []);
            catalogProperty.SetValue(null, catalog);
            var overlayType = AccessTools.Inner(service, "IsolatedCardOverlayState");
            var overlay = Activator.CreateInstance(overlayType, stateKey)!;
            AccessTools.Property(overlayType, "CanReuseExternalDependencies").SetValue(overlay, false);
            var paths = (Dictionary<string, string>)AccessTools.Property(overlayType, "ResourcePaths").GetValue(overlay)!;
            overlayStates[stateKey] = overlay;
            var requestType = AccessTools.Inner(service, "CardPortraitRequest");
            var getPortrait = AccessTools.Method(service, "GetOrLoadCardPortrait");
            Texture2D? Request(int index, string? requestKey = null)
            {
                var path = $"res://card-{index}.tres";
                paths[path] = $"res://isolated/card-{index}.tres";
                var request = Activator.CreateInstance(requestType, "regent", SkinCatalog.BaseOptionId,
                    path, requestKey ?? $"regent\n{index}", false, false)!;
                return (Texture2D?)getPortrait.Invoke(null, [request]);
            }

            var first = Request(0) ?? throw new InvalidOperationException("Fixture was not loaded.");
            for (var index = 1; index <= 65; index++)
                Require(Request(index) != null, "Fixture failed to fill the 64-entry request cache.");
            Require(ReferenceEquals(first, Request(0)) && ReferenceEquals(first, Request(0, "regent\nother-request")) &&
                    Loads["res://isolated/card-0.tres"] == 1 && Released.Count == 0,
                "Returning after LRU eviction or with another request key must reuse the same isolated texture.");
            AccessTools.Method(service, "ClearCardPortraitCache").Invoke(null, ["regent"]);
            Require(ReferenceEquals(first, Request(0)) && Loads["res://isolated/card-0.tres"] == 1,
                "Changing a group selection must invalidate request lookups without dropping live alias textures.");

            Require(Request(999) == null && Request(999, "regent\nlarge-another-request") == null &&
                    Loads["res://isolated/card-999.tres"] == 1 && Released.Count == 1,
                "An over-budget texture must be released on the game thread without allocating it again.");
            Require(Modes.All(mode => mode == ResourceLoader.CacheMode.IgnoreDeep),
                "Isolated loads must keep the existing cache mode for private dependencies.");
            AccessTools.Method(typeof(CardPortraitSessionCleanupPatch), "Postfix").Invoke(null, [true]);
            Require(Released.Count == 67 && Released.Contains(first), "Run cleanup did not explicitly release every owned texture.");
            Require(Request(0) is { } reloaded && !ReferenceEquals(reloaded, first) &&
                    Loads["res://isolated/card-0.tres"] == 2, "A new session must reload the alias.");
            SkinService.ResetCardPortraitSession();
            Require(Released.Count == 68, "Explicit reset did not release the new session's texture.");
        }
        finally
        {
            SkinService.ResetCardPortraitSession();
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
            root.Delete(recursive: true);
        }
        Console.WriteLine("Card portrait session passed: real LRU eviction, alias reuse, memory ceiling, explicit release and reload.");
    }

    private static bool Load(string path, ResourceLoader.CacheMode cacheMode, ref Texture2D __result)
    {
        Loads[path] = Loads.GetValueOrDefault(path) + 1;
        Modes.Add(cacheMode);
        __result = (Texture2D)RuntimeHelpers.GetUninitializedObject(typeof(CompressedTexture2D));
        GC.SuppressFinalize(__result);
        Sizes.Add(__result, path.EndsWith("999.tres", StringComparison.Ordinal) ? 8192 : 32);
        return false;
    }
    private static bool Size(Texture2D __instance, ref int __result) { __result = Sizes[__instance]; return false; }
    private static bool Id(GodotObject __instance, ref ulong __result)
    {
        __result = (uint)RuntimeHelpers.GetHashCode(__instance);
        return false;
    }
    private static bool Valid(GodotObject? instance, ref bool __result)
    {
        __result = instance is Texture2D texture && Sizes.ContainsKey(texture) && !Released.Contains(texture);
        return false;
    }
    private static bool Dispose(GodotObject __instance, bool disposing)
    {
        if (__instance is not Texture2D texture || !Sizes.ContainsKey(texture)) return true;
        Require(disposing && Released.Add(texture), "Owned texture released twice or through a finalizer.");
        return false;
    }
    private static bool Skip() => false;
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
