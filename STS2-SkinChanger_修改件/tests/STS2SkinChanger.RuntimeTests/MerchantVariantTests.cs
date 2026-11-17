using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

internal static class MerchantVariantTests
{
    private static Node _root = null!;
    private static MerchantInventory _inventory = null!;
    private static int _skins, _setups, _completed;
    internal static void Run(string game, string workshop, string oldRoot)
    {
        var silence = AuditLogSilencer.Install();
        var root = Path.Combine(workshop, "3748331824");
        var updated = Assembly.LoadFrom(Path.Combine(root, "Merchant2CuteII.dll"));
        var old = Assembly.LoadFrom(Path.Combine(oldRoot, "Merchant2CuteIIB.dll"));
        Require(MerchantVariantScenes.HasBlitzContract(updated), "Updated merchant's explicit Blitz contract missing.");
        Require(!MerchantVariantScenes.HasBlitzContract(old), "Old merchant must retain a native Blitz fallback.");
        using (var pack = PckArchive.Open(Path.Combine(root, "Merchant2CuteII.pck")))
        {
            var jsonPath = "res://animations/customs/merchant2cute_ii/blitz_merchant/top/blitz_merchant_top.spine-json";
            var json = Encoding.UTF8.GetString(pack.ReadFile(jsonPath));
            foreach (var token in new[] { "animation", "done", "done_loop", "default", "outline", "TOKEN_1", "TOKEN_6" })
                Require(json.Contains(token, StringComparison.Ordinal), "Updated Blitz skeleton lacks native animation/slot: " + token);
        }
        var descriptors = new[] {
            new SkinModDescriptor("Merchant2CuteII", "Updated", Path.Combine(root, "Merchant2CuteII.pck"), false, root, true),
            new SkinModDescriptor("Merchant2CuteIIB", "Old", Path.Combine(oldRoot, "Merchant2CuteIIB.pck"), false, oldRoot, true)
        };
        using (var catalog = SkinCatalog.Build(game, descriptors))
        {
            Require(catalog.Groups.Single(g => g.Id == "merchant").Options.Single(o => o.Id == "Merchant2CuteII").Assets.Keys
                .Any(path => path.Contains("customs/merchant2cute_ii/blitz_merchant/top/blitz_merchant_top_skel_data.tres")),
                "Private Blitz skeleton must be owned by the selected merchant even when fake merchant chooses another package.");
            foreach (var id in new[] { "merchant", "fake_merchant_monster" })
                foreach (var descriptor in descriptors)
                    Require(catalog.Groups.Single(g => g.Id == id).Options.Any(o => o.Id == descriptor.Id), "Merchant package missing from family: " + descriptor.Id + "/" + id);
        }
        var fixture = new Dictionary<string, SkinGroup> { ["merchant"] = new("merchant", "Merchant") };
        fixture["merchant"].Options.Add(new("real-only", "Real only", new Dictionary<string, ResourceAsset>(), IsRuntimeProvider: true)
            { UsesPrivateMerchantRoomCallback = true });
        SkinCatalog.AddMerchantVariantFallbackOptions(fixture);
        SkinCatalog.AddMerchantVariantFallbackOptions(fixture);
        var fallback = fixture["fake_merchant_monster"].Options.Single();
        Require(fallback.IsMerchantVariantFallback && !fallback.IsRuntimeProvider && !fallback.UsesPrivateMerchantRoomCallback && fallback.Assets.Count == 0,
            "Missing-family options must not replay the wrong provider or invent scene resources.");
        var screenType = typeof(AncientCompendiumEntry).Assembly.GetType("STS2SkinChanger.Ui.AncientCompendiumScreen", true)!;
        var category = screenType.GetNestedType("OtherCategory", BindingFlags.NonPublic)!;
        var entries = (Array)AccessTools.Method(screenType, "GetOtherEntries").Invoke(null, [Enum.Parse(category, "Merchants")])!;
        Require(entries.Length == (MerchantVariantScenes.HasBlitz ? 3 : 2), "Native branch must determine preview entries.");
        if (MerchantVariantScenes.HasBlitz)
        {
            var entry = entries.Cast<object>().Single(e => (string)e.GetType().GetProperty("Id")!.GetValue(e)! == MerchantVariantScenes.BlitzEntryId);
            Require((string)entry.GetType().GetProperty("GroupLookupId")!.GetValue(entry)! == "merchant", "Blitz preview must share package choices with ordinary merchant.");
        }
        _root = Fake<Control>();
        var harmony = new Harmony("sc.tests.merchant-variants");
        try
        {
            harmony.Patch(MerchantVariantScenes.Skinner(updated), prefix: new HarmonyMethod(typeof(MerchantVariantTests), nameof(Skin)));
            for (var i = 0; i < 200; i++) MerchantVariantScenes.ApplySkinner(updated, _root);
            Require(_skins == 200, "Author's pre-tree skinning did not dispatch exactly once per tree.");
            var buttonType = typeof(NMerchantRoom).Assembly.GetType("MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantButton");
            if (buttonType != null)
            {
                var button = (NMerchantButton)RuntimeHelpers.GetUninitializedObject(buttonType); GC.SuppressFinalize(button);
                _inventory = (MerchantInventory)RuntimeHelpers.GetUninitializedObject(typeof(MerchantInventory));
                AccessTools.Property(typeof(MerchantInventory), "BlitzChoicesRemaining").SetValue(_inventory, 2);
                harmony.Patch(AccessTools.Method(buttonType, "Setup"), prefix: new HarmonyMethod(typeof(MerchantVariantTests), nameof(Setup)));
                harmony.Patch(AccessTools.Method(buttonType, "CompleteShop"), prefix: new HarmonyMethod(typeof(MerchantVariantTests), nameof(Complete)));
                var roomType = typeof(NMerchantRoom).Assembly.GetType("MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom", true)!;
                var room = RuntimeHelpers.GetUninitializedObject(roomType); GC.SuppressFinalize(room);
                var field = MerchantRuntimeAppearance.ResolveMerchantButtonField(room)!;
                var ordinary = Fake<NMerchantButton>();
                var rejected = false;
                try
                {
                    AccessTools.Method(typeof(MerchantRuntimeAppearance), "ReplaceMerchantButtonCore").Invoke(null,
                        [ordinary, ordinary, room, field, new Action<NMerchantButton>(_ => { })]);
                }
                catch (TargetInvocationException error) when (error.InnerException is InvalidOperationException) { rejected = true; }
                Require(rejected && field.GetValue(room) == null, "Wrong button type must fail before native tree mutation.");
                Require(PatchProcessor.GetOriginalInstructions(AccessTools.Method(buttonType, "_ExitTree"))
                    .Any(i => i.operand is MethodBase m && m.Name == "Teardown"), "Native exit must retain purchase-event teardown.");
                for (var i = 0; i < 200; i++) MerchantVariantScenes.BindBlitzInventory(button, _inventory);
                Require(_setups == 200 && (int)AccessTools.Property(typeof(MerchantInventory), "BlitzChoicesRemaining").GetValue(_inventory)! == 2,
                    "Hot swaps must bind the same inventory and preserve its remaining choices.");
                AccessTools.Property(typeof(MerchantInventory), "BlitzChoicesRemaining").SetValue(_inventory, 0);
                MerchantVariantScenes.BindBlitzInventory(button, _inventory);
                Require(_completed == 1, "Exhausted inventory must retain the native completed-shop presentation.");
            }
        }
        finally { harmony.UnpatchAll(harmony.Id); }
        silence.UnpatchAll(silence.Id);
        Console.WriteLine("Merchant variants passed: actual updated/old DLL and skeleton slots, both package families, native-fallback isolation, branch-specific third preview, 200 pre-tree skinning and inventory bindings.");
    }
    private static bool Skin(Node __0, ref int __result) { Require(ReferenceEquals(__0, _root), "Wrong scene root."); _skins++; __result = 1; return false; }
    private static bool Complete() { _completed++; return false; }
    private static bool Setup(MerchantInventory __0) { Require(ReferenceEquals(__0, _inventory), "Purchase model replaced."); _setups++; return false; }
    private static T Fake<T>() where T : class { var item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T)); GC.SuppressFinalize(item); return item; }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
