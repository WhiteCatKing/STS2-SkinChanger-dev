using System.Text;
using System.Text.Json;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class SpineResourcePreflightTests
{
    private const string Atlas = "res://animations/character_select/ironclad/characterselect_ironclad.atlas";
    private const string Page = "res://animations/character_select/ironclad/characterselect_ironclad.png";
    private const string PackedAtlas = "res://.godot/imported/characterselect_ironclad.atlas-b06f18b64456c1a191a18ae8efee7013.spatlas";
    private const string PackedPage = "res://.godot/imported/characterselect_ironclad.png-1b865d1e0a90d3a7fe38379cdb48c8d3";

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-spine-pages-");
        var writer = PckArchive.WriteOutput;
        var logging = AuditLogSilencer.Install();
        var harmony = new Harmony("sc.tests.spine-preflight");
        try
        {
            PckArchive.WriteOutput = null;
            harmony.Patch(AccessTools.Method(typeof(SkinService), "MountOverlay"),
                prefix: new HarmonyMethod(typeof(SpineResourcePreflightTests), nameof(SkipMount)));

            const string pageText = "characterselect_ironclad.png\nsize: 1,1\nfilter: Linear, Linear\nregion\n  bounds: 0,0,1,1\n";
            byte[] AtlasPayload(string text) => JsonSerializer.SerializeToUtf8Bytes(new { source_path = Atlas, atlas_data = text });
            byte[] Import(string key, string target) => Encoding.UTF8.GetBytes($"[remap]\n{key}=\"{target}\"\n");
            string Pack(string name, Dictionary<string, byte[]> files)
            {
                var pck = Path.Combine(root.FullName, name + ".pck");
                PckArchive.Write(pck, files);
                return pck;
            }

            var baseline = Pack("game", new()
            {
                [Atlas + ".import"] = Import("path", PackedAtlas),
                [Page + ".import"] = Import("path.bptc", PackedPage + ".bptc.ctex"),
                [PackedAtlas] = AtlasPayload(pageText),
                [PackedPage + ".bptc.ctex"] = Texture(1)
            });

            var valid = Pack("valid", new()
            {
                [PackedAtlas] = AtlasPayload(pageText),
                [PackedPage + ".ctex"] = Texture(2),
                [PackedPage + ".s3tc.ctex"] = Texture(3)
            });
            var broken = Pack("broken", new()
            {
                [PackedAtlas] = AtlasPayload(pageText + "\nmissing.png\nsize: 1,1\n"),
                [PackedPage + ".ctex"] = Texture(2)
            });
            CheckMapping(baseline, valid, "valid");
            CheckFallback(baseline, broken, "broken");
            Console.WriteLine("Spine preflight passed: imported-only payload mapping, missing second page and original selection fallback.");
        }
        finally
        {
            PckArchive.WriteOutput = writer;
            harmony.UnpatchAll(harmony.Id);
            logging.UnpatchAll(logging.Id);
            root.Delete(recursive: true);
        }
    }

    private static void CheckMapping(string baseline, string pack, string id)
    {
        using var catalog = CreateCatalog(baseline, pack, id);
        var overlay = catalog.BuildRuntimeResourceOverlay("ironclad", id, [Atlas], "spine-test");
        var alias = overlay.ResourcePaths[Page];
        Require(overlay.SpinePages.Contains(alias), "A mapped page must be validated as part of the atlas.");
        var payloadAlias = overlay.PayloadAliases[PackedPage + ".ctex"];
        Require(Encoding.UTF8.GetString(overlay.Files[alias + ".import"]).Contains(payloadAlias) &&
                overlay.Files[payloadAlias].SequenceEqual(Texture(2)),
            "The baseline .import must use the provider's format-neutral page from the same source hash.");
    }

    private static void CheckFallback(string baseline, string pack, string id)
    {
        using var catalog = CreateCatalog(baseline, pack, id);
        var attempted = new List<string>();
        var overlay = SkinService.WithSpinePageFallback(catalog, "ironclad", id, selection =>
        {
            attempted.Add(selection);
            return catalog.BuildRuntimeResourceOverlay("ironclad", selection, [Atlas], "spine-test");
        });
        Require(attempted.SequenceEqual(new[] { id, SkinCatalog.BaseOptionId }) &&
                catalog.ResolveSpineSelection("ironclad", id) == SkinCatalog.BaseOptionId &&
                overlay.Files[overlay.PayloadAliases[PackedPage + ".bptc.ctex"]].SequenceEqual(Texture(1)),
            "The missing second page must reject the selected overlay and retry with the game's original bytes.");
    }

    private static SkinCatalog CreateCatalog(string baseline, string pack, string id) =>
        SkinCatalog.Build(baseline, [new SkinModDescriptor(id, id, pack, false)]);

    private static byte[] Texture(byte marker)
    {
        var texture = new byte[53];
        "GST2"u8.CopyTo(texture);
        texture[40] = 1;
        texture[42] = 1;
        texture[52] = marker;
        return texture;
    }

    private static bool SkipMount() => false;

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
