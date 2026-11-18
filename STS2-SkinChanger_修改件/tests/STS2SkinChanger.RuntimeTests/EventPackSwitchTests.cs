using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class EventPackSwitchTests
{
    private const string Group = "event:this_or_that", PathName = "res://images/events/this_or_that.png";
    private static readonly Dictionary<string, Texture2D> Textures = new();
    private static int _loads;
    internal static void Run(string game, string workshop)
    {
        _ = AuditLogSilencer.Install();
        var specs = new[] { ("3765802910", "NSFW-Event"), ("3841546779", "RegentEventArt") };
        var descriptors = specs.Select(s => new SkinModDescriptor(s.Item2, s.Item2,
            System.IO.Path.Combine(workshop, s.Item1, s.Item2 + ".pck"), false,
            System.IO.Path.Combine(workshop, s.Item1), false)).ToArray();
        using var catalog = SkinCatalog.Build(game, descriptors);
        Require(catalog.Groups.Single(g => g.Id == Group).Options.Select(o => o.Id).ToHashSet()
            .SetEquals(specs.Select(s => s.Item2)), "Actual event packs must remain separate options.");
        var choices = new[] { "__base__", "NSFW-Event", "RegentEventArt" };
        var payloads = new Dictionary<string, byte[]>();
        var baselines = new Dictionary<string, string>();
        foreach (var choice in choices)
        {
            var file = choice == "__base__" ? game : descriptors.Single(d => d.Id == choice).PckPath;
            using var archive = STS2SkinChanger.Pck.PckArchive.Open(file!);
            var import = Encoding.UTF8.GetString(archive.ReadFile(PathName + ".import"));
            var target = Regex.Match(import, "path(?:\\.[a-z0-9_]+)?=\"([^\"]+)\"").Groups[1].Value;
            payloads[choice] = archive.ReadFile(target);
            Textures[choice] = Fake<CompressedTexture2D>();
        }
        Require(payloads.Values.Select(p => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(p))).Distinct().Count() == 3,
            "Test must use three distinct actual CTEX payloads.");
        for (var cycle = 0; cycle < 4; cycle++)
        foreach (var choice in choices)
        {
            var overlay = catalog.BuildRuntimeResourceOverlay(Group, choice, [PathName], "event-regression/" + cycle + choice);
            Require(overlay.Files.Where(p => p.Key.EndsWith(".ctex")).All(p => p.Value.SequenceEqual(payloads[choice])) &&
                overlay.Files.Keys.Any(p => p.EndsWith(".ctex")), "Actual selected payload leaked from another pack: " + choice);
            var selections = new Dictionary<string, string> { [Group] = choice };
            Require(catalog.TryResolveEventText("THIS_OR_THAT.pages.INITIAL.description", "zhs", selections, out var text),
                "Actual event text must resolve.");
            if (cycle == 0) baselines[choice] = text;
            else Require(baselines[choice] == text, "Switching contaminated original text.");
        }
        // Each package owns its dialogue; vanilla must stay independent.
        Require(baselines.Values.Distinct().Count() == 3,
            "A choice inherited another source's dialogue.");
        var oldCatalog = SkinService.Catalog; var oldConfig = SkinService.Config;
        var config = new SkinConfig();
        var harmony = new Harmony("sc.tests.event-pack-switch");
        try
        {
            AccessTools.Property(typeof(SkinService), "Catalog").SetValue(null, catalog);
            AccessTools.Property(typeof(SkinService), "Config").SetValue(null, config);
            harmony.Patch(AccessTools.Method(typeof(EventSkinRuntime), "LoadTexture"),
                prefix: new HarmonyMethod(typeof(EventPackSwitchTests), nameof(Load)));
            harmony.CreateClassProcessor(typeof(EventSkinInitialPortraitPatch)).Patch();
            var modelType = typeof(EventModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Events.ThisOrThat", true)!;
            var model = (EventModel)RuntimeHelpers.GetUninitializedObject(modelType);
            AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField").SetValue(model, new ModelId("EVENT", "THIS_OR_THAT"));
            for (var cycle = 0; cycle < 100; cycle++)
            foreach (var choice in choices)
            {
                config.Selections[Group] = choice;
                Require(EventSkinRuntime.TryLoadInitialPortrait(model, false, out var direct) && ReferenceEquals(direct, Textures[choice]),
                    "Compendium initial portrait used stale AssetCache.");
                Require(ReferenceEquals(model.CreateInitialPortrait(), Textures[choice]), "Live native portrait factory bypassed selection.");
            }
            Require(_loads == 600, "Direct preview/native factory were not both exercised.");
            var nested = typeof(EventCompendiumPreview).GetNestedTypes(BindingFlags.NonPublic)
                .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(m => m.GetMethodBody() != null);
            Require(nested.Any(m => PatchProcessor.GetOriginalInstructions(m).Any(i => i.operand is MethodBase target &&
                target.DeclaringType == typeof(EventSkinRuntime) && target.Name == "TryLoadInitialPortrait")),
                "Actual preview callback must prefer selected portrait before native cache.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            AccessTools.Property(typeof(SkinService), "Catalog").SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), "Config").SetValue(null, oldConfig);
            Textures.Clear();
        }
        Console.WriteLine("Actual event pack switching passed: three distinct CTEX sources, vanilla dialogue isolation, repeated native/preview portrait reads without AssetCache and 600 scoped loads.");
    }
    private static bool Load(string __0, string __1, ref Texture2D __result)
    {
        Require(__0 == Group && __1 == PathName, "Wrong event resource/owner.");
        _loads++; __result = Textures[SkinService.Config.GetSelection(Group)]; return false;
    }
    private static T Fake<T>() where T : class
    { var obj = (T)RuntimeHelpers.GetUninitializedObject(typeof(T)); GC.SuppressFinalize(obj); return obj; }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
