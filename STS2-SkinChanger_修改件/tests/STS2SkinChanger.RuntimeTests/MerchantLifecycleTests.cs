using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class MerchantLifecycleTests
{
    private static string _scenePath = "";

    internal static void Run()
    {
        var harmony = new Harmony("sc.tests.merchant-lifecycle");
        try
        {
            harmony.Patch(AccessTools.PropertyGetter(typeof(Node), "SceneFilePath"), prefix: Patch(nameof(GetPath)));
            harmony.Patch(AccessTools.PropertySetter(typeof(Node), "SceneFilePath"), prefix: Patch(nameof(SetPath)));
            var node = (Node)RuntimeHelpers.GetUninitializedObject(typeof(Node));
            GC.SuppressFinalize(node);
            foreach (var canonical in new[] { "res://scenes/merchant/characters/fool_merchant.tscn",
                         "res://scenes/rest_site/characters/fool_rest_site.tscn" })
            {
                for (var i = 0; i < 100; i++)
                {
                    _scenePath = $"res://sts2_skin_runtime/test/{i}/" + canonical[6..];
                    NonCombatSceneIdentity.Register(_scenePath, canonical);
                    AccessTools.Method(typeof(CznPresentationBridge), "RestoreNonCombatIdentity").Invoke(null, [node]);
                    Require(_scenePath == canonical, "Author's original scene identity must be visible before Ready.");
                }
            }
            var unknown = "res://unrelated/scenes/fool_merchant.tscn";
            Require(NonCombatSceneIdentity.Resolve(unknown) == unknown, "Do not guess provenance from a filename.");

            var refresh = Calls(AccessTools.Method(typeof(MerchantRuntimeAppearance), "TryRefreshMerchant"));
            Require(Array.FindIndex(refresh, m => m.Name == "EnsureSelectedMerchantProviderActive") <
                    Array.FindIndex(refresh, m => m.Name == "InstantiateMerchantButton"),
                "Selected merchant must initialize before building its first visual.");
            var activation = Calls(AccessTools.Method(typeof(MerchantRuntimeAppearance), "EnsureSelectedMerchantProviderActive"));
            Require(activation.Any(m => m.Name == "TryActivateDeferredProvider"), "Re-focusing alone must not stand in for completed initialization.");
            var owner = AccessTools.Method(typeof(ManagedSkinModLoader), "IsMerchantSkinProvider");
            Require((bool)owner.Invoke(null, ["ATA_Merchant", "ATA_Merchant"])!, "Keep the selected merchant active.");
            foreach (var old in new[] { "Merchant2CuteII", "Merchant2CuteIIB", "OtherMerchant" })
                Require(!(bool)owner.Invoke(null, [old, "ATA_Merchant"])!, "Outgoing merchant must deactivate: " + old);
            Require(!(bool)owner.Invoke(null, ["ATA_Merchant", null])!, "Original selection must not keep an old merchant active.");
            Console.WriteLine("Merchant lifecycle passed: 200 authored shop/rest scene identities, initialization-before-build and outgoing-provider isolation.");
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }

    private static MethodBase[] Calls(MethodBase method) => PatchProcessor.GetOriginalInstructions(method)
        .Select(i => i.operand).OfType<MethodBase>().ToArray();
    private static HarmonyMethod Patch(string name) => new(typeof(MerchantLifecycleTests), name);
    private static bool GetPath(ref string __result) { __result = _scenePath; return false; }
    private static bool SetPath(string value) { _scenePath = value; return false; }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
