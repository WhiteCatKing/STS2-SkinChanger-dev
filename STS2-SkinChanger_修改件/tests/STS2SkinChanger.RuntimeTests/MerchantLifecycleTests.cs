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
            var roomType = typeof(MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom).Assembly
                .GetType(MerchantRoomConcreteReadyPatch.StandardTypeName, true)!;
            for (var i = 0; i < 200; i++)
            {
                var room = (MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom)RuntimeHelpers.GetUninitializedObject(roomType);
                GC.SuppressFinalize(room);
                Require(MerchantRuntimeAppearance.TryClaimInitialMerchantRoom(room), "A new shop must initialize its selected appearance.");
                Require(!MerchantRuntimeAppearance.TryClaimInitialMerchantRoom(room), "Base/concrete Ready must not rebuild the same shop twice.");
            }
            foreach (var patchType in new[] { typeof(MerchantRoomCreateAppearancePatch), typeof(MerchantRoomConcreteReadyPatch) })
                Require(Calls(AccessTools.Method(patchType, "Postfix")).Any(m => m.Name == "QueueInitialMerchantAppearance"),
                    "Shop entry must refresh cached merchant visuals on both game layouts: " + patchType.Name);
            var initial = Calls(AccessTools.Method(typeof(MerchantRuntimeAppearance), "CompleteInitialMerchantAppearance"));
            Require(initial.Any(m => m.Name == "TryRefreshMerchant"), "Initial entry must use the selected isolated visual transaction.");
            Require(!initial.Any(m => m.Name is "Initialize" or "_Ready" or "TryCreateCurrentMerchantRoom"),
                "Initial appearance must preserve live room and inventory initialization.");
            Console.WriteLine("Merchant lifecycle passed: 200 authored shop/rest scene identities, initialization-before-build and outgoing-provider isolation.");
            Console.WriteLine("Initial shop entry passed: 200 distinct rooms, duplicate Ready rejection and retained inventory refresh boundary.");
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
