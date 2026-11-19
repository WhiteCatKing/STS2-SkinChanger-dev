using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CharacterDialogueLocalizationTests
{
    internal static void Run()
    {
        VerifySelection();
        VerifyDialogueProjection();
        Console.WriteLine("Character dialogue localization passed: selection, global UI, and reversible dialogue projection.");
    }

    private static void VerifySelection()
    {
        using var game = new TestPack("game");
        using var authored = new TestPack("FoolSkin");
        using var alternate = new TestPack("OtherSkin");
        var scoped = authored.Localization("eng", "ancients", """{"NEOW.talk.FOOL.0-0.char":"Skin line"}""");
        // A different language can ship general gameplay text in its own ancients table.
        var mixed = authored.Localization("zhs", "ancients", """
            {"NEOW.talk.FOOL.0-0.char":"Skin line","NEOW.talk.ANY.0-0.ancient":"General line"}
            """);
        var settings = authored.Localization("eng", "settings", """{"example.label":"Mod settings"}""");
        var bowUi = authored.Localization("eng", "bow_ui", """{"bow.label":"Bow settings"}""");
        var characters = authored.Localization("eng", "characters", """{"FOOL.title":"Skin title"}""");
        game.Write();
        authored.Write();
        alternate.Write();
        using var catalog = SkinCatalog.Build(game.Path, [authored.Descriptor, alternate.Descriptor]);

        Require(catalog.IsManagedCosmeticLocalizationPath(scoped), "Owned character dialogue should be scoped.");
        Require(!catalog.IsManagedCosmeticLocalizationPath(mixed), "A mixed gameplay table should keep its global lifetime.");
        var all = new[] { scoped, mixed, settings, bowUi, characters };
        var always = new[] { mixed, settings, bowUi };
        var selections = new Dictionary<string, string>();
        Check(always);
        selections["fool"] = "FoolSkin";
        Check(all);
        selections["fool"] = "OtherSkin";
        Check(always);
        selections["fool"] = "FoolSkin";
        Check(all);

        void Check(string[] expected)
        {
            var tables = catalog.FilterModdedLocalizationTables(all, selections);
            Require(tables.ToHashSet().SetEquals(expected), "Only selected character dialogue and global tables should merge.");
            var overlay = catalog.BuildOverlay(selections);
            Require(overlay.Keys.Where(all.Contains).ToHashSet().SetEquals(expected),
                "The overlay should carry the same selected and global localization tables.");
        }
    }

    private static void VerifyDialogueProjection()
    {
        var table = new Dictionary<string, string>
        {
            ["NEOW.talk.FOOL.0-0r.char"] = "New speaker",
            ["NEOW.talk.FOOL.0-0r.next"] = "Continue",
            ["NEOW.talk.FOOL.0-1r.ancient"] = "New response"
        };
        var loc = (LocManager)RuntimeHelpers.GetUninitializedObject(typeof(LocManager));
        AccessTools.Field(typeof(LocManager), "_tables").SetValue(loc,
            new Dictionary<string, LocTable> { ["ancients"] = new LocTable("ancients", table) });
        var setLoc = AccessTools.PropertySetter(typeof(LocManager), nameof(LocManager.Instance));
        var previous = LocManager.Instance;
        setLoc.Invoke(null, [loc]);
        var template = new AncientDialogue("", "") { VisitIndex = 1 };
        var fallback = new AncientDialogue("") { VisitIndex = 1 };
        var original = new AncientDialogueSet
        {
            FirstVisitEverDialogue = fallback,
            AgnosticDialogues = [fallback],
            CharacterDialogues = new() { ["REGENT"] = [template] }
        };
        var firstVisit = new ModelId("CHARACTER", "FOOL");
        try
        {
            var result = HildeAncientDialogueCompat.Project(original, "NEOW", AddAuthorDialogue);
            var dialogue = result.GetValidDialogues(firstVisit, 1, 1, true).Single();
            Require(!ReferenceEquals(result, original) && !ReferenceEquals(dialogue, fallback) &&
                    dialogue.IsRepeating && dialogue.VisitIndex == 1 &&
                    dialogue.Lines[0].LineText?.LocEntryKey == "NEOW.talk.FOOL.0-0r.char" &&
                    dialogue.Lines[0].NextButtonText?.LocEntryKey == "NEOW.talk.FOOL.0-0r.next",
                "The selected projection should use game loc keys and preserve dialogue visit metadata.");
            Require(!original.CharacterDialogues.ContainsKey("FOOL") &&
                    ReferenceEquals(original.GetValidDialogues(firstVisit, 1, 1, true).Single(), fallback) &&
                    ReferenceEquals(result.GetValidDialogues(firstVisit, 1, 0, true).Single(), fallback),
                "The canonical dialogue must still use its own fallback, including first visits.");
            Require(ReferenceEquals(result.CharacterDialogues["REGENT"][0], template) &&
                    template.Lines[0].LineText == null,
                "Projection must not repopulate the shared REGENT template.");
            Require(ReferenceEquals(HildeAncientDialogueCompat.Project(result, "NEOW", AddAuthorDialogue), result),
                "Do not replace an existing FOOL dialogue, including game or other mod dialogue.");
            table.Clear();
            Require(ReferenceEquals(HildeAncientDialogueCompat.Project(original, "NEOW", AddAuthorDialogue), original),
                "A missing localization table must not publish a broken injected dialogue.");
        }
        finally
        {
            setLoc.Invoke(null, [previous]);
        }

        static void AddAuthorDialogue(AncientDialogueSet set, string entry)
        {
            if (entry == "NEOW")
            {
                set.CharacterDialogues.Add("FOOL", [new AncientDialogue("", "") { VisitIndex = 1 }]);
            }
        }
    }

    private sealed class TestPack : IDisposable
    {
        private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("sc-ancient-loc-");
        private readonly Dictionary<string, byte[]> _files = new()
        {
            ["res://scenes/creature_visuals/fool.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Fool\" type=\"Node2D\"]\n"),
            ["res://animations/characters/fool/model.tres"] = Encoding.UTF8.GetBytes("[gd_resource type=\"Resource\" format=3]\n[resource]\n")
        };

        internal TestPack(string id)
        {
            Path = System.IO.Path.Combine(_directory.FullName, id + ".pck");
            Descriptor = new SkinModDescriptor(id, id, Path, false, _directory.FullName);
        }

        internal string Path { get; }
        internal SkinModDescriptor Descriptor { get; }

        internal string Localization(string language, string table, string json)
        {
            var path = $"res://{Descriptor.Id}/localization/{language}/{table}.json";
            _files.Add(path, Encoding.UTF8.GetBytes(json));
            return path;
        }

        internal void Write() => PckArchive.Write(Path, _files);
        public void Dispose() => _directory.Delete(recursive: true);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
