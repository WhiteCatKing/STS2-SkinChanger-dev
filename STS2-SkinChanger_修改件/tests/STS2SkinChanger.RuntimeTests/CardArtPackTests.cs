using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

/// <summary>
/// Verifies that a pure asset card-art pack is read as data, including against a real installed
/// bundle when one is present. A pack carries no code, so the whole risk is in the format rules.
/// </summary>
internal static class CardArtPackTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];

    public static void Run()
    {
        CheckFormatRules();
        CheckWorkshopRecognition();
        CheckDisabledAndUnloadedModsSkippedAndFallbackToDefault();
        CheckAncientAndSameNameOwnership();
        CheckLooseProviderFolder();
        CheckRealPacks();
        Console.WriteLine(
            "Card art pack contracts passed: bundle parsing, master-over-crop preference, " +
            "malformed-entry rejection, canvas routing, Workshop recognition, disabled/unloaded mod filtering " +
            "& default fallback, loose provider folders and real pack dimensions.");
    }

    private static void CheckAncientAndSameNameOwnership()
    {
        var root = Directory.CreateTempSubdirectory("sts2-card-art-ownership-").FullName;
        try
        {
            const string foolArt = "res://fool_card_beauty/art/fool_ancient_fixture.png";
            const string vanillaArt = "res://vanilla_card_beauty/art/same_name.png";
            const string modArt = "res://custom/cards/card_portraits/custom/same_name.png";
            CardCatalogEntry[] cards =
            [
                new("FoolAncientFixture", "res://images/atlases/card_atlas.sprites/fool/fool_ancient_fixture.tres",
                    "fool", "fool", "ancients") { IsCharacterPool = true },
                new("SameName", "res://images/atlases/card_atlas.sprites/ironclad/same_name.tres",
                    "ironclad", "ironclad", "ironclad") { IsGameCard = true },
                new("SameName", "res://custom/cards/card_portraits/custom/same_name.png",
                    "custom", "custom", "custom")
            ];

            var game = Path.Combine(root, "game.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]>());
            using var catalog = SkinCatalog.Build(game, []);
            var image = MakePng(608, 416, false);
            catalog.AttachCardArtPacks(
                [
                    new("fool_card_beauty", "Fool", "", root, [new(foolArt, "fool_ancient_fixture", image, "", "", false)]),
                    new("vanilla_card_beauty", "Vanilla", "", root, [new(vanillaArt, "same_name", image, "", "", false)]),
                    new("custom_card_beauty", "Custom", "", root, [new(modArt, "same_name", image, "", "", false)])
                ], cards);
            catalog.FinalizeCardGroups(cards);

            var groups = catalog.CardGroups.ToDictionary(group => group.Id);
            foreach (var groupId in new[] { "fool", "ancients" })
            {
                var option = groups[groupId].Options.Single(option => option.Id == "fool_card_beauty");
                Require(option.NormalPortraits.GetValueOrDefault("FoolAncientFixture") == foolArt &&
                        option.Assets.ContainsKey(foolArt),
                    "The Fool's Ancient card must keep its bundle option and exact private asset in both groups.");
            }

            Require(groups["ironclad"].Options.Any(option => option.Id == "vanilla_card_beauty") &&
                    groups["custom"].Options.All(option => option.Id != "vanilla_card_beauty"),
                "An unscoped replacement for a name shared with a mod card must remain available to the vanilla card.");
            Require(groups["custom"].Options.Any(option => option.Id == "custom_card_beauty") &&
                    groups["ironclad"].Options.All(option => option.Id != "custom_card_beauty"),
                "An explicit mod-pool resource path must keep the replacement scoped to that pool.");
        }
        finally { Directory.Delete(root, recursive: true); }
    }


    /// <summary>
    /// A provider that keeps its card art as ordinary image files must become selectable card art,
    /// while a folder that holds something else must not. Everything here runs on synthetic files:
    /// no game, no provider code, no initializer.
    /// </summary>
    private static void CheckLooseProviderFolder()
    {
        var root = Directory.CreateTempSubdirectory("sts2-loose-card-art-");
        try
        {
            var provider = Path.Combine(root.FullName, "LooseCards");
            var portraits = Path.Combine(provider, "portraits");
            Directory.CreateDirectory(portraits);
            var cards = new List<CardCatalogEntry>();
            for (var index = 0; index < 24; index++)
            {
                var name = $"loose_card_{index:00}";
                cards.Add(new CardCatalogEntry(
                    $"LooseCard{index:00}",
                    $"res://images/packed/card_portraits/silent/{name}.png",
                    "silent",
                    "silent",
                    "silent"));
                // The picture's own file name is what identifies its card, in the same upper-case
                // form the game's card IDs use.
                File.WriteAllBytes(
                    Path.Combine(portraits, name.ToUpperInvariant() + ".png"),
                    MakePng(608, 416, withAlpha: false));
            }

            // A picture that is not a card and a name that two cards share: neither may be offered,
            // and neither may stop the folder from being read.
            File.WriteAllBytes(Path.Combine(portraits, "reference_frame.png"), MakePng(320, 180, false));
            File.WriteAllBytes(Path.Combine(portraits, "SHARED_NAME.png"), MakePng(608, 416, false));
            File.WriteAllBytes(Path.Combine(portraits, ".hidden_card.png"), MakePng(608, 416, false));
            var ambiguousCards = cards
                .Append(new CardCatalogEntry(
                    "SharedName", "res://images/packed/card_portraits/silent/shared_name.png",
                    "silent", "silent", "silent"))
                .Append(new CardCatalogEntry(
                    "SharedName", "res://images/packed/card_portraits/defect/shared_name.png",
                    "defect", "defect", "defect"))
                .ToArray();

            var packs = LooseCardPortraitPackScanner.Scan(
                [new LooseCardPortraitProvider("LooseCards", "Loose Cards", provider)],
                ambiguousCards);
            Require(packs.Count == 1,
                $"a provider folder of card portraits must be read as one card pack, got {packs.Count}.");
            Require(packs[0].Id == "LooseCards" && packs[0].Name == "Loose Cards",
                "a loose pack must keep the provider's own id and name.");
            Require(packs[0].Cards.Count == 24,
                $"only the pictures that name one card may be offered, got {packs[0].Cards.Count}.");
            Require(packs[0].Cards.All(card =>
                    card.SourcePath.StartsWith("res://LooseCards/portraits/", StringComparison.Ordinal)),
                "a loose pack's pictures must stay identified by the provider's own folder.");
            Require(packs[0].Cards.All(card => !card.SourcePath.Contains(
                    "SHARED_NAME", StringComparison.OrdinalIgnoreCase)),
                "a picture whose name two cards share must not be routed to an arbitrary card.");

            var game = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]>());
            using (var catalog = SkinCatalog.Build(game, []))
            {
                var (covered, used) = catalog.AttachCardArtPacks(packs, ambiguousCards);
                catalog.FinalizeCardGroups(ambiguousCards);
                Require(covered.Count == 24 && used.Count == 1,
                    $"a loose pack must register every card it covers, got {covered.Count}.");
                var option = catalog.CardGroups
                    .SelectMany(group => group.Options)
                    .Single(candidate => candidate.Id == "LooseCards");
                Require(option.NormalPortraits.Count == 24 && option.Assets.Count == 24,
                    "a loose pack's cards must reach the card rows with their own images mounted.");
                Require(option.ProviderRootPath == provider,
                    "a loose pack must carry its folder so the Workshop item it came from is known.");
            }

            // A folder whose pictures are not cards, or whose matches are too few to be card art,
            // stays the author's own material.
            var icons = Path.Combine(root.FullName, "IconProvider", "images");
            Directory.CreateDirectory(icons);
            for (var index = 0; index < 24; index++)
            {
                File.WriteAllBytes(
                    Path.Combine(icons, $"icon_{index:00}.png"),
                    MakePng(64, 64, withAlpha: true));
            }

            var fewMatches = Path.Combine(root.FullName, "FewCards", "portraits");
            Directory.CreateDirectory(fewMatches);
            for (var index = 0; index < 10; index++)
            {
                File.WriteAllBytes(
                    Path.Combine(fewMatches, $"LOOSE_CARD_{index:00}.png"),
                    MakePng(608, 416, withAlpha: false));
            }

            var packaged = Path.Combine(root.FullName, "PackagedProvider");
            Directory.CreateDirectory(packaged);
            File.WriteAllBytes(Path.Combine(packaged, "PackagedProvider.pck"), [1, 2, 3]);
            var packagedPortraits = Path.Combine(packaged, "portraits");
            Directory.CreateDirectory(packagedPortraits);
            foreach (var card in packs[0].Cards)
            {
                File.WriteAllBytes(
                    Path.Combine(packagedPortraits, Path.GetFileName(card.SourcePath)),
                    MakePng(608, 416, withAlpha: false));
            }

            var bundled = Path.Combine(root.FullName, "BundledProvider");
            var bundledPortraits = Path.Combine(bundled, "portraits");
            Directory.CreateDirectory(bundledPortraits);
            foreach (var card in packs[0].Cards)
            {
                File.WriteAllBytes(
                    Path.Combine(bundledPortraits, Path.GetFileName(card.SourcePath)),
                    MakePng(608, 416, withAlpha: false));
            }

            File.WriteAllText(
                Path.Combine(bundled, "BundledProvider.cardartpack.json"),
                "{\"format\":\"card_art_bundle\",\"version\":1,\"count\":0,\"overrides\":[]}",
                new UTF8Encoding(false));

            var rejected = LooseCardPortraitPackScanner.Scan(
                [
                    new LooseCardPortraitProvider("IconProvider", "Icon Provider",
                        Path.GetDirectoryName(icons)!),
                    new LooseCardPortraitProvider("FewCards", "Few Cards",
                        Path.GetDirectoryName(fewMatches)!),
                    new LooseCardPortraitProvider("PackagedProvider", "Packaged Provider", packaged),
                    new LooseCardPortraitProvider("BundledProvider", "Bundled Provider", bundled)
                ],
                ambiguousCards);
            Require(rejected.Count == 0,
                "only a folder that covers cards in bulk may become a pack, got " +
                string.Join("、", rejected.Select(pack => pack.Id)));
        }
        finally
        {
            try { Directory.Delete(root.FullName, recursive: true); }
            catch (IOException) { }
        }
    }

    /// <summary>
    /// A pure asset pack ships no cosmetic manifest, so it is recognised through its bundle alone.
    /// The Workshop item it was installed from must still be offered for submission, exported as
    /// a taken-over card provider, and marked in the Mod list like any other provider.
    /// </summary>
    private static void CheckWorkshopRecognition()
    {
        var root = Directory.CreateTempSubdirectory("sts2-card-art-recognition-");
        try
        {
            var item = Path.Combine(root.FullName, "2868840", "424242");
            Directory.CreateDirectory(item);
            File.WriteAllText(
                Path.Combine(item, "Sample_Pack.cardartpack.json"),
                "{\"format\":\"card_art_bundle\",\"version\":1,\"count\":1,\"overrides\":[" +
                Entry("res://images/packed/card_portraits/silent/abrasive.png", "full_art",
                    MakePng(1000, 760, withAlpha: true), MakePng(1212, 1704, withAlpha: false)) + "]}",
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(item, "Sample_Pack.json.json"),
                "{\"id\":\"Sample_Pack\",\"name\":\"Sample Pack\",\"version\":\"1.0.0\"}",
                new UTF8Encoding(false));

            var game = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]>());
            var cards = new[]
            {
                new CardCatalogEntry("Abrasive", "res://images/packed/card_portraits/silent/abrasive.png",
                    "silent", "silent", "silent")
            };

            // The companion descriptor carries none of the cosmetic manifest fields, so the
            // folder must not be judged by that file alone.
            Require(WorkshopSubmissionScanner.ReadDescriptors(item).Length == 0,
                "a card-art pack folder must not be mistaken for a cosmetic manifest.");
            Require(WorkshopSubmissionScanner.SubmittableNames(item).SequenceEqual(new[] { "Sample Pack" }),
                "a pack-only folder must still be nameable for submission.");
            var described = CardArtPackScanner.Describe(item).Single();
            Require(described is { Id: "Sample_Pack", Name: "Sample Pack" },
                $"the pack descriptor must come from the bundle's companion file, got {described}.");

            // The export must not depend on the loader having registered a descriptor for the
            // pack: the folder the bundle sits in is what ties it to its Workshop item.
            using (var catalog = SkinCatalog.Build(game, []))
            {
                var (covered, used) = catalog.AttachCardArtPacks(
                    CardArtPackScanner.Scan([item]).Packs, cards);
                catalog.FinalizeCardGroups(cards);
                Require(covered.Count == 1 && used.Count == 1 && used[0].Id == "Sample_Pack",
                    "the pack must register the cards it covers.");
                var exported = catalog.ExportWorkshopCatalog().SingleOrDefault(entry => entry.Id == 424242);
                Require(exported != null && exported.Targets.Any(target => target.Kind == "cards"),
                    "an attached pack must export its Workshop item as a taken-over card provider.");
            }

            var scan = WorkshopSubmissionScanner.Scan(
                new(424242, "Sample Pack", item), game, cards, "0.111.0", new Dictionary<string, string>());
            Require(scan.Item is { RestartRequired: true } &&
                    scan.Item.Targets.Any(target => target.Kind == "cards"),
                $"a downloaded pack must be submittable and must not promise hot loading: {scan.Error}");
            Require(!string.Join("", WorkshopSubmissionCodec.Encode(scan.Item!, "1.0.7.1", "0.111.0"))
                    .Contains(":\\", StringComparison.Ordinal),
                "a submission must not encode the author's local paths.");

            ManagedSkinModLoader.RememberArtPackProviders(["Sample_Pack"]);
            Require(ManagedSkinModLoader.IsManagedProviderForDisplay(new Mod
            {
                path = item,
                manifest = new ModManifest { id = "Sample_Pack", name = "Sample Pack" }
            }), "a taken-over pack's Mod must be marked like any other provider.");
            Require(!ManagedSkinModLoader.IsManagedProviderForDisplay(new Mod
            {
                path = item,
                manifest = new ModManifest { id = "Unrelated_Pack", name = "Unrelated" }
            }), "an unrelated Mod must not inherit the pack's marking.");
        }
        finally
        {
            try { Directory.Delete(root.FullName, recursive: true); }
            catch (IOException) { }
        }
    }

    /// <summary>
    /// Verifies that scanning a shared parent root (e.g. Workshop content\2868840 or mods\)
    /// skips directories belonging to manifested Mods that were not loaded (disabled in settings,
    /// failed dependency/version checks, or unmanaged duplicates), still discovers manifest-less
    /// pure-asset packs, falls back previously selected cards to game default without manual
    /// skin_changer.json edits, and restores selection cleanly once the Mod is re-enabled.
    /// </summary>
    internal static void CheckDisabledAndUnloadedModsSkippedAndFallbackToDefault()
    {
        var root = Directory.CreateTempSubdirectory("sts2-card-art-disabled-filter-");
        var serviceType = typeof(SkinService);
        var setCatalog = AccessTools.PropertySetter(serviceType, nameof(SkinService.Catalog))!;
        var setConfig = AccessTools.PropertySetter(serviceType, nameof(SkinService.Config))!;
        var cardLookupField = AccessTools.Field(serviceType, "_cardLookupCache")!;
        var sanitizeCardsMethod = AccessTools.Method(serviceType, "SanitizeCardSelections")!;
        var resolvePortraitMethod = AccessTools.Method(serviceType, "ResolveCardPortraitRequest")!;
        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var previousLookupCache = cardLookupField.GetValue(null);
        try
        {
            var workshopRoot = Directory.CreateDirectory(Path.Combine(root.FullName, "content", "2868840")).FullName;
            var activeDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100001")).FullName;
            var disabledDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100002")).FullName;
            var disabledNestedDir = Directory.CreateDirectory(Path.Combine(disabledDir, "nested")).FullName;
            var failedDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100003")).FullName;
            var dupDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100004")).FullName;
            var settingsDisabledDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100005")).FullName;
            var pureAssetDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100006")).FullName;
            var multiManifestDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100007")).FullName;
            var sharedMixedDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "MixedBundleFolder")).FullName;
            var sharedNoArtDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "MixedNoCardArtFolder")).FullName;
            var parentNoArtDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "ParentDisabledNoArt")).FullName;
            var childActiveDir = Directory.CreateDirectory(Path.Combine(parentNoArtDir, "ChildActiveMod")).FullName;
            var siblingSilentDir = Directory.CreateDirectory(Path.Combine(workshopRoot, "100009")).FullName;

            void WriteBundle(string dir, string id, string name, string portraitPath, bool writeDescriptor = true, int masterHeight = 1704)
            {
                var bundle = "{\"format\":\"card_art_bundle\",\"version\":1,\"count\":1,\"overrides\":[" +
                    Entry(portraitPath, "full_art", MakePng(1000, 760, withAlpha: true), MakePng(1212, masterHeight, withAlpha: false)) +
                    "]}";
                File.WriteAllText(Path.Combine(dir, id + ".cardartpack.json"), bundle, new UTF8Encoding(false));
                if (writeDescriptor)
                {
                    File.WriteAllText(Path.Combine(dir, id + ".json.json"),
                        $"{{\"id\":\"{id}\",\"name\":\"{name}\",\"version\":\"1.0.0\"}}", new UTF8Encoding(false));
                }
            }

            const string abrasivePath = "res://images/packed/card_portraits/silent/abrasive.png";
            const string strikePath = "res://images/packed/card_portraits/ironclad/strike_ironclad.png";
            const string defendPath = "res://images/packed/card_portraits/defect/defend_defect.png";

            WriteBundle(activeDir, "ActiveCardMod", "Active Card Mod", strikePath);
            WriteBundle(disabledDir, "DisabledCardMod", "Disabled Card Mod", abrasivePath);
            WriteBundle(disabledNestedDir, "NestedDisabledCardMod", "Nested Disabled Card Mod", abrasivePath);
            WriteBundle(failedDir, "FailedDepCardMod", "Failed Dep Card Mod", abrasivePath);
            WriteBundle(dupDir, "UnmanagedDupCardMod", "Unmanaged Dup Card Mod", abrasivePath);
            WriteBundle(settingsDisabledDir, "SettingsDisabledMod", "Settings Disabled Mod", abrasivePath);
            WriteBundle(pureAssetDir, "PureAssetCardPack", "Pure Asset Card Pack", defendPath);
            WriteBundle(multiManifestDir, "MultiManifestCardMod", "Multi Manifest Card Mod", strikePath);
            // Shared directory containing both an enabled Mod and a disabled Mod plus card art:
            // stays scannable as a whole directory, and triggers a mixed-directory warning + main-menu banner.
            WriteBundle(sharedMixedDir, "ZetaActive", "Zeta Active", strikePath);
            File.WriteAllText(Path.Combine(sharedMixedDir, "AlphaDisabled.json"),
                "{\"id\":\"AlphaDisabled\",\"name\":\"Alpha Disabled\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            // Shared directory with enabled + disabled Mods but NO *.cardartpack.json must NOT trigger notice.
            File.WriteAllText(Path.Combine(sharedNoArtDir, "NoArtActive.json"),
                "{\"id\":\"NoArtActive\",\"name\":\"No Art Active\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(sharedNoArtDir, "NoArtDisabled.json"),
                "{\"id\":\"NoArtDisabled\",\"name\":\"No Art Disabled\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            // Parent directory with disabled Mod (no *.cardartpack.json) enclosing a child active Mod with card art:
            // must NOT falsely trigger mixed-directory notice on the parent.
            File.WriteAllText(Path.Combine(parentNoArtDir, "ParentDisabled.json"),
                "{\"id\":\"ParentDisabled\",\"name\":\"Parent Disabled\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            WriteBundle(childActiveDir, "ChildActiveMod", "Child Active Mod", strikePath);
            // Sibling silent pack (in its own loaded folder, tested separately in legacy priority check below).
            WriteBundle(siblingSilentDir, "SiblingSilentPack", "Sibling Silent Pack", abrasivePath);

            static Mod MakeMod(string id, string path, ModLoadState state, string? name = null) => new()
            {
                manifest = new ModManifest { id = id, name = name ?? id, version = "1.0.0" },
                path = path,
                state = state,
                modSource = ModSource.SteamWorkshop
            };

            // Use mixed forward slashes and trailing separators to ensure path normalization matches.
            var activeMod = MakeMod("ActiveCardMod", activeDir, ModLoadState.Loaded, "Active Card Mod");
            var disabledMod = MakeMod("DisabledCardMod", disabledDir.Replace('\\', '/') + "/", ModLoadState.Disabled, "Disabled Card Mod");
            var failedMod = MakeMod("FailedDepCardMod", failedDir, ModLoadState.Failed, "Failed Dep Card Mod");
            var dupMod = MakeMod("UnmanagedDupCardMod", dupDir, ModLoadState.DisabledDuplicate, "Unmanaged Dup Card Mod");
            var settingsDisabledMod = MakeMod("SettingsDisabledMod", settingsDisabledDir, ModLoadState.Loaded, "Settings Disabled Mod");
            var multiLoadedMod = MakeMod("MultiManifestCardMod", multiManifestDir, ModLoadState.Loaded, "Multi Manifest Card Mod");
            var multiDupMod = MakeMod("MultiManifestCardMod", multiManifestDir, ModLoadState.DisabledDuplicate, "Multi Manifest Card Mod");
            var sharedActiveMod = MakeMod("ZetaActive", sharedMixedDir, ModLoadState.Loaded, "Zeta Active");
            var sharedDisabledMod = MakeMod("AlphaDisabled", sharedMixedDir, ModLoadState.Disabled, "Alpha Disabled");
            var noArtActiveMod = MakeMod("NoArtActive", sharedNoArtDir, ModLoadState.Loaded, "No Art Active");
            var noArtDisabledMod = MakeMod("NoArtDisabled", sharedNoArtDir, ModLoadState.Disabled, "No Art Disabled");
            var parentDisabledMod = MakeMod("ParentDisabled", parentNoArtDir, ModLoadState.Disabled, "Parent Disabled");
            var childActiveMod = MakeMod("ChildActiveMod", childActiveDir, ModLoadState.Loaded, "Child Active Mod");
            var siblingSilentMod = MakeMod("SiblingSilentPack", siblingSilentDir, ModLoadState.Disabled, "Sibling Silent Pack");

            var modSettings = new ModSettings { PlayerAgreedToModLoading = true };
            modSettings.ModList.Add(new SettingsSaveMod(disabledMod) { IsEnabled = false });
            modSettings.ModList.Add(new SettingsSaveMod(settingsDisabledMod) { IsEnabled = false });
            modSettings.ModList.Add(new SettingsSaveMod(sharedDisabledMod) { IsEnabled = false });
            modSettings.ModList.Add(new SettingsSaveMod(noArtDisabledMod) { IsEnabled = false });
            modSettings.ModList.Add(new SettingsSaveMod(parentDisabledMod) { IsEnabled = false });

            var mods = new[]
            {
                activeMod,
                disabledMod,
                failedMod,
                dupMod,
                settingsDisabledMod,
                multiLoadedMod,
                multiDupMod,
                sharedActiveMod,
                sharedDisabledMod,
                noArtActiveMod,
                noArtDisabledMod,
                parentDisabledMod,
                childActiveMod,
                siblingSilentMod
            };

            // 1) Independent disabled / failed / unmanaged duplicate / settings-disabled manifested
            //    mod directories are excluded; active, mixed (hosting an active mod), and manifest-less
            //    pure-asset directories stay scannable as a whole directory.
            var excluded = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(mods, modSettings);
            var excludedSet = excluded.ToHashSet(StringComparer.OrdinalIgnoreCase);
            Require(excludedSet.Contains(disabledDir) &&
                    excludedSet.Contains(failedDir) &&
                    excludedSet.Contains(dupDir) &&
                    excludedSet.Contains(settingsDisabledDir),
                "禁用、依赖/版本失败、未接管重复以及设置禁用的有清单独立 Mod 目录必须全部进入排除集：" + string.Join(", ", excluded));
            Require(!excludedSet.Contains(activeDir) &&
                    !excludedSet.Contains(pureAssetDir) &&
                    !excludedSet.Contains(multiManifestDir) &&
                    !excludedSet.Contains(sharedMixedDir) &&
                    !excludedSet.Contains(sharedNoArtDir) &&
                    !excludedSet.Contains(parentNoArtDir),
                "已加载 Mod、含活动项的混放目录及无清单纯素材目录不得整目录排除：" + string.Join(", ", excluded));

            var scanDisabled = CardArtPackScanner.Scan([workshopRoot], excluded);
            var foundIds = scanDisabled.Packs.Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            Require(!foundIds.Contains("DisabledCardMod") &&
                    !foundIds.Contains("NestedDisabledCardMod") &&
                    !foundIds.Contains("FailedDepCardMod") &&
                    !foundIds.Contains("UnmanagedDupCardMod") &&
                    !foundIds.Contains("SettingsDisabledMod"),
                "被禁用或未加载的有清单独立 Mod 目录必须被跳过：" + string.Join(", ", foundIds));
            Require(foundIds.Contains("PureAssetCardPack"),
                "没有清单的纯素材卡图包仍必须能被扫描并找到。");
            Require(foundIds.Contains("ActiveCardMod") &&
                    foundIds.Contains("MultiManifestCardMod") &&
                    foundIds.Contains("ChildActiveMod"),
                "正常已加载的 Mod 目录里的卡图包仍必须能被找到。");

            // Structural guards: SkinService.RegisterCardArtPacks must wire directory exclusion + mixed-dir notice,
            // and CacheWriteNoticeReadyPatch must notify MixedCardArtDirectoryNotice on main menu ready.
            var registerMethod = AccessTools.Method(serviceType, "RegisterCardArtPacks")
                                 ?? throw new InvalidOperationException("找不到 SkinService.RegisterCardArtPacks。");
            var registerCalls = PatchProcessor.GetOriginalInstructions(registerMethod)
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(registerCalls.Any(m => m.DeclaringType == typeof(ManagedSkinModLoader) &&
                                           m.Name == nameof(ManagedSkinModLoader.GetUnloadedManifestedModDirectories)),
                "SkinService.RegisterCardArtPacks 必须调用 ManagedSkinModLoader.GetUnloadedManifestedModDirectories 获取目录排除集。");
            Require(registerCalls.Any(m => m.DeclaringType == typeof(CardArtPackScanner) &&
                                           m.Name == nameof(CardArtPackScanner.Scan) &&
                                           m.GetParameters().Length == 2),
                "SkinService.RegisterCardArtPacks 必须把目录排除集传给 CardArtPackScanner.Scan。");
            Require(registerCalls.Any(m => m.DeclaringType == serviceType &&
                                           m.Name == nameof(SkinService.ReportMixedCardArtDirectories)),
                "SkinService.RegisterCardArtPacks 必须调用 SkinService.ReportMixedCardArtDirectories 提示混放目录。");

            static MethodInfo[] CollectPatchTargetCalls(string patchTypeName)
            {
                var patchType = typeof(SkinService).Assembly.GetType(patchTypeName, throwOnError: true)!;
                var postfix = AccessTools.Method(patchType, "Postfix")
                              ?? throw new InvalidOperationException($"找不到 {patchTypeName}.Postfix。");
                return PatchProcessor.GetOriginalInstructions(postfix)
                    .Select(i => i.operand)
                    .OfType<MethodInfo>()
                    .Concat(patchType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                        .SelectMany(nested => nested.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        .Where(m => m.GetMethodBody() != null)
                        .SelectMany(m => PatchProcessor.GetOriginalInstructions(m))
                        .Select(i => i.operand)
                        .OfType<MethodInfo>())
                    .ToArray();
            }

            var mainMenuPopupCalls = CollectPatchTargetCalls("STS2SkinChanger.Ui.LoadOrderWarning.MainMenuLoadOrderWarningPatch");
            var modalReadyPopupCalls = CollectPatchTargetCalls("STS2SkinChanger.Ui.LoadOrderWarning.LoadOrderWarningModalReadyPatch");
            var modalClosedPopupCalls = CollectPatchTargetCalls("STS2SkinChanger.Ui.LoadOrderWarning.LoadOrderWarningModalClosedPatch");
            Require(mainMenuPopupCalls.Any(m => m.DeclaringType == typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice) &&
                                                m.Name == nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.OnMainMenuReady)),
                "MainMenuLoadOrderWarningPatch 必须在加载顺序检查之后通知 MixedCardArtDirectoryNotice.OnMainMenuReady。");
            Require(modalReadyPopupCalls.Any(m => m.DeclaringType == typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice) &&
                                                  m.Name == nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.TryShow)) &&
                    modalClosedPopupCalls.Any(m => m.DeclaringType == typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice) &&
                                                   m.Name == nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose)) &&
                    !modalClosedPopupCalls.Any(m => m.DeclaringType == typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice) &&
                                                    m.Name == nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.TryShow)),
                "NModalContainer._Ready 补丁必须衔接 TryShow，而 Clear 补丁必须经 ScheduleTryShowAfterClose 延迟到关闭回调结束后再接续，避免在重启按钮释放的 Clear 中抢弹。");

            // 2) Previously selected card art from DisabledCardMod falls back to Game Default without
            //    manual skin_changer.json edits when the Mod is disabled, and works again once re-enabled.
            var gamePath = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(gamePath, new Dictionary<string, byte[]>());
            var silentPool = new DisabledFilterSilentPool();
            var abrasiveCard = new DisabledFilterAbrasiveCard();
            AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField").SetValue(abrasiveCard, new ModelId("CARD", "ABRASIVE"));
            AccessTools.Field(typeof(CardModel), "_pool").SetValue(abrasiveCard, silentPool);

            var cardEntries = new[]
            {
                new CardCatalogEntry(nameof(DisabledFilterAbrasiveCard), abrasivePath, "silent", "silent", "silent")
                {
                    IsCharacterPool = true,
                    IsGameCard = true
                },
                new CardCatalogEntry("StrikeIronclad", strikePath, "ironclad", "ironclad", "ironclad")
                {
                    IsCharacterPool = true,
                    IsGameCard = true
                },
                new CardCatalogEntry("DefendDefect", defendPath, "defect", "defect", "defect")
                {
                    IsCharacterPool = true,
                    IsGameCard = true
                }
            };

            // Stored user config from an earlier session when DisabledCardMod was enabled & selected
            // (both category selection/priority AND per-card override). User did NOT edit skin_changer.json.
            var config = new SkinConfig { CardPriorityDefaultsVersion = 1 };
            config.Selections["cards:silent"] = "DisabledCardMod";
            config.Selections["cards:item:card.abrasive"] = "DisabledCardMod";
            config.CardSkinPriorities["silent"] = [new CardSkinPriorityEntry("DisabledCardMod", Enabled: true)];

            void ResetCardLookupCache() =>
                cardLookupField.SetValue(null, Activator.CreateInstance(cardLookupField.FieldType)!);

            var inspectControlsType = typeof(SkinService).Assembly.GetType("STS2SkinChanger.Ui.CardInspectSkinControls", throwOnError: true)!;
            var selectionDisplayNameMethod = AccessTools.Method(inspectControlsType, "SelectionDisplayName")
                                             ?? throw new InvalidOperationException("找不到 CardInspectSkinControls.SelectionDisplayName。");

            // Launch 1: DisabledCardMod is disabled -> skipped from catalog -> Abrasive must fall back to Game Default
            // and collapsed individual-card dropdown must display "Follow Category" instead of the stale disabled pack ID,
            // while keeping the stored override untouched in Config for re-enable.
            using (var disabledCatalog = SkinCatalog.Build(gamePath, []))
            {
                disabledCatalog.AttachCardArtPacks(scanDisabled.Packs, cardEntries);
                disabledCatalog.FinalizeCardGroups(cardEntries);
                setCatalog.Invoke(null, [disabledCatalog]);
                setConfig.Invoke(null, [config]);
                ResetCardLookupCache();
                sanitizeCardsMethod.Invoke(null, null);

                Require(!disabledCatalog.CardGroups.Any(g => g.Options.Any(o => o.Id.Equals("DisabledCardMod", StringComparison.OrdinalIgnoreCase))),
                    "禁用后 DisabledCardMod 不得进入卡牌皮肤目录。");
                Require(disabledCatalog.CardGroups.Any(g => g.Id == "defect" && g.Options.Any(o => o.Id == "PureAssetCardPack")),
                    "无清单纯素材卡图包 PureAssetCardPack 应照常进入目录。");
                Require(SkinService.GetCardSelection("silent") == SkinCatalog.BaseOptionId,
                    $"包被跳过后分组选择必须回退游戏默认（__base__），无需手改配置，实际：{SkinService.GetCardSelection("silent")}");
                Require(SkinService.GetEffectiveCardSelection(abrasiveCard) == SkinCatalog.BaseOptionId,
                    $"包被跳过后单卡生效选择必须回退游戏默认（__base__），实际：{SkinService.GetEffectiveCardSelection(abrasiveCard)}");
                var collapsedOverride = SkinService.GetCardOverrideSelection(abrasiveCard);
                var collapsedLabel = (string)selectionDisplayNameMethod.Invoke(null, [collapsedOverride, abrasiveCard])!;
                Require(collapsedOverride == SkinService.InheritCardSelectionId &&
                        !collapsedLabel.Contains("DisabledCardMod", StringComparison.OrdinalIgnoreCase) &&
                        collapsedLabel == STS2SkinChanger.Ui.CardSkinControls.InheritedSelectionText(abrasiveCard),
                    $"单卡详情收起下拉遇到失效覆盖时必须按可用选项回退显示「跟随分类」，实际：id={collapsedOverride}, label={collapsedLabel}");
                Require(config.Selections["cards:item:card.abrasive"] == "DisabledCardMod",
                    "单卡显示层回退不得破坏存档中的旧单卡覆盖值，以便重新启用后自动恢复。");
                Require(SkinService.GetSelectedCardOption(abrasiveCard) == null &&
                        SkinService.GetCardPresentation(abrasiveCard) == null &&
                        resolvePortraitMethod.Invoke(null, [abrasiveCard]) == null,
                    "包被跳过后启动即应直接呈现游戏原版卡面与原版布局。");
                var emptyOverlay = disabledCatalog.BuildCardOverlay(
                    config.Selections,
                    new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "silent" });
                Require(emptyOverlay.Count == 0, "跳过后不得为缺失卡图包挂载任何覆盖文件。");
            }

            // Launch 2: Player re-enables DisabledCardMod in Mod Manager (same untouched skin_changer.json Config!).
            disabledMod.state = ModLoadState.Loaded;
            modSettings.ModList.RemoveAll(entry => string.Equals(entry.Id, disabledMod.manifest!.id, StringComparison.OrdinalIgnoreCase));
            var excludedAfterReenable = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(mods, modSettings);
            Require(!excludedAfterReenable.Contains(disabledDir, StringComparer.OrdinalIgnoreCase),
                "重新启用 Mod 后，其目录不得再被排除。");

            var scanReenabled = CardArtPackScanner.Scan([workshopRoot], excludedAfterReenable);
            Require(scanReenabled.Packs.Any(p => p.Id.Equals("DisabledCardMod", StringComparison.OrdinalIgnoreCase)),
                "重新启用后 DisabledCardMod 的卡图包必须能被重新扫描到。");

            using (var reenabledCatalog = SkinCatalog.Build(gamePath, []))
            {
                reenabledCatalog.AttachCardArtPacks(scanReenabled.Packs, cardEntries);
                reenabledCatalog.FinalizeCardGroups(cardEntries);
                setCatalog.Invoke(null, [reenabledCatalog]);
                setConfig.Invoke(null, [config]);
                ResetCardLookupCache();
                sanitizeCardsMethod.Invoke(null, null);

                Require(reenabledCatalog.CardGroups.Any(g =>
                        g.Id == "silent" &&
                        g.Options.Any(o => o.Id.Equals("DisabledCardMod", StringComparison.OrdinalIgnoreCase))),
                    "重新启用后 DisabledCardMod 必须重新出现在 silent 卡牌选项中。");
                Require(SkinService.GetCardSelection("silent") == "DisabledCardMod" &&
                        SkinService.GetCardOverrideSelection(abrasiveCard) == "DisabledCardMod" &&
                        SkinService.GetEffectiveCardSelection(abrasiveCard) == "DisabledCardMod" &&
                        SkinService.GetSelectedCardOption(abrasiveCard)?.Id == "DisabledCardMod",
                    $"重新启用后原有配置中的 DisabledCardMod 应可重新选中并生效，实际：group={SkinService.GetCardSelection("silent")}, override={SkinService.GetCardOverrideSelection(abrasiveCard)}, card={SkinService.GetEffectiveCardSelection(abrasiveCard)}");
                var restoredLabel = (string)selectionDisplayNameMethod.Invoke(null, [SkinService.GetCardOverrideSelection(abrasiveCard), abrasiveCard])!;
                Require(restoredLabel == "Disabled Card Mod",
                    $"重新启用后单卡详情下拉应恢复显示该包名称，实际：{restoredLabel}");
                var portraitRequest = resolvePortraitMethod.Invoke(null, [abrasiveCard]);
                Require(portraitRequest != null &&
                        (bool)portraitRequest.GetType().GetProperty("UseSelectedProvider")!.GetValue(portraitRequest)!,
                    "重新启用后应能正常解析并使用 DisabledCardMod 的自定义卡图。");
                Require(SkinService.GetCardPresentation(abrasiveCard) is { UseExpandedPortraitLayout: true },
                    "重新启用后全画幅布局应随之恢复。");
            }

            // 3) Legacy config (only Selections["cards:silent"] = "DisabledCardMod", NO CardSkinPriorities)
            //    across two launches when sibling pack SiblingSilentPack stays enabled in the same pool:
            //    Launch 1 (DisabledCardMod disabled, SiblingSilentPack active) must preserve DisabledCardMod
            //    as the pending top priority slot; Launch 2 (DisabledCardMod re-enabled) must restore
            //    DisabledCardMod back to #1 priority ahead of SiblingSilentPack instead of appending it at the end.
            disabledMod.state = ModLoadState.Disabled;
            modSettings.ModList.Add(new SettingsSaveMod(disabledMod) { IsEnabled = false });
            siblingSilentMod.state = ModLoadState.Loaded;
            var legacyConfig = new SkinConfig { CardPriorityDefaultsVersion = 1 };
            legacyConfig.Selections["cards:silent"] = "DisabledCardMod";

            var getPriorityEntriesMethod = AccessTools.Method(serviceType, "GetCardPriorityEntriesInternal")
                                           ?? throw new InvalidOperationException("找不到 SkinService.GetCardPriorityEntriesInternal。");

            var legacyExcludedLaunch1 = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(mods, modSettings);
            var legacyScanLaunch1 = CardArtPackScanner.Scan([workshopRoot], legacyExcludedLaunch1);
            using (var legacyCatalog1 = SkinCatalog.Build(gamePath, []))
            {
                legacyCatalog1.AttachCardArtPacks(legacyScanLaunch1.Packs, cardEntries);
                legacyCatalog1.FinalizeCardGroups(cardEntries);
                setCatalog.Invoke(null, [legacyCatalog1]);
                setConfig.Invoke(null, [legacyConfig]);
                ResetCardLookupCache();
                sanitizeCardsMethod.Invoke(null, null);

                var silentGroup1 = legacyCatalog1.CardGroups.Single(g => g.Id == "silent");
                var visibleLaunch1 = (IReadOnlyList<CardSkinPriorityEntry>)getPriorityEntriesMethod.Invoke(null, [silentGroup1, false])!;
                Require(visibleLaunch1.Count == 1 && visibleLaunch1[0].OptionId == "SiblingSilentPack",
                    "旧配置禁用 A 但同池还有 B 时，当前可见优先级列表仅应显示 B。");
                Require(SkinService.GetEffectiveCardSelection(abrasiveCard) == "SiblingSilentPack",
                    $"禁用 A 时单卡应切到同池可用包 B，实际：{SkinService.GetEffectiveCardSelection(abrasiveCard)}");
                Require(legacyConfig.CardSkinPriorities["silent"].Select(e => e.OptionId)
                            .SequenceEqual(["DisabledCardMod", "SiblingSilentPack"], StringComparer.OrdinalIgnoreCase) &&
                        legacyConfig.CardSkinPriorities["silent"][0].Enabled,
                    "旧配置迁移分支必须把暂缺的旧选择 A 保留在首位待恢复，实际：" +
                    string.Join(" -> ", legacyConfig.CardSkinPriorities["silent"].Select(e => $"{e.OptionId}:{e.Enabled}")));
            }

            // Launch 2 for legacy config: re-enable DisabledCardMod alongside SiblingSilentPack.
            disabledMod.state = ModLoadState.Loaded;
            modSettings.ModList.RemoveAll(entry => string.Equals(entry.Id, disabledMod.manifest!.id, StringComparison.OrdinalIgnoreCase));
            var legacyExcludedLaunch2 = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(mods, modSettings);
            var legacyScanLaunch2 = CardArtPackScanner.Scan([workshopRoot], legacyExcludedLaunch2);
            using (var legacyCatalog2 = SkinCatalog.Build(gamePath, []))
            {
                legacyCatalog2.AttachCardArtPacks(legacyScanLaunch2.Packs, cardEntries);
                legacyCatalog2.FinalizeCardGroups(cardEntries);
                setCatalog.Invoke(null, [legacyCatalog2]);
                setConfig.Invoke(null, [legacyConfig]);
                ResetCardLookupCache();
                sanitizeCardsMethod.Invoke(null, null);

                var silentGroup2 = legacyCatalog2.CardGroups.Single(g => g.Id == "silent");
                var visibleLaunch2 = (IReadOnlyList<CardSkinPriorityEntry>)getPriorityEntriesMethod.Invoke(null, [silentGroup2, false])!;
                Require(visibleLaunch2.Select(o => o.OptionId).SequenceEqual(["DisabledCardMod", "SiblingSilentPack", "NestedDisabledCardMod"], StringComparer.OrdinalIgnoreCase) &&
                        visibleLaunch2[0].Enabled,
                    "旧配置在重新启用 A 后必须恢复 A 为首位优先项（其余新出现包才追加到末尾），实际：" +
                    string.Join(" -> ", visibleLaunch2.Select(o => $"{o.OptionId}:{o.Enabled}")));
                Require(SkinService.GetCardSelection("silent") == "DisabledCardMod" &&
                        SkinService.GetEffectiveCardSelection(abrasiveCard) == "DisabledCardMod",
                    $"旧配置重新启用 A 后分类与单卡生效项必须自动恢复为 A，实际：group={SkinService.GetCardSelection("silent")}, card={SkinService.GetEffectiveCardSelection(abrasiveCard)}");
            }

            // 4) Baseline ff04fa5 metadata compatibility fixtures:
            //    4a) Unexcluded directory with A.json, B.json and renamed.cardartpack.json resolves to A / Declared A.
            var baselineRoot = Directory.CreateDirectory(Path.Combine(root.FullName, "baseline-compat")).FullName;
            var renamedDir = Directory.CreateDirectory(Path.Combine(baselineRoot, "renamed-pair")).FullName;
            File.WriteAllText(Path.Combine(renamedDir, "A.json"),
                "{\"id\":\"A\",\"name\":\"Declared A\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(renamedDir, "B.json"),
                "{\"id\":\"B\",\"name\":\"Declared B\",\"version\":\"1.0.0\"}", new UTF8Encoding(false));
            WriteBundle(renamedDir, "renamed", "Ignored", strikePath, writeDescriptor: false);
            var renamedScan = CardArtPackScanner.Scan([renamedDir]);
            var renamedDescribed = CardArtPackScanner.Describe(renamedDir);
            Require(renamedScan.Packs.Count == 1 &&
                    renamedScan.Packs[0].Id == "A" &&
                    renamedScan.Packs[0].Name == "Declared A",
                $"无排除目录下 A.json + B.json + renamed.cardartpack.json 必须保持 ff04fa5 归属（A / Declared A），实际：{renamedScan.Packs.FirstOrDefault()?.Id} / {renamedScan.Packs.FirstOrDefault()?.Name}");
            Require(renamedDescribed.Count == 1 &&
                    renamedDescribed[0].Id == "A" &&
                    renamedDescribed[0].Name == "Declared A",
                $"CardArtPackScanner.Describe 必须保持 ff04fa5 归属（A / Declared A），实际：{renamedDescribed.FirstOrDefault()}");

            //    4b) Similar names FoolArt / FoolArtNSFW / FoolArt_NSFW in their own folders:
            //        disabling FoolArt excludes ONLY FoolArt and leaves both FoolArtNSFW and FoolArt_NSFW intact.
            var similarRoot = Directory.CreateDirectory(Path.Combine(baselineRoot, "similar-names")).FullName;
            var foolArtDir = Directory.CreateDirectory(Path.Combine(similarRoot, "FoolArt")).FullName;
            var foolArtNsfwDir = Directory.CreateDirectory(Path.Combine(similarRoot, "FoolArtNSFW")).FullName;
            var foolArtUnderscoreDir = Directory.CreateDirectory(Path.Combine(similarRoot, "FoolArt_NSFW")).FullName;
            WriteBundle(foolArtDir, "FoolArt", "Fool Art", strikePath);
            WriteBundle(foolArtNsfwDir, "FoolArtNSFW", "Fool Art NSFW", strikePath);
            WriteBundle(foolArtUnderscoreDir, "FoolArt_NSFW", "Fool Art Underscore NSFW", strikePath);
            var similarUnexcluded = CardArtPackScanner.Scan([similarRoot]);
            Require(similarUnexcluded.Packs.Select(p => (p.Id, p.Name))
                        .OrderBy(p => p.Id, StringComparer.Ordinal)
                        .SequenceEqual(new[]
                        {
                            ("FoolArt", "Fool Art"),
                            ("FoolArtNSFW", "Fool Art NSFW"),
                            ("FoolArt_NSFW", "Fool Art Underscore NSFW")
                        }),
                "无排除扫描 FoolArt / FoolArtNSFW / FoolArt_NSFW 必须保持 ff04fa5 结果。");
            var similarMods = new[]
            {
                MakeMod("FoolArt", foolArtDir, ModLoadState.Disabled, "Fool Art"),
                MakeMod("FoolArtNSFW", foolArtNsfwDir, ModLoadState.Loaded, "Fool Art NSFW"),
                MakeMod("FoolArt_NSFW", foolArtUnderscoreDir, ModLoadState.Loaded, "Fool Art Underscore NSFW")
            };
            var similarSettings = new ModSettings { PlayerAgreedToModLoading = true };
            similarSettings.ModList.Add(new SettingsSaveMod(similarMods[0]) { IsEnabled = false });
            var similarExcluded = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(similarMods, similarSettings);
            var similarFiltered = CardArtPackScanner.Scan([similarRoot], similarExcluded);
            Require(similarFiltered.Packs.Select(p => p.Id)
                        .OrderBy(id => id, StringComparer.Ordinal)
                        .SequenceEqual(["FoolArtNSFW", "FoolArt_NSFW"]),
                "禁用 FoolArt 时仅可排除其独立目录，不得误删 FoolArtNSFW 或 FoolArt_NSFW，实际：" +
                string.Join(", ", similarFiltered.Packs.Select(p => p.Id)));

            // 5) Mixed-directory warning log + non-modal main-menu banner + 15-language coverage:
            //    - Triggers ONLY for exact same directory containing both enabled and unloaded distinct
            //      Mods plus *.cardartpack.json (sharedMixedDir);
            //    - Does NOT trigger for independent directories, mixed directories without *.cardartpack.json,
            //      same-ID duplicate directories, or parent-child layouts even when the parent disabled Mod
            //      carries its own *.cardartpack.json (while whole-directory scan protection stays intact).
            var independentOnlyMods = new[] { activeMod, disabledMod, failedMod, dupMod, settingsDisabledMod, multiLoadedMod, multiDupMod };
            Require(ManagedSkinModLoader.GetMixedCardArtDirectories(independentOnlyMods, modSettings).Count == 0,
                "独立目录与同 ID 重复目录不得触发混放目录提示。");
            var noArtMixedMods = new[] { noArtActiveMod, noArtDisabledMod, parentDisabledMod, childActiveMod };
            Require(ManagedSkinModLoader.GetMixedCardArtDirectories(noArtMixedMods, modSettings).Count == 0,
                "不含 *.cardartpack.json 的混放目录与父目录无卡图的嵌套目录不得触发混放目录提示。");

            var parentChildRoot = Directory.CreateDirectory(Path.Combine(baselineRoot, "parent-child-with-art")).FullName;
            var parentDisabledWithArtDir = Directory.CreateDirectory(Path.Combine(parentChildRoot, "ParentDisabledWithArt")).FullName;
            var childActiveInParentDir = Directory.CreateDirectory(Path.Combine(parentDisabledWithArtDir, "ChildActiveOwnDir")).FullName;
            WriteBundle(parentDisabledWithArtDir, "ParentDisabledWithArt", "Parent Disabled With Art", strikePath);
            WriteBundle(childActiveInParentDir, "ChildActiveOwnDir", "Child Active Own Dir", defendPath);
            var parentDisabledWithArtMod = MakeMod("ParentDisabledWithArt", parentDisabledWithArtDir, ModLoadState.Disabled, "Parent Disabled With Art");
            var childActiveOwnMod = MakeMod("ChildActiveOwnDir", childActiveInParentDir, ModLoadState.Loaded, "Child Active Own Dir");
            var parentChildSettings = new ModSettings { PlayerAgreedToModLoading = true };
            parentChildSettings.ModList.Add(new SettingsSaveMod(parentDisabledWithArtMod) { IsEnabled = false });
            var parentChildMods = new[] { parentDisabledWithArtMod, childActiveOwnMod };
            var parentChildExcluded = ManagedSkinModLoader.GetUnloadedManifestedModDirectories(parentChildMods, parentChildSettings);
            Require(!parentChildExcluded.Contains(parentDisabledWithArtDir, StringComparer.OrdinalIgnoreCase),
                "父目录禁用 Mod 包裹独立子目录活动 Mod 时，扫描保护仍应保留父目录不被整目录排除。");
            Require(ManagedSkinModLoader.GetMixedCardArtDirectories(parentChildMods, parentChildSettings).Count == 0,
                "父目录自带卡图的禁用 Mod 与独立子目录活动 Mod 不是精确同目录清单，不得误报混放目录提示。");

            var warnLogs = new List<string>();
            STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.ResetForTests();
            var reportedConflicts = SkinService.ReportMixedCardArtDirectories(mods, modSettings, warnLogs.Add);
            Require(reportedConflicts.Count == 1 &&
                    reportedConflicts[0].DirectoryPath.Equals(sharedMixedDir, StringComparison.OrdinalIgnoreCase) &&
                    reportedConflicts[0].FolderName == "MixedBundleFolder" &&
                    reportedConflicts[0].ActiveModNames.SequenceEqual(["Zeta Active"]) &&
                    reportedConflicts[0].UnloadedModNames.SequenceEqual(["Alpha Disabled"]),
                "只有同时包含已启用和未加载 Mod 且含 *.cardartpack.json 的混放目录应触发提示，实际：" +
                string.Join(", ", reportedConflicts.Select(c => c.DirectoryPath)));
            Require(warnLogs.Count == 1 &&
                    warnLogs[0].Contains(sharedMixedDir, StringComparison.OrdinalIgnoreCase) &&
                    warnLogs[0].Contains("Zeta Active", StringComparison.Ordinal) &&
                    warnLogs[0].Contains("Alpha Disabled", StringComparison.Ordinal),
                $"混放目录日志警告必须包含完整目录路径与涉及的 Mod 名，实际：{string.Join(" | ", warnLogs)}");

            var noticeState = new MixedCardArtDirectoryNoticeState();
            noticeState.Report(reportedConflicts);
            Require(!noticeState.TryTakeForMainMenu(out _),
                "到达主菜单前不得提前弹出混放目录弹窗。");
            noticeState.MarkMainMenuReached();
            Require(!noticeState.TryTakeForMainMenu(out _, deferDisplay: true),
                "加载顺序弹窗显示中（含关闭回调尚未走完）或其它模态框打开/待显示时，混放目录弹窗必须暂缓等待。");
            // Restart path: once LoadOrderWarning requests a restart, even when OpenModal is cleared
            // and LoadOrderWarning is no longer pending/showing, the mixed popup must NOT pop up.
            Require(!noticeState.TryTakeForMainMenu(out _, deferDisplay: false, suppressDisplay: true),
                "加载顺序弹窗点击「重启」后即使容器已清空也不得再弹出混放目录弹窗。");
            // Normal close path: once LoadOrderWarning closes without requesting restart, pop up once.
            Require(noticeState.TryTakeForMainMenu(out var takenConflicts, deferDisplay: false, suppressDisplay: false) &&
                    takenConflicts.Count == 1 &&
                    takenConflicts[0].FolderName == "MixedBundleFolder",
                "前置弹窗普通关闭且未请求重启后，混放目录弹窗必须照常弹出一次。");
            noticeState.RestoreForRetry(takenConflicts);
            Require(noticeState.TryTakeForMainMenu(out var retriedConflicts, deferDisplay: false, suppressDisplay: false) &&
                    retriedConflicts.Count == 1,
                "弹窗节点尚未就绪或创建失败调用 RestoreForRetry 后，应允许在下一次钩子重试显示。");
            noticeState.MarkMainMenuReached();
            Require(!noticeState.TryTakeForMainMenu(out _),
                "同一次启动内再次回到主菜单不得重复弹出弹窗。");
            var nextLaunchState = new MixedCardArtDirectoryNoticeState();
            nextLaunchState.Report(reportedConflicts);
            nextLaunchState.MarkMainMenuReached();
            Require(nextLaunchState.TryTakeForMainMenu(out _),
                "下次启动条件仍成立时必须照常弹出一次（无永久屏蔽开关或不再提示）。");

            var loadOrderControllerType = typeof(SkinService).Assembly.GetType("STS2SkinChanger.Ui.LoadOrderWarning.LoadOrderWarningController", throwOnError: true)!;
            var showingField = AccessTools.Field(loadOrderControllerType, "_showingPopup")
                               ?? throw new InvalidOperationException("找不到 LoadOrderWarningController._showingPopup。");
            var restartField = AccessTools.Field(loadOrderControllerType, "_restartRequested")
                               ?? throw new InvalidOperationException("找不到 LoadOrderWarningController._restartRequested。");
            var pendingField = AccessTools.Field(loadOrderControllerType, "_pending")
                               ?? throw new InvalidOperationException("找不到 LoadOrderWarningController._pending。");
            var isShowingOrPendingProp = AccessTools.Property(loadOrderControllerType, "IsShowingOrPending")
                                         ?? throw new InvalidOperationException("找不到 LoadOrderWarningController.IsShowingOrPending。");
            var isRestartRequestedProp = AccessTools.Property(loadOrderControllerType, "IsRestartRequested")
                                         ?? throw new InvalidOperationException("找不到 LoadOrderWarningController.IsRestartRequested。");
            var prevShowing = showingField.GetValue(null);
            var prevRestart = restartField.GetValue(null);
            var prevPending = pendingField.GetValue(null);
            try
            {
                // Simulate LoadOrderWarning confirmation flow: while popup is open / close callback still running,
                // IsShowingOrPending is true; after Restart is clicked, IsRestartRequested stays true even once
                // _showingPopup and _pending are cleared; after normal close, both are false.
                pendingField.SetValue(null, false);
                showingField.SetValue(null, true);
                restartField.SetValue(null, false);
                Require((bool)isShowingOrPendingProp.GetValue(null)! && !(bool)isRestartRequestedProp.GetValue(null)!,
                    "加载顺序弹窗尚未走完关闭回调时 IsShowingOrPending 必须为 true，防止 Clear 后置抢弹。");

                showingField.SetValue(null, false);
                restartField.SetValue(null, true);
                Require(!(bool)isShowingOrPendingProp.GetValue(null)! && (bool)isRestartRequestedProp.GetValue(null)!,
                    "点击重启后 IsRestartRequested 必须保持 true，阻止退出前弹出混放弹窗。");

                var restartState = new MixedCardArtDirectoryNoticeState();
                restartState.Report(reportedConflicts);
                restartState.MarkMainMenuReached();
                Require(!restartState.TryTakeForMainMenu(
                        out _,
                        deferDisplay: (bool)isShowingOrPendingProp.GetValue(null)!,
                        suppressDisplay: (bool)isRestartRequestedProp.GetValue(null)!),
                    "结合 LoadOrderWarningController.IsRestartRequested 状态时，点击重启后不得消费或弹出混放弹窗。");

                restartField.SetValue(null, false);
                Require(restartState.TryTakeForMainMenu(
                        out _,
                        deferDisplay: (bool)isShowingOrPendingProp.GetValue(null)!,
                        suppressDisplay: (bool)isRestartRequestedProp.GetValue(null)!),
                    "结合 LoadOrderWarningController 状态时，普通关闭结束后必须正常接续弹出混放弹窗。");
            }
            finally
            {
                showingField.SetValue(null, prevShowing);
                restartField.SetValue(null, prevRestart);
                pendingField.SetValue(null, prevPending);
            }

            var mixedTryShow = AccessTools.Method(typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice), "TryShow")
                               ?? throw new InvalidOperationException("找不到 MixedCardArtDirectoryNotice.TryShow。");
            var mixedTryShowCalls = PatchProcessor.GetOriginalInstructions(mixedTryShow)
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(mixedTryShowCalls.Any(m => m.DeclaringType == loadOrderControllerType &&
                                               m.Name == "get_IsShowingOrPending") &&
                    mixedTryShowCalls.Any(m => m.DeclaringType == loadOrderControllerType &&
                                               m.Name == "get_IsRestartRequested") &&
                    mixedTryShowCalls.Any(m => m.DeclaringType == typeof(MegaCrit.Sts2.Core.Nodes.CommonUi.NModalContainer) &&
                                               m.Name == "get_OpenModal"),
                "MixedCardArtDirectoryNotice.TryShow 必须检查 NModalContainer.OpenModal、LoadOrderWarningController.IsShowingOrPending 与 IsRestartRequested。");

            var scheduleAfterClose = AccessTools.Method(typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice), nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose))
                                     ?? throw new InvalidOperationException("找不到 MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose。");
            var scheduleAfterCloseCalls = PatchProcessor.GetOriginalInstructions(scheduleAfterClose)
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(scheduleAfterCloseCalls.Any(m => m.DeclaringType == typeof(DeferredDispatch) &&
                                                     m.Name == nameof(DeferredDispatch.CallDeferred)),
                "ScheduleTryShowAfterClose 必须经 DeferredDispatch.CallDeferred 延后到关闭回调之外执行。");

            var restartMethod = AccessTools.Method(loadOrderControllerType, "RestartGame")
                                ?? throw new InvalidOperationException("找不到 LoadOrderWarningController.RestartGame。");
            var restartWrittenFields = PatchProcessor.GetOriginalInstructions(restartMethod)
                .Select(i => i.operand)
                .OfType<FieldInfo>()
                .ToArray();
            Require(restartWrittenFields.Any(f => f == restartField),
                "LoadOrderWarningController.RestartGame 必须设置 _restartRequested 标记。");

            var loadOrderNestedCalls = loadOrderControllerType
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .SelectMany(nested => nested.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .Where(m => m.GetMethodBody() != null)
                .SelectMany(m => PatchProcessor.GetOriginalInstructions(m))
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(loadOrderNestedCalls.Any(m => m.DeclaringType == typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice) &&
                                                  m.Name == nameof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose)),
                "LoadOrderWarningController.ShowWarning 结束后（未请求重启时）必须接续 ScheduleTryShowAfterClose。");

            var showPopupCalls = typeof(STS2SkinChanger.Ui.MixedCardArtDirectoryNotice)
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .SelectMany(nested => nested.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .Where(m => m.GetMethodBody() != null)
                .SelectMany(m => PatchProcessor.GetOriginalInstructions(m))
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(showPopupCalls.Any(m => m.DeclaringType == typeof(MegaCrit.Sts2.Core.Nodes.Multiplayer.NGenericPopup) &&
                                            m.Name == nameof(MegaCrit.Sts2.Core.Nodes.Multiplayer.NGenericPopup.Create)) &&
                    showPopupCalls.Any(m => m.DeclaringType == typeof(MegaCrit.Sts2.Core.Nodes.CommonUi.NVerticalPopup) &&
                                            m.Name == nameof(MegaCrit.Sts2.Core.Nodes.CommonUi.NVerticalPopup.SetText)) &&
                    showPopupCalls.Any(m => m.DeclaringType == typeof(MegaCrit.Sts2.Core.Nodes.CommonUi.NVerticalPopup) &&
                                            m.Name == "get_NoButton") &&
                    showPopupCalls.Any(m => m.DeclaringType == typeof(CanvasItem) &&
                                            m.Name == nameof(CanvasItem.Hide)),
                "混放目录提示必须复用 NGenericPopup/NVerticalPopup 竖排弹窗并隐藏 NoButton，只留单个「知道了」按钮。");

            var mixedTranslations = (System.Collections.IDictionary)AccessTools.Field(
                typeof(ModLocalization),
                "MixedCardArtDirectoryTranslations").GetValue(null)!;
            Require(mixedTranslations.Count == ModLocalization.SupportedLanguages.Count &&
                    ModLocalization.SupportedLanguages.All(lang => mixedTranslations.Contains(lang)),
                "混放目录弹窗多语言必须覆盖 ModLocalization.SupportedLanguages 的全部 15 种语言。");
            foreach (var language in ModLocalization.SupportedLanguages)
            {
                var popupTitle = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupTitle(language);
                var popupBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(reportedConflicts, language);
                Require(!string.IsNullOrWhiteSpace(popupTitle) &&
                        !string.IsNullOrWhiteSpace(popupBody) &&
                        popupBody.Contains("MixedBundleFolder", StringComparison.Ordinal),
                    $"混放目录弹窗在语言 {language} 下标题/正文为空或缺少文件夹名：title={popupTitle} | body={popupBody}");
            }

            var zhsTitle = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupTitle("zhs");
            var zhtTitle = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupTitle("zht");
            var engTitle = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupTitle("eng");
            var zhsBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(reportedConflicts, "zhs");
            var zhtBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(reportedConflicts, "zht");
            var engBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(reportedConflicts, "eng");
            var fallbackTitle = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupTitle("unknown");
            var fallbackBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(reportedConflicts, "unknown");
            var escapedBody = STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.FormatPopupMessage(
                [new MixedCardArtDirectoryConflict(sharedMixedDir, "[Pack] Mixed", ["A"], ["B"])],
                "eng");
            Require(zhsTitle.Contains("混放", StringComparison.Ordinal) &&
                    zhtTitle.Contains("資料夾", StringComparison.Ordinal) &&
                    engTitle.Contains("Mixed Mod folder", StringComparison.Ordinal) &&
                    zhsBody.Contains("「MixedBundleFolder」", StringComparison.Ordinal) &&
                    zhsBody.Contains("禁用不会让这些卡图失效", StringComparison.Ordinal) &&
                    zhtBody.Contains("「MixedBundleFolder」", StringComparison.Ordinal) &&
                    zhtBody.Contains("資料夾", StringComparison.Ordinal) &&
                    engBody.Contains("\"MixedBundleFolder\"", StringComparison.Ordinal) &&
                    fallbackTitle == engTitle &&
                    fallbackBody == engBody &&
                    escapedBody.Contains("(Pack) Mixed", StringComparison.Ordinal) &&
                    !escapedBody.Contains("[Pack]", StringComparison.Ordinal),
                $"中英文/繁体/BBCode转义/未知语言回退文案不符合预期：zhs={zhsTitle}/{zhsBody} | zht={zhtTitle}/{zhtBody} | eng={engTitle}/{engBody}");
        }
        finally
        {
            STS2SkinChanger.Ui.MixedCardArtDirectoryNotice.ResetForTests();
            setCatalog.Invoke(null, [previousCatalog]);
            setConfig.Invoke(null, [previousConfig]);
            cardLookupField.SetValue(null, previousLookupCache);
            try { Directory.Delete(root.FullName, recursive: true); }
            catch (IOException) { }
        }
    }

    private sealed class DisabledFilterSilentPool : CardPoolModel
    {
        public override string Title => "silent";
        public override string EnergyColorName => "silent";
        public override string CardFrameMaterialPath => "card_frame_green";
        public override Color DeckEntryCardColor => Colors.White;
        public override bool IsColorless => false;
        protected override CardModel[] GenerateAllCards() => [];
    }

    private sealed class DisabledFilterAbrasiveCard : CardModel
    {
        public DisabledFilterAbrasiveCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }
        public override string PortraitPath => "res://images/packed/card_portraits/silent/abrasive.png";
    }

    private static void CheckFormatRules()
    {
        var root = Path.Combine(Path.GetTempPath(), "sts2-card-art-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var rendered = MakePng(1000, 760, withAlpha: true);
            var master = MakePng(1212, 1704, withAlpha: false);
            var tiny = MakePng(600, 456, withAlpha: true);

            // Two override objects for the same card: the second must replace the first, and the
            // uncropped master must win over the pre-rendered art window crop, which cannot fill
            // a whole card.
            var first = Entry(
                "res://images/packed/card_portraits/silent/abrasive.png", "full_art", tiny, null);
            var second = Entry(
                "res://images/packed/card_portraits/silent/abrasive.png", "full_art", rendered, master);

            var bundle = string.Concat(
                "{\"format\":\"card_art_bundle\",\"version\":1,\"count\":2,\"overrides\":[",
                first,
                ",",
                second,
                "]}");
            Require(CardArtPackScanPolicy.SourcePathRegex().Matches(bundle).Count == 2,
                "the sample fixture must contain exactly two entries.");
            Require(bundle.Contains(Convert.ToBase64String(rendered), StringComparison.Ordinal) &&
                    bundle.Contains(Convert.ToBase64String(tiny), StringComparison.Ordinal),
                "the sample fixture must carry both entry payloads.");

            File.WriteAllText(
                Path.Combine(root, "Sample_Pack.cardartpack.json"),
                bundle,
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(root, "Sample_Pack.json.json"),
                "{\"id\":\"Sample_Pack\",\"name\":\"Sample Pack\",\"version\":\"1.0.0\"}",
                new UTF8Encoding(false));

            var packs = CardArtPackScanner.Scan([root]).Packs;
            Require(packs.Count == 1, $"one source expected for the sample pack, got {packs.Count}.");
            var pack = packs[0];
            Require(pack.Id == "Sample_Pack", $"declared id must be used, got {pack.Id}.");
            Require(pack.Name == "Sample Pack", $"declared name must be used, got {pack.Name}.");
            Require(pack.Cards.Count == 1,
                $"repeated entries for one card must collapse, got {pack.Cards.Count}: " +
                string.Join(", ", pack.Cards.Select(card => $"{card.CardId}/{card.Bytes.Length}")));

            var abrasive = pack.Cards.Single(card => card.CardId == "abrasive");
            Require(abrasive.IsFullArt, "display_mode=full_art must be recognised as full art.");
            Require(abrasive.SourcePath.StartsWith("res://", StringComparison.Ordinal) &&
                    abrasive.Bytes.Length > 0 &&
                    abrasive.Bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature),
                "the card must keep its resource path and a decoded PNG payload.");
            Require(abrasive.UsesEditSource && ReadPngSize(abrasive.Bytes) == (1212, 1704),
                $"the uncropped master must be displayed, got {ReadPngSize(abrasive.Bytes)}.");

            // A card-shaped image covers the whole card through the tall portrait canvas.
            Require(CardArtPackScanPolicy.BuildCardPresentation(abrasive) is
                    { UseExpandedPortraitLayout: true, PortraitVisible: true, UseFullFrameArt: false },
                "a card-shaped image must render through the tall portrait canvas.");

            // The art window's wide crop must never be stretched over a whole card: that is what
            // magnified the artwork and cropped its sides.
            var windowCrop = abrasive with
            {
                Bytes = rendered,
                UsesEditSource = false,
                DisplayMode = "full_art"
            };
            Require(!windowCrop.IsCardShaped,
                "a 1000x760 art window crop must not count as card shaped.");
            Require(CardArtPackScanPolicy.BuildCardPresentation(windowCrop) == null,
                "an art window crop must stay in the game's framed art window.");
            Require(ReadPngSize(windowCrop.Bytes) == (1000, 760),
                "the framed layout fixture must stay at the art window's own size.");

            // Entries without usable art must be dropped instead of registering a broken card.
            File.WriteAllText(
                Path.Combine(root, "Broken_Pack.cardartpack.json"),
                "{\"format\":\"card_art_bundle\",\"version\":1,\"overrides\":[" +
                Entry("res://images/packed/card_portraits/silent/accelerant.png", "full_art", rendered, null)
                    .Replace(Convert.ToBase64String(rendered), "not-valid-base64!!") + "," +
                "{\"display_mode\":\"full_art\",\"png_base64\":\"\"}]}",
                new UTF8Encoding(false));
            // A bundle whose entries all fail validation must be skipped entirely rather than
            // registering a card with no usable artwork.
            var broken = CardArtPackScanner.Scan([root]).Packs
                .SingleOrDefault(candidate => candidate.Path.EndsWith(
                    "Broken_Pack.cardartpack.json",
                    StringComparison.OrdinalIgnoreCase));
            Require(broken == null,
                $"a bundle without usable art must be skipped, kept {broken?.Cards.Count} cards.");

            // A file that is not a bundle must not be treated as one, so only the sample pack
            // remains a source at this point.
            File.WriteAllText(
                Path.Combine(root, "unrelated.json"),
                "{\"packs\":{}}",
                new UTF8Encoding(false));
            Require(CardArtPackScanner.Scan([root]).Packs.Count == 1,
                "a JSON file that is not a card_art_bundle must not be read as a pack.");

            // A second bundle beside the first stays its own source, and nested scan roots must
            // not read the same pack twice.
            File.WriteAllText(
                Path.Combine(root, "Sample_Pack_Lowres.cardartpack.json"),
                "{\"format\":\"card_art_bundle\",\"version\":1,\"overrides\":[" +
                Entry("res://images/packed/card_portraits/silent/abrasive.png", "full_art", tiny, null) +
                "]}",
                new UTF8Encoding(false));
            // Renditions of the same pack collapse into one source, and the fuller one wins.
            var widened = CardArtPackScanner.Scan([root, Directory.GetParent(root)!.FullName]).Packs;
            Require(widened.Count == 1,
                $"renditions of one pack must collapse into a single source, got {widened.Count}.");
            Require(widened[0].Path.EndsWith(
                    "Sample_Pack.cardartpack.json",
                    StringComparison.OrdinalIgnoreCase),
                $"the fuller rendition must win, kept {Path.GetFileName(widened[0].Path)}.");

            // Regression: each override stores its payloads *before* its own source_path, so
            // entries must be delimited by their braces. Reading between two paths instead would
            // give every card the artwork of the entry after it and drop the bundle's last card.
            var orderRoot = Path.Combine(root, "order");
            Directory.CreateDirectory(orderRoot);
            File.WriteAllText(
                Path.Combine(orderRoot, "Order_Pack.cardartpack.json"),
                "{\"format\":\"card_art_bundle\",\"version\":1,\"count\":2,\"overrides\":[" +
                Entry("res://images/packed/card_portraits/silent/abrasive.png", "full_art",
                    MakePng(1000, 760, withAlpha: true), MakePng(1212, 1704, withAlpha: false)) + "," +
                Entry("res://images/packed/card_portraits/silent/accelerant.png", "full_art",
                    MakePng(500, 380, withAlpha: true), MakePng(600, 844, withAlpha: false)) + "]}",
                new UTF8Encoding(false));

            var ordered = CardArtPackScanner.Scan([orderRoot]).Packs.Single().Cards;
            Require(ordered.Count == 2,
                $"both entries must be read, got {ordered.Count}: " +
                string.Join(", ", ordered.Select(card => card.CardId)));
            Require(ReadPngSize(ordered.Single(card => card.CardId == "abrasive").Bytes) == (1212, 1704),
                "a card must display the artwork stored in its own entry, not the next entry's.");
            Require(ReadPngSize(ordered.Single(card => card.CardId == "accelerant").Bytes) == (600, 844),
                "the last entry of a bundle must keep its own artwork.");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
        }
    }

    private static void CheckRealPacks()
    {
        const string workshop = @"D:\Programs\Steam\steamapps\workshop\content\2868840";
        if (!Directory.Exists(workshop))
        {
            Console.WriteLine("Skipping real pack check: no Workshop content folder on this machine.");
            return;
        }

        var packs = CardArtPackScanner.Scan([workshop]).Packs
            .Where(pack => pack.Cards.Count > 0)
            .ToArray();
        if (packs.Length == 0)
        {
            Console.WriteLine("Skipping real pack check: no installed card art pack found.");
            return;
        }

        var duplicateIds = packs
            .GroupBy(pack => pack.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        Require(duplicateIds.Length == 0,
            "a pack id must resolve to exactly one source: " + string.Join(", ", duplicateIds));

        foreach (var pack in packs)
        {
            Console.WriteLine(
                $"  installed pack {pack.Id} ({pack.Name}): {pack.Cards.Count} cards, " +
                $"full art={pack.Cards.Count(card => card.IsFullArt)}, " +
                $"card shaped={pack.Cards.Count(card => card.IsCardShaped)}, " +
                $"from master={pack.Cards.Count(card => card.UsesEditSource)}");
        }

        var silent = packs.FirstOrDefault(pack => pack.Cards.Any(card => card.CardId == "abrasive"));
        if (silent == null)
        {
            return;
        }

        // The whole-card canvas and the artwork must agree on the card's own aspect, otherwise
        // covering one with the other has to crop and magnify the picture.
        const double cardAspect = 300.0 / 422.0;
        var sample = silent.Cards.Single(card => card.CardId == "abrasive");
        Require(sample.UsesEditSource,
            "an installed pack must display the uncropped master instead of the art window crop.");
        Require(sample.IsCardShaped,
            "the displayed card art must be shaped like a card, not like the art window.");
        var (width, height) = ReadPngSize(sample.Bytes);
        var aspect = (double)width / height;
        Require(Math.Abs(aspect - cardAspect) < 0.02,
            $"the displayed card art must match the card's {cardAspect:F3} aspect, " +
            $"got {width}x{height} ({aspect:F3}).");
        Require(sample.Bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature),
            "the card image must be a PNG.");

        var duplicated = silent.Cards
            .GroupBy(card => card.CardId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        Require(duplicated.Length == 0,
            "a pack must not address the same card twice: " + string.Join(", ", duplicated));

        // One card per override: a bundle whose artwork shifted by one entry, or whose final entry
        // was dropped, cannot match the number of entries the bundle actually declares.
        var declared = CountDeclaredEntries(silent.Path);
        Require(silent.Cards.Count == declared,
            $"an installed pack must yield one card per entry: declared {declared}, " +
            $"read {silent.Cards.Count}.");
    }

    /// <summary>Counts a bundle's override entries without using the reader under test.</summary>
    private static int CountDeclaredEntries(string packPath)
    {
        const string marker = "\"source_path\"";
        var count = 0;
        using var reader = new StreamReader(packPath);
        var buffer = new char[1 << 20];
        var overlap = string.Empty;
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            var window = overlap + new string(buffer, 0, read);
            for (var index = window.IndexOf(marker, StringComparison.Ordinal);
                 index >= 0;
                 index = window.IndexOf(marker, index + marker.Length, StringComparison.Ordinal))
            {
                count++;
            }

            // A marker straddling two reads must still be counted exactly once.
            overlap = window[^Math.Min(marker.Length - 1, window.Length)..];
        }

        return count;
    }

    private static (int Width, int Height) ReadPngSize(byte[] bytes) =>
        (BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)),
         BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)));

    private static string Entry(string sourcePath, string displayMode, byte[]? payload, byte[]? editSource)
    {
        var builder = new StringBuilder();
        builder.Append("{\"adjust_offset_x\":0.0,\"adjust_offset_y\":0.0,\"adjust_zoom\":1.0,");
        builder.Append("\"display_mode\":\"").Append(displayMode).Append("\",");
        if (editSource != null)
        {
            builder.Append("\"edit_source_png_base64\":\"")
                .Append(Convert.ToBase64String(editSource))
                .Append("\",");
        }

        builder.Append("\"height\":760,");
        builder.Append("\"png_base64\":\"")
            .Append(payload == null ? string.Empty : Convert.ToBase64String(payload))
            .Append("\",");
        builder.Append("\"source_path\":\"").Append(sourcePath).Append("\",");
        builder.Append("\"type\":\"static\",\"updated_at\":\"2026-01-01T00:00:00\",\"width\":1000}");
        return builder.ToString();
    }

    /// <summary>Builds a real PNG so the reader exercises signature validation and IHDR parsing.</summary>
    internal static byte[] MakePng(int width, int height, bool withAlpha)
    {
        using var stream = new MemoryStream();
        stream.Write(PngSignature);
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4, 4), height);
        header[8] = 8;
        header[9] = withAlpha ? (byte)6 : (byte)2;
        WriteChunk(stream, "IHDR", header);

        // The reader only validates the signature and reads IHDR, so a stored deflate block keeps
        // this fixture small while still being a structurally valid PNG.
        var raw = new byte[(withAlpha ? 4 : 3) * width + 1];
        using var compressed = new MemoryStream();
        using (var deflate = new System.IO.Compression.ZLibStream(
                   compressed,
                   System.IO.Compression.CompressionLevel.NoCompression,
                   leaveOpen: true))
        {
            deflate.Write(raw);
        }

        WriteChunk(stream, "IDAT", compressed.ToArray());
        WriteChunk(stream, "IEND", []);
        return stream.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(Encoding.ASCII.GetBytes(type));
        stream.Write(data);
        // CRCs are never verified by the reader, so a placeholder keeps the fixture simple.
        stream.Write([0, 0, 0, 0]);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
