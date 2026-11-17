using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

internal static class ExtraCreatureTests
{
    internal static void Run(string game)
    {
        var all = OtherCreatureCatalog.All;
        Require(all.Count == 5 && all.Select(c => c.Id).Distinct().Count() == 5, "Five distinct creature entries required.");
        Require(OtherCreatureCatalog.Available(_ => false).Select(c => c.Id)
            .SequenceEqual(new[] { "byrdpip", "paels_legion", "osty" }), "Older branch must keep Osty and omit beta-only creatures.");
        var available = OtherCreatureCatalog.Available(OtherCreaturePreview.HasModel).ToArray();
        Require(available.Length == 3 + new[] { "ByrdineMonster", "Wuppy" }
            .Count(n => typeof(MonsterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Monsters." + n) != null),
            "Actual branch capability was not used.");
        using var pack = PckArchive.Open(game);
        foreach (var creature in all)
        {
            var scene = Encoding.UTF8.GetString(pack.ReadFile(creature.ScenePath));
            Require(scene.Contains("SpineSprite") && scene.Contains("skeleton_data_res"), "Actual creature scene/visual node missing: " + creature.Id);
            if (creature.Id == "byrdine_monster")
                Require(scene.Contains("/byrdine/byrdine_skel_data.tres") && scene.Contains("hair/v4"), "Wrong evolved bird scene/skin.");
            if (creature.Id is "osty" or "byrdine_monster" or "wuppy")
            {
                var folder = creature.Id == "byrdine_monster" ? "byrdine" : creature.Id;
                var import = Encoding.UTF8.GetString(pack.ReadFile($"res://animations/monsters/{folder}/{folder}.skel.import"));
                var path = Regex.Match(import, "path=\"([^\"]+)\"").Groups[1].Value;
                Require(path.Length > 0 && pack.ReadFile(path).Length > 100, "Native skeleton payload missing: " + folder);
            }
        }
        var osty = OtherCreatureCatalog.Find("osty")!;
        Require(osty.Actions.Any(a => a.Kind == OtherCreatureActionKind.Attack) &&
            osty.Actions.Any(a => a.Kind == OtherCreatureActionKind.Revive), "Osty actions missing.");
        var wuppy = OtherCreatureCatalog.Find("wuppy")!;
        Require(wuppy.Actions.Select(a => a.AnimationAliases.First()).SequenceEqual(new[] { "hurt", "die" }),
            "Wuppy must expose its actual native actions.");
        Require(OtherCreatureFrameAnimation.Resolve(new[] { "IDLE", "attack" }, new[] { "idle", "attack" }) == "IDLE" &&
            OtherCreatureFrameAnimation.Resolve(new[] { "idle_loop" }, new[] { "die" }) == null,
            "Frame action availability must honor actual names and aliases.");
        Require(OtherCreatureActionText.Names.Count == 15 && OtherCreatureActionText.Names.Values
            .All(t => new[] { t.Cast, t.Hurt, t.Death, t.Revive }.All(s => !string.IsNullOrWhiteSpace(s))), "15 action translations required.");
        var screen = typeof(ContextualSkinControls).Assembly.GetType("STS2SkinChanger.Ui.AncientCompendiumScreen", true)!;
        Require(Calls(AccessTools.Method(screen, "GetOtherEntries"), typeof(OtherCreatureCatalog), "Available"), "Compendium omitted branch gate.");
        Require(Calls(AccessTools.Method(screen, "PopulateSkinDropdown"), typeof(OstySkinChoices), "Populate") &&
            Calls(AccessTools.Method(typeof(ContextualSkinControls), "PopulateOsty"), typeof(OstySkinChoices), "Populate"),
            "Both surfaces must use one list/current selection.");
        Require(Calls(AccessTools.Method(screen, "OnSkinSelected"), typeof(OstySkinChoices), "Apply") &&
            Calls(AccessTools.Method(typeof(ContextualSkinControls), "ApplyOstySelection"), typeof(OstySkinChoices), "Apply"),
            "Both surfaces must write the same companion selection.");
        Require(Calls(AccessTools.Method(typeof(OstySkinChoices), "Apply"), typeof(SkinService), "ApplyCompanionSelection"),
            "Companion selection must not use the owner-linked character transaction.");
        Require(Calls(AccessTools.Method(screen, "OnSubmenuOpened"), typeof(SkinService), "add_CompanionSelectionChanged") &&
            Calls(AccessTools.Method(screen, "OnSubmenuClosed"), typeof(SkinService), "remove_CompanionSelectionChanged") &&
            Calls(AccessTools.Method(screen, "_ExitTree"), typeof(SkinService), "remove_CompanionSelectionChanged"),
            "Sync event must have bounded lifetime.");
        Require(Calls(AccessTools.Method(typeof(OtherCreaturePreview), "CreateOsty"), typeof(OstyModelPreviewControls), "LoadSkeletonForCurrentSelection") &&
            Calls(AccessTools.Method(typeof(OtherCreaturePreview), "CreateOsty"), typeof(OstyModelPreviewControls), "TryLoadSceneForCurrentSelection") &&
            Calls(AccessTools.Method(typeof(OtherCreaturePreview), "CreateOsty"), typeof(OstyImpostorBridge), "CreateSelectDisplay"),
            "Osty must reuse existing three-path rendering contracts.");
        Require(Calls(AccessTools.Method(typeof(OtherCreatureFrameAnimation), "Play"), typeof(Resource), "Duplicate"),
            "Frame actions must not edit shared loop settings.");
        Console.WriteLine($"Extra creature catalogue passed: {available.Length} actual-branch entries, five native PCK scenes, shared Osty choices/renderers, actions and event cleanup.");
    }
    private static bool Calls(MethodBase method, Type type, string name) => PatchProcessor.GetOriginalInstructions(method)
        .Any(i => i.operand is MethodBase m && m.DeclaringType == type && m.Name == name);
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
