using System.Reflection;
using System.Text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class PrivateMerchantTests
{
    internal static void Run(string gamePack, string root)
    {
        var silence = AuditLogSilencer.Install();
        var scratch = Path.Combine(Path.GetTempPath(), "sc-private-merchant-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        try
        {
            var provider = new SkinModDescriptor("neowmerchant", "商人娘化涅奥版",
                Path.Combine(root, "neowmerchant.pck"), false, root, true);
            var probe = SkinCatalog.ProbeSkinProviders([provider], gamePack).Single();
            Require(probe.VisualGroupCount == 1 && probe.ServedGroupIds!.SequenceEqual(["merchant"]),
                "Actual loader probe did not discover the private merchant.");
            using var catalog = SkinCatalog.Build(gamePack, [provider]);
            var option = catalog.Groups.Single(group => group.Id == "merchant").Options.Single();
            Require(option.Id == provider.Id && option.UsesPrivateMerchantRoomCallback && option.IsRuntimeProvider,
                "Private merchant lacks its selected room lifecycle.");
            Require(catalog.GetFullRuntimeProviderGroups(provider.Id).SequenceEqual(["merchant"]),
                "Private merchant leaked into a character, ancient or fake-merchant group.");
            var scene = "res://scenes/rooms/standard_merchant_room.tscn";
            var overlay = catalog.BuildRuntimeResourceOverlay("merchant", provider.Id, [scene], "private-merchant/001", true);
            Require(overlay.ResourcePaths.ContainsKey(scene), "Native merchant scene must remain loadable.");
            // Full providers mount their private PCK once, not in every per-scene alias pack.
            var mounted = catalog.BuildOverlay(new Dictionary<string, string> { ["merchant"] = provider.Id });
            foreach (var path in new[] { "merchant/data.tres", "hand/data.tres", "merchant/texture.ctex",
                         "hand/texture.ctex", "ui/merchant_hit.gd", "ui/merchant_outline.gdshader" })
                Require(mounted.ContainsKey("res://npc_feminization2/" + path),
                    "Runtime overlay lost required author resource: " + path);
            Require(option.Assets.Keys.All(path => !path.StartsWith("res://animations/backgrounds/")),
                "Private overlay must not replace the vanilla skeleton targets used by the author's matcher.");
            var selections = catalog.BuildVisualSelectionTransaction("merchant", provider.Id, new Dictionary<string, string>());
            Require(selections.Count == 1 && selections["merchant"] == provider.Id, "Merchant selection rewrote unrelated groups.");
            var native = Assembly.LoadFrom(Path.Combine(root, "neowmerchant.dll"));
            var callback = native.GetType("NpcFeminization.MerchantRoomPatch", true)!;
            Require(callback.GetCustomAttributes<HarmonyPatch>().Any(attr => attr.info.declaringType == typeof(NMerchantRoom) && attr.info.methodName == "_Ready"),
                "Actual author callback no longer matches the supported room lifecycle.");
            Require((bool)AccessTools.Method(typeof(ManagedSkinModLoader), "IsNodeReadyPresentationTarget")
                .Invoke(null, [AccessTools.Method(typeof(NMerchantRoom), "_Ready")])!, "Room callback cannot be replayed by the existing runtime.");
            using var noDll = SkinCatalog.Build(gamePack, [provider with { HasDll = false }]);
            Require(!noDll.Groups.Any(group => group.Options.Any(item => item.Id == provider.Id)),
                "Private file names alone must not claim a merchant.");
            var fakePack = Path.Combine(scratch, "neowmerchant.pck");
            PckArchive.Write(fakePack, new Dictionary<string, byte[]>
            {
                ["res://npc_feminization2/merchant/data.tres"] = Encoding.UTF8.GetBytes("[gd_resource type=\"ShaderMaterial\" format=3]\n[resource]\n")
            });
            using var noSkeleton = SkinCatalog.Build(gamePack, [provider with { PckPath = fakePack }]);
            Require(!noSkeleton.Groups.Any(group => group.Options.Any(item => item.Id == provider.Id)),
                "A room patch and similarly named helper resource are not a merchant skin.");
            Console.WriteLine("Private merchant passed: real loader probe/DLL/PCK, exclusive merchant option, complete body/hand/UI overlay, selected callback replay and negative content guards.");
        }
        finally
        {
            silence.UnpatchAll(silence.Id);
            Directory.Delete(scratch, recursive: true);
        }
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
