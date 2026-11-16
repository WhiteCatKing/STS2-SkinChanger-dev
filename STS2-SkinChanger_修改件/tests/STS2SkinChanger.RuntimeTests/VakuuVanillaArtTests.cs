using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class VakuuVanillaArtTests
{
    private static string _language = "eng";
    internal static void Run(string gamePack)
    {
        var silence = AuditLogSilencer.Install();
        var harmony = new Harmony("sc.tests.vakuu-vanilla");
        var oldCatalog = SkinService.Catalog; var oldConfig = SkinService.Config;
        try
        {
            using var catalog = SkinCatalog.Build(gamePack, []);
            var group = catalog.Groups.Single(g => g.Id == "vakuu");
            Require(group.Options.Count(o => o.Id == VakuuVanillaArt.EasterEggId) == 1, "Missing or duplicate built-in egg option without skin mods.");
            Require(catalog.Groups.Where(g => g.Id != "vakuu").All(g => g.Options.All(o => o.Id != VakuuVanillaArt.EasterEggId)),
                "Egg option leaked to another Ancient.");
            var paths = new[] { VakuuVanillaArt.NormalPath, VakuuVanillaArt.EasterEggPath };
            var overlay = catalog.BuildRuntimeResourceOverlay("vakuu", SkinCatalog.BaseOptionId, paths, "vakuu-test/001");
            Require(paths.All(overlay.ResourcePaths.ContainsKey), "Actual game does not expose both official images.");
            Require(VakuuVanillaArt.PathFor("vakuu", SkinCatalog.BaseOptionId) == paths[0] &&
                    VakuuVanillaArt.PathFor("vakuu", VakuuVanillaArt.EasterEggId) == paths[1] &&
                    VakuuVanillaArt.PathFor("vakuu", "external-image") == null &&
                    VakuuVanillaArt.PathFor("neow", VakuuVanillaArt.EasterEggId) == null, "Vanilla art routing crossed selection/group boundaries.");
            var selection = catalog.BuildVisualSelectionTransaction("vakuu", VakuuVanillaArt.EasterEggId, new Dictionary<string, string>());
            Require(selection["vakuu"] == VakuuVanillaArt.EasterEggId, "Built-in selection did not enter normal persistence transaction.");
            var json = System.Text.Json.JsonSerializer.Serialize(new SkinConfig { Selections = new(selection, StringComparer.OrdinalIgnoreCase) });
            var config = (SkinConfig)AccessTools.Method(typeof(SkinConfig), "Deserialize").Invoke(null, [json])!;
            Require(config.GetSelection("vakuu") == VakuuVanillaArt.EasterEggId, "Built-in selection lost on reload.");
            config.AncientSkinPriorities.RegionGroups[SkinService.AncientRegionKey] = ["vakuu"];
            AccessTools.PropertySetter(typeof(SkinService), "Catalog").Invoke(null, [catalog]);
            AccessTools.PropertySetter(typeof(SkinService), "Config").Invoke(null, [config]);
            Require(SkinService.GetAncientPriorityOptions().All(o => o.OptionId != VakuuVanillaArt.EasterEggId),
                "Built-in egg must not become an automatically enabled category skin.");
            harmony.Patch(AccessTools.PropertyGetter(typeof(ModLocalization), "CurrentLanguage"), prefix: new(typeof(VakuuVanillaArtTests), nameof(Language)));
            foreach (var language in new[] { "eng", "zhs", "zht", "jpn", "kor", "deu", "fra", "ita", "esp", "spa", "pol", "ptb", "rus", "tha", "tur" })
            {
                _language = language;
                var normal = ModLocalization.Get(ModText.GameDefault);
                var egg = ModLocalization.DisplayOptionName(VakuuVanillaArt.EasterEggId);
                Require(egg.StartsWith(normal) && egg.Length > normal.Length, "Default prefix/localization mismatch: " + language);
                if (language == "zhs") Require(egg == normal + "彩蛋", "Chinese option label mismatch.");
            }
            Require(SkinOptionStylePolicy.IsAccented(SkinCatalog.BaseOptionId) && SkinOptionStylePolicy.IsAccented(VakuuVanillaArt.EasterEggId),
                "Both official choices must share the existing gold accent policy.");
            Console.WriteLine("Vakuu vanilla art passed: real normal/egg resources, exclusive option, saved selection, category exclusion and 15 localized default labels.");
        }
        finally
        {
            AccessTools.PropertySetter(typeof(SkinService), "Catalog").Invoke(null, [oldCatalog]);
            AccessTools.PropertySetter(typeof(SkinService), "Config").Invoke(null, [oldConfig]);
            harmony.UnpatchAll(harmony.Id); silence.UnpatchAll(silence.Id);
        }
    }
    private static bool Language(ref string __result) { __result = _language; return false; }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
