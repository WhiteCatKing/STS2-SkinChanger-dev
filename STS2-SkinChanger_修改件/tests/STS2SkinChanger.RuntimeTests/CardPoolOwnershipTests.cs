using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

// #11 / #217：卡池未被识别的 Mod 卡与借用其它角色占位 PortraitPath 的 Mod 卡（如久留美），
// 不得匹配其它角色的卡图包，也不得在旧配置残留时被覆盖原图。
internal static class CardPoolOwnershipTests
{
    internal static void Run()
    {
        // 游戏自带卡：仍按原有规则让框架按角色皮肤覆盖（不受这条限制）。
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: true, hasKnownCardPool: false),
            "原版卡不能被这条规则挡住。");
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: true, hasKnownCardPool: true),
            "原版卡不能被这条规则挡住。");

        // 已被切换器识别卡池的 Mod 卡：仍可被选中的卡图皮肤接管。
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: false, hasKnownCardPool: true),
            "已识别卡池的 Mod 卡仍然要能被选中的卡图皮肤接管。");

        // 卡池未被识别的 Mod 卡（久留美）：保留自己的原图。
        Require(SkinService.KeepsIntrinsicModCardArt(isGameCard: false, hasKnownCardPool: false),
            "卡池未被识别的 Mod 卡必须保留原图，不能被别的卡图包或框架角色卡面顶替。");

        CheckForeignPoolPortraitDetection();
        CheckNestedBorrowedPathAndSharedPredicate();
        CheckBorrowedIroncladPlaceholderIsolation();
        CheckSingleFlatPackMatchingTwoCharactersAtRuntime();
        CheckRealCatalogBuildRuntimeLookup();
        CheckPrivateBinaryPortraits();
        CheckStockPresentationFallback();

        Console.WriteLine("Card pool ownership policy passed.");
    }

    private static readonly List<string> PresentationLoadPaths = [];

    private static void CheckStockPresentationFallback()
    {
        const string legacy = "res://images/atlases/compressed.sprites/card_template/";
        const string current = "res://images/atlases/compressed_atlas.sprites/";
        var pairs = new[]
        {
            (legacy + "ancient_card_border.tres", current + "ancient_card_border.tres"),
            (legacy + "ancient_card_text_bg_attack.tres", current + "ancient_text_bg_attack.tres"),
            (legacy + "ancient_card_text_bg_skill.tres", current + "ancient_text_bg_skill.tres"),
            (legacy + "ancient_card_text_bg_power.tres", current + "ancient_text_bg_power.tres")
        };
        var harmony = new Harmony("Gurio.SkinChanger.Tests.StockCardPresentation");
        var log = AuditLogSilencer.Install();
        var root = Directory.CreateTempSubdirectory("sc-stock-card-");
        var writer = PckArchive.WriteOutput;
        var oldCatalog = SkinService.Catalog;
        var catalogProperty = AccessTools.Property(typeof(SkinService), "Catalog");
        try
        {
            PckArchive.WriteOutput = null;
            var load = typeof(ResourceLoader).GetMethods().Single(method =>
                method.Name == nameof(ResourceLoader.Load) && method.IsGenericMethodDefinition);
            harmony.Patch(load.MakeGenericMethod(typeof(Texture2D)),
                prefix: new HarmonyMethod(typeof(CardPoolOwnershipTests), nameof(CapturePresentationLoad)));

            foreach (var version in new[] { "legacy", "current", "absent" })
            {
                var game = Path.Combine(root.FullName, version + ".pck");
                var files = new Dictionary<string, byte[]> { ["res://fixture.bin"] = [0] };
                foreach (var (oldPath, newPath) in pairs)
                {
                    var path = version == "legacy" ? oldPath : newPath;
                    if (version != "absent") files[path] = "[gd_resource type=\"AtlasTexture\" format=3]\n[resource]\n"u8.ToArray();
                }
                PckArchive.Write(game, files);
                using var catalog = SkinCatalog.Build(game, []);
                catalogProperty.SetValue(null, catalog);
                var card = InitCard(new EchoForm(), new DefectCardPool(), "ECHO_FORM");
                PresentationLoadPaths.Clear();
                foreach (var (oldPath, newPath) in pairs)
                {
                    SkinService.LoadCardPresentationResource<Texture2D>(card, oldPath);
                    SkinService.LoadCardPresentationResource<Texture2D>(card, newPath);
                }
                var expected = version == "absent" ? [] : pairs
                    .SelectMany(pair => Enumerable.Repeat(version == "legacy" ? pair.Item1 : pair.Item2, 2))
                    .ToArray();
                Require(PresentationLoadPaths.SequenceEqual(expected),
                    "Moved Ancient textures must use the active game archive. A missing pair must never load NOPE.");
                const string authoredPath = "res://pack/private/card_overlay.png";
                Require(catalog.ResolveCardPresentationBaselinePath(authoredPath) == authoredPath,
                    "The compatibility lookup must leave authored resource paths alone.");
            }
        }
        finally
        {
            catalogProperty.SetValue(null, oldCatalog);
            PckArchive.WriteOutput = writer;
            harmony.UnpatchAll(harmony.Id);
            log.UnpatchAll(log.Id);
        }
    }

    private static bool CapturePresentationLoad(string path, ref Texture2D? __result)
    {
        PresentationLoadPaths.Add(path);
        __result = null;
        return false;
    }

    private static void CheckPrivateBinaryPortraits()
    {
        const string rebound = "res://images/atlases/card_atlas.sprites/event/rebound.tres";
        const string normalPng = "res://images/packed/card_portraits/event/Rebound.png";
        const string betaPng = "res://images/packed/card_portraits/event/beta/rebound.png";
        const string payload = "res://.godot/exported/rebound.res";
        const string providerTexture = "res://.godot/imported/rebound-skin.ctex";
        const string gameTexture = "res://.godot/imported/rebound-game.ctex";
        var root = Directory.CreateTempSubdirectory("sc-private-card-");
        var previousWriter = PckArchive.WriteOutput;
        var previousCatalog = SkinService.Catalog;
        var catalogProperty = AccessTools.Property(typeof(SkinService), "Catalog");
        try
        {
            PckArchive.WriteOutput = null;
            var game = Path.Combine(root.FullName, "game.pck");
            static byte[] Remap(string target) => System.Text.Encoding.UTF8.GetBytes($"[remap]\npath=\"{target}\"\n");
            PckArchive.Write(game, new Dictionary<string, byte[]>
            {
                [betaPng + ".import"] = Remap(gameTexture),
                [gameTexture] = TestTexture(10)
            });

            var options = new List<SkinModDescriptor>();
            foreach (var option in new[] { "ReboundA", "ReboundB" })
            {
                var pck = Path.Combine(root.FullName, option + ".pck");
                PckArchive.Write(pck, new Dictionary<string, byte[]>
                {
                    [rebound + ".remap"] = Remap(payload),
                    [payload] = MakeBinaryPortrait(normalPng.ToLowerInvariant(), compress: options.Count == 1),
                    [normalPng + ".import"] = Remap(providerTexture),
                    [providerTexture] = TestTexture((byte)option[^1])
                });
                options.Add(new(option, option, pck, false));
            }
            using var catalog = SkinCatalog.Build(game, options);
            catalog.FinalizeCardGroups([new("Rebound", rebound, "misc", "event", "event")]);

            foreach (var option in options)
            {
                var overlay = catalog.BuildIsolatedCardResource("misc", option.Id, rebound, true, option.Id);
                var aliases = overlay.ResourcePaths;
                var patched = CardBinaryResource.Parse(overlay.Files[overlay.PayloadAliases[payload]])!;
                Require(overlay.CanonicalDependencyPaths.Count == 0 &&
                        overlay.Files.Keys.All(path => path.StartsWith("res://sts2_skin_runtime/" + option.Id + "/")),
                    "Isolated card PCKs must never write to a canonical path.");
                Require(patched.Dependencies(rebound).Single() == aliases[normalPng] &&
                        aliases[normalPng].EndsWith("/Rebound.png", StringComparison.Ordinal) &&
                        overlay.Files[overlay.PayloadAliases[providerTexture]].SequenceEqual(TestTexture((byte)option.Id[^1])),
                    "Two skins of the same card must resolve their own dependency and retain its concrete import case.");
                var patchedBytes = overlay.Files[overlay.PayloadAliases[payload]];
                Require(System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(patchedBytes.AsSpan(patchedBytes.Length - 16)) ==
                        patchedBytes.Length - 8 && patchedBytes.AsSpan(patchedBytes.Length - 4).SequenceEqual("RSRC"u8),
                    "Growing a dependency path must preserve the body and move its internal offset.");
            }

            catalogProperty.SetValue(null, catalog);
            var card = InitCard(new Rebound(), new EventCardPool(), "REBOUND");
            var baseline = AccessTools.Method(typeof(SkinService), "ResolveGameCardPortraitFallback")
                .Invoke(null, [card, rebound]);
            Require((string?)baseline == betaPng && catalog.ResolveBaseline(rebound) == null,
                "Game default for a card with only a beta PNG must choose the game archive's own portrait.");
            var defaultOverlay = catalog.BuildIsolatedCardResource("misc", SkinCatalog.BaseOptionId, betaPng, false, "base");
            Require(defaultOverlay.CanonicalDependencyPaths.Count == 0 &&
                    defaultOverlay.Files[defaultOverlay.PayloadAliases[gameTexture]].SequenceEqual(TestTexture(10)),
                "Game default after the two skins must resolve the baseline texture.");
        }
        finally
        {
            catalogProperty.SetValue(null, previousCatalog);
            PckArchive.WriteOutput = previousWriter;
        }
    }

    private static byte[] TestTexture(byte marker)
    {
        var bytes = new byte[52];
        "GST2"u8.CopyTo(bytes);
        bytes[40] = bytes[42] = 8;
        bytes[51] = marker;
        return bytes;
    }

    private static byte[] MakeBinaryPortrait(string texturePath, bool compress)
    {
        using var buffer = new MemoryStream();
        using var writer = new BinaryWriter(buffer);
        static void WriteString(BinaryWriter writer, string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            writer.Write(bytes.Length + 1);
            writer.Write(bytes);
            writer.Write((byte)0);
        }
        if (!compress) writer.Write("RSRC"u8);
        foreach (var number in new[] { 0, 0, 4, 5, 6 }) writer.Write(number);
        WriteString(writer, "AtlasTexture");
        writer.Write(0L);
        writer.Write(2);
        writer.Write(1234L);
        for (var i = 0; i < 11; i++) writer.Write(0);
        writer.Write(0); // String table
        writer.Write(1);
        WriteString(writer, "Texture2D");
        WriteString(writer, texturePath);
        writer.Write(5678L);
        writer.Write(1);
        WriteString(writer, "local://Main");
        writer.Write(buffer.Position + sizeof(long));
        writer.Write(42);
        writer.Write("RSRC"u8);
        if (!compress) return buffer.ToArray();

        using var block = new MemoryStream();
        using (var deflate = new System.IO.Compression.ZLibStream(block, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
            deflate.Write(buffer.ToArray());
        using var container = new MemoryStream();
        using var packed = new BinaryWriter(container);
        packed.Write("RSCC"u8);
        packed.Write((int)Godot.FileAccess.CompressionMode.Deflate);
        packed.Write(4096);
        packed.Write((int)buffer.Length);
        packed.Write((int)block.Length);
        packed.Write(block.ToArray());
        packed.Write("RSCC"u8);
        return container.ToArray();
    }

    private static void CheckRealCatalogBuildRuntimeLookup()
    {
        const string ironcladStrike = "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
        const string silentNeutralize = "res://images/atlases/card_atlas.sprites/silent/neutralize.tres";
        const string silentPackNeutralize = "res://SilentPack/card_portraits/silent/neutralize.png";
        const string foolStrikeBaseline = "res://FoolMod/card_portraits/fool/strike_fool.png";
        const string foolPackStrike = "res://NailongFool/card_portraits/fool/strike_fool.png";

        var temp = Directory.CreateTempSubdirectory("sc-real-card-lookup-");
        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var configSetter = AccessTools.PropertySetter(typeof(SkinService), "Config");
        var lookupCacheField = AccessTools.Field(typeof(SkinService), "_cardLookupCache");
        var previousCache = lookupCacheField.GetValue(null);
        var resolveRequest = AccessTools.Method(typeof(SkinService), "ResolveCardPortraitRequest");
        var oldWriter = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            var gamePck = Path.Combine(temp.FullName, "game.pck");
            var ironcladPck = Path.Combine(temp.FullName, "CNRIroncladCards.pck");
            var silentPck = Path.Combine(temp.FullName, "SilentPack.pck");
            var foolPck = Path.Combine(temp.FullName, "NailongFool.pck");
            static byte[] Remap(string target) => System.Text.Encoding.UTF8.GetBytes($"[remap]\npath=\"{target}\"\n");
            static byte[] Import(string target) => System.Text.Encoding.UTF8.GetBytes($"[remap]\npath=\"{target}\"\n[deps]\nsource_file=\"res://src.png\"\ndest_files=[\"{target}\"]\n");
            byte[] baselineBytes = [1, 2, 3, 4];
            byte[] skinBytes = [9, 8, 7, 6];

            PckArchive.Write(gamePck, new Dictionary<string, byte[]>
            {
                [ironcladStrike + ".remap"] = Remap("res://.godot/exported/base_strike_ironclad.res"),
                ["res://.godot/exported/base_strike_ironclad.res"] = baselineBytes,
                [silentNeutralize + ".remap"] = Remap("res://.godot/exported/base_neutralize.res"),
                ["res://.godot/exported/base_neutralize.res"] = baselineBytes
            });
            PckArchive.Write(ironcladPck, new Dictionary<string, byte[]>
            {
                [ironcladStrike + ".remap"] = Remap("res://.godot/exported/cnr_strike_ironclad.res"),
                ["res://.godot/exported/cnr_strike_ironclad.res"] = skinBytes
            });
            PckArchive.Write(silentPck, new Dictionary<string, byte[]>
            {
                [silentPackNeutralize + ".import"] = Import("res://.godot/imported/neutralize.ctex"),
                ["res://.godot/imported/neutralize.ctex"] = skinBytes
            });
            PckArchive.Write(foolPck, new Dictionary<string, byte[]>
            {
                [foolPackStrike + ".import"] = Import("res://.godot/imported/strike_fool.ctex"),
                ["res://.godot/imported/strike_fool.ctex"] = skinBytes
            });

            using var catalog = SkinCatalog.Build(gamePck,
            [
                new SkinModDescriptor("CNRIroncladCards", "CNRIroncladCards", ironcladPck, false),
                new SkinModDescriptor("SilentPack", "SilentPack", silentPck, false),
                new SkinModDescriptor("NailongFool", "NailongFool", foolPck, false)
            ]);
            catalog.FinalizeCardGroups(
            [
                new CardCatalogEntry("StrikeIronclad", ironcladStrike, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
                new CardCatalogEntry("Neutralize", silentNeutralize, "silent", "silent", "silent") { IsCharacterPool = true },
                new CardCatalogEntry("StrikeFool", foolStrikeBaseline, "fool", "fool", "fool") { IsCharacterPool = true },
                new CardCatalogEntry("KurumiFirstCandle", ironcladStrike, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true }
            ]);

            catalogSetter.Invoke(null, [catalog]);
            lookupCacheField.SetValue(null, Activator.CreateInstance(lookupCacheField.FieldType));
            var config = AccessTools.Method(typeof(SkinConfig), "Deserialize").Invoke(null,
            ["""
            {
              "Selections": {
                "cards:ironclad": "CNRIroncladCards",
                "cards:silent": "SilentPack",
                "cards:fool": "NailongFool",
                "cards:kurumi": "CNRIroncladCards"
              },
              "CardSkinPriorities": {
                "ironclad": [{ "OptionId": "CNRIroncladCards", "Enabled": true }],
                "silent": [{ "OptionId": "SilentPack", "Enabled": true }],
                "fool": [{ "OptionId": "NailongFool", "Enabled": true }]
              }
            }
            """])!;
            configSetter.Invoke(null, [config]);

            var ironcladCard = InitCard(new StrikeIronclad(), new IroncladCardPool(), "STRIKE_IRONCLAD");
            var silentCard = InitCard(new Neutralize(), new SilentCardPool(), "NEUTRALIZE");
            var foolCard = InitCard(new StrikeFool(), new FoolTestPool(), "STRIKE_FOOL");
            var kurumiCard = InitCard(new KurumiFirstCandle(), new KurumiTestPool(), "KURUMI_FIRST_CANDLE");

            var ironcladReq = resolveRequest.Invoke(null, [ironcladCard]);
            var silentReq = resolveRequest.Invoke(null, [silentCard]);
            var foolReq = resolveRequest.Invoke(null, [foolCard]);
            var kurumiReq = resolveRequest.Invoke(null, [kurumiCard]);

            Require(ironcladReq != null &&
                    (string)AccessTools.Property(ironcladReq.GetType(), "Selection").GetValue(ironcladReq)! == "CNRIroncladCards" &&
                    (string)AccessTools.Property(ironcladReq.GetType(), "ResourcePath").GetValue(ironcladReq)! == ironcladStrike,
                "真实目录构建后，铁甲原版打击卡在运行时必须命中 CNRIroncladCards。");
            Require(silentReq != null &&
                    (string)AccessTools.Property(silentReq.GetType(), "Selection").GetValue(silentReq)! == "SilentPack" &&
                    (string)AccessTools.Property(silentReq.GetType(), "ResourcePath").GetValue(silentReq)! == silentPackNeutralize,
                "真实目录构建后，猎宝原版中和卡在运行时必须命中 SilentPack。");
            Require(foolReq != null &&
                    (string)AccessTools.Property(foolReq.GetType(), "Selection").GetValue(foolReq)! == "NailongFool" &&
                    (string)AccessTools.Property(foolReq.GetType(), "ResourcePath").GetValue(foolReq)! == foolPackStrike,
                "真实目录构建后，愚者打击卡在运行时必须命中 NailongFool。");
            Require(kurumiReq == null && !SkinService.HasCardSkin(kurumiCard),
                "真实目录构建后，久留美借用铁甲占位路径仍必须被隔离。");
        }
        finally
        {
            catalogSetter.Invoke(null, [previousCatalog]);
            configSetter.Invoke(null, [previousConfig]);
            lookupCacheField.SetValue(null, previousCache);
            PckArchive.WriteOutput = oldWriter;
            temp.Delete(recursive: true);
        }
    }

    private static void CheckNestedBorrowedPathAndSharedPredicate()
    {
        const string nestedIroncladStrike = "res://assets/card_portraits/default/ironclad/strike_ironclad.tres";
        const string flatStrike = "res://pack/cards/strike_ironclad.png";
        const string flatKurumi = "res://pack/cards/kurumi_first_candle.png";
        const string customAtlasKurumi = "res://pack/custom_card_atlas.sprites/kurumi/kurumi_first_candle.tres";

        // 即使 knownCardGroups 混入了外层包装目录 "default"，解析器也必须优先识别内建/已知卡池 "ironclad"。
        var pollutedKnownGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "default", "kurumi", "ironclad" };
        var knownPools = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "kurumi", "ironclad" };
        var parsedWithGroups = SkinCatalog.TryGetCardArtIdentity(nestedIroncladStrike, pollutedKnownGroups, knownPools);
        var parsedWithoutGroups = SkinCatalog.TryGetCardArtIdentity(nestedIroncladStrike);
        Require(parsedWithGroups is { Category: "ironclad", Stem: "strikeironclad" } &&
                parsedWithoutGroups is { Category: "ironclad", Stem: "strikeironclad" },
            "嵌套路径 default/ironclad/strike_ironclad.tres 必须稳定解析为 ironclad 分类，不能退回外层 default。");

        var kurumiNested = SkinCatalog.TryCreateCardMatchIdentity(
            "KurumiFirstCandle", nestedIroncladStrike, "kurumi", pollutedKnownGroups, knownPools);
        Require(kurumiNested is { HasForeignPortrait: true },
            "久留美卡使用嵌套铁甲占位路径必须判定为跨卡池占位路径。");
        var ironcladNested = SkinCatalog.TryCreateCardMatchIdentity(
            "StrikeIronclad", nestedIroncladStrike, "ironclad", pollutedKnownGroups, knownPools);
        Require(ironcladNested is { HasForeignPortrait: false },
            "铁甲卡使用嵌套铁甲路径属于本卡池，不应误判为跨卡池占位路径。");

        var flatStrikeId = SkinCatalog.TryGetCardArtIdentity(flatStrike, pollutedKnownGroups, knownPools)!.Value;
        var flatKurumiId = SkinCatalog.TryGetCardArtIdentity(flatKurumi, pollutedKnownGroups, knownPools)!.Value;
        var customAtlasId = SkinCatalog.TryGetCardArtIdentity(customAtlasKurumi, pollutedKnownGroups, knownPools)!.Value;
        Require(!SkinCatalog.CardArtMatches(flatStrikeId, kurumiNested!.Value),
            "久留美嵌套铁甲占位路径对平铺 strike_ironclad.png 必须返回 false。");
        Require(SkinCatalog.CardArtMatches(flatKurumiId, kurumiNested.Value),
            "久留美嵌套铁甲占位路径对按类名命中的平铺 kurumi_first_candle.png 必须返回 true。");
        Require(SkinCatalog.CardArtMatches(customAtlasId, kurumiNested.Value),
            "久留美卡对自定义 atlas 标记路径 custom_card_atlas.sprites/kurumi/kurumi_first_candle.tres 必须返回 true。");
        Require(SkinCatalog.CardArtMatches(flatStrikeId, ironcladNested!.Value),
            "铁甲卡使用嵌套铁甲路径对平铺 strike_ironclad.png 仍应返回 true。");

        CardCatalogEntry[] entries =
        [
            new("StrikeIronclad", nestedIroncladStrike, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
            new("KurumiFirstCandle", nestedIroncladStrike, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true }
        ];
        var catalog = NewCatalog(NewPckOption("FlatStrikeOnly", flatStrike));
        catalog.FinalizeCardGroups(entries);
        Require(!catalog.CardGroups.Any(group => group.Id == "kurumi") &&
                catalog.CardGroups.Any(group => group.Id == "ironclad"),
            "目录构建时，嵌套铁甲占位路径不得让平铺 strike_ironclad.png 混入 kurumi 分组。");
        Require(!catalog.CardArtMatches(flatStrike, "KurumiFirstCandle", nestedIroncladStrike, "kurumi") &&
                catalog.CardArtMatches(flatKurumi, "KurumiFirstCandle", nestedIroncladStrike, "kurumi") &&
                catalog.CardArtMatches(flatStrike, "StrikeIronclad", nestedIroncladStrike, "ironclad"),
            "SkinCatalog 实例级共用谓词必须与目录构建保持同一结果。");

        // 运行时 SkinService.CardArtMatches 与目录侧共用解析器和卡池上下文，必须返回完全一致的结果。
        var previousCatalog = SkinService.Catalog;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        try
        {
            catalogSetter.Invoke(null, [catalog]);
            var kurumiPool = new KurumiTestPool();
            var nestedKurumi = InitCard(new KurumiNestedCard(), kurumiPool, "KURUMI_NESTED_CARD");
            var nestedIronclad = InitCard(new IroncladNestedStrike(), new IroncladCardPool(), "IRONCLAD_NESTED_STRIKE");
            const string flatNestedKurumi = "res://pack/cards/kurumi_nested_card.png";
            const string customAtlasNestedKurumi = "res://pack/custom_card_atlas.sprites/kurumi/kurumi_nested_card.tres";

            Require(!SkinService.CardArtMatches(flatStrike, nestedKurumi),
                "运行时久留美嵌套铁甲占位路径对平铺 strike_ironclad.png 必须返回 false。");
            Require(SkinService.CardArtMatches(flatNestedKurumi, nestedKurumi),
                "运行时久留美嵌套铁甲占位路径对按类名命中的平铺卡图必须返回 true。");
            Require(SkinService.CardArtMatches(customAtlasNestedKurumi, nestedKurumi),
                "运行时久留美卡对自定义 atlas 标记路径必须返回 true。");
            Require(SkinService.CardArtMatches(flatStrike, nestedIronclad),
                "运行时铁甲嵌套路径对平铺 strike_ironclad.png 仍应返回 true。");

            // 暖路径 SelectProviderCardPath：单候选直接返回，多候选按升级等级选对应编号图。
            const string v1 = "res://pack/cards/ironclad/strike_ironclad1.png";
            const string v2 = "res://pack/cards/ironclad/strike_ironclad2.png";
            var upgradeCard = InitCard(new UpgradeableTestCard(), new IroncladCardPool(), "STRIKE_IRONCLAD");
            Require(SkinService.SelectProviderCardPath([v1], upgradeCard, nestedIroncladStrike) == v1,
                "单候选路径必须直接返回唯一候选。");
            upgradeCard.FakeUpgradeLevel = 0;
            Require(SkinService.SelectProviderCardPath([v2, v1], upgradeCard, nestedIroncladStrike) == v1,
                "未升级时多候选应优先选择编号 1 的卡图。");
            upgradeCard.FakeUpgradeLevel = 1;
            Require(SkinService.SelectProviderCardPath([v1, v2], upgradeCard, nestedIroncladStrike) == v2,
                "升级后多候选应随升级等级切换为编号 2 的卡图。");
        }
        finally
        {
            catalogSetter.Invoke(null, [previousCatalog]);
        }
    }

    private static void CheckSingleFlatPackMatchingTwoCharactersAtRuntime()
    {
        const string ironcladStrike = "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
        const string ironcladDefend = "res://images/atlases/card_atlas.sprites/ironclad/defend_ironclad.tres";
        const string flatStrike = "res://SharedFlatPack/cards/strike_ironclad.png";
        const string flatDefend = "res://SharedFlatPack/cards/defend_ironclad.png";
        const string flatKurumiFirst = "res://SharedFlatPack/cards/kurumi_first_candle.png";

        CardCatalogEntry[] entries =
        [
            new("StrikeIronclad", ironcladStrike, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
            new("DefendIronclad", ironcladDefend, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
            new("KurumiFirstCandle", ironcladStrike, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true },
            new("KurumiTryMachine", ironcladDefend, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true }
        ];

        var catalog = NewCatalog(NewPckOption("SharedFlatPack", flatStrike, flatDefend, flatKurumiFirst));
        catalog.FinalizeCardGroups(entries);
        var groups = catalog.CardGroups.ToDictionary(group => group.Id, StringComparer.OrdinalIgnoreCase);
        var hasIronclad = groups.TryGetValue("ironclad", out var ironcladGroup);
        var hasKurumi = groups.TryGetValue("kurumi", out var kurumiGroup);
        Require(hasIronclad && hasKurumi,
            "同一平铺包同时包含铁甲与久留美卡名时，应分别进入两个角色分组。");
        var ironcladOpt = ironcladGroup!.Options.Single(option => option.Id == "SharedFlatPack");
        var kurumiOpt = kurumiGroup!.Options.Single(option => option.Id == "SharedFlatPack");
        Require(ironcladOpt.Assets.ContainsKey(flatStrike) && ironcladOpt.Assets.ContainsKey(flatDefend) &&
                !ironcladOpt.Assets.ContainsKey(flatKurumiFirst),
            "铁甲分组下的平铺包只能携带铁甲命中的资产。");
        Require(kurumiOpt.Assets.ContainsKey(flatKurumiFirst) &&
                !kurumiOpt.Assets.ContainsKey(flatStrike) && !kurumiOpt.Assets.ContainsKey(flatDefend),
            "久留美分组下的平铺包只能携带按久留美卡类名命中的资产，不得混入铁甲占位图。");

        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var configSetter = AccessTools.PropertySetter(typeof(SkinService), "Config");
        var lookupCacheField = AccessTools.Field(typeof(SkinService), "_cardLookupCache");
        var previousCache = lookupCacheField.GetValue(null);
        var resolveRequest = AccessTools.Method(typeof(SkinService), "ResolveCardPortraitRequest");
        try
        {
            catalogSetter.Invoke(null, [catalog]);
            lookupCacheField.SetValue(null, Activator.CreateInstance(lookupCacheField.FieldType));
            var config = AccessTools.Method(typeof(SkinConfig), "Deserialize").Invoke(null,
            ["""
            {
              "Selections": {
                "cards:ironclad": "SharedFlatPack",
                "cards:kurumi": "SharedFlatPack"
              },
              "CardSkinPriorities": {
                "ironclad": [{ "OptionId": "SharedFlatPack", "Enabled": true }],
                "kurumi": [{ "OptionId": "SharedFlatPack", "Enabled": true }]
              }
            }
            """])!;
            configSetter.Invoke(null, [config]);

            var kurumiPool = new KurumiTestPool();
            var ironcladCard = InitCard(new StrikeIronclad(), new IroncladCardPool(), "STRIKE_IRONCLAD");
            var kurumiMatched = InitCard(new KurumiFirstCandle(), kurumiPool, "KURUMI_FIRST_CANDLE");
            var kurumiUnmatched = InitCard(new KurumiTryMachine(), kurumiPool, "KURUMI_TRY_MACHINE");

            var ironcladReq = resolveRequest.Invoke(null, [ironcladCard]);
            var kurumiMatchedReq = resolveRequest.Invoke(null, [kurumiMatched]);
            var kurumiUnmatchedReq = resolveRequest.Invoke(null, [kurumiUnmatched]);
            Require(ironcladReq != null &&
                    (string)AccessTools.Property(ironcladReq.GetType(), "ResourcePath").GetValue(ironcladReq)! == flatStrike,
                "运行时同一平铺包必须把铁甲打击路由到 strike_ironclad.png。");
            Require(kurumiMatchedReq != null &&
                    (string)AccessTools.Property(kurumiMatchedReq.GetType(), "ResourcePath").GetValue(kurumiMatchedReq)! == flatKurumiFirst,
                "运行时同一平铺包必须把久留美命中卡路由到 kurumi_first_candle.png。");
            Require(kurumiUnmatchedReq == null,
                "运行时同一平铺包不得用 defend_ironclad.png 覆盖借用防御占位路径的久留美未命中卡。");
        }
        finally
        {
            catalogSetter.Invoke(null, [previousCatalog]);
            configSetter.Invoke(null, [previousConfig]);
            lookupCacheField.SetValue(null, previousCache);
        }
    }

    private static void CheckForeignPoolPortraitDetection()
    {
        var knownPools = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ironclad", "kurumi", "fool" };
        Require(!SkinCatalog.IsForeignCardPoolPortrait("ironclad", "ironclad", knownPools),
            "本卡池路径不应判定为跨卡池占位路径。");
        Require(!SkinCatalog.IsForeignCardPoolPortrait("", "kurumi", knownPools),
            "无分类目录的路径不应判定为跨卡池占位路径。");
        Require(!SkinCatalog.IsForeignCardPoolPortrait("custom_art", "kurumi", knownPools),
            "Mod 自己的私有子目录不应判定为跨卡池占位路径。");
        Require(SkinCatalog.IsForeignCardPoolPortrait("ironclad", "kurumi", knownPools),
            "久留美卡借用 ironclad 路径必须判定为跨卡池占位路径。");
        Require(SkinCatalog.IsForeignCardPoolPortrait("ironclad", "kurumi", null),
            "即使未传入 knownPoolGroups，借用原版内建卡池目录也必须判定为跨卡池占位路径。");
        Require(SkinCatalog.IsForeignCardPoolPortrait("fool", "kurumi", knownPools),
            "借用其他 Mod 角色卡池目录也必须判定为跨卡池占位路径。");
    }

    private static void CheckBorrowedIroncladPlaceholderIsolation()
    {
        const string ironcladStrike = "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
        const string ironcladDefend = "res://images/atlases/card_atlas.sprites/ironclad/defend_ironclad.tres";
        const string ironcladBreak = "res://images/atlases/card_atlas.sprites/ironclad/break.tres";
        const string flatStrike = "res://FlatPack/cards/strike_ironclad.png";
        const string kurumiOwnArt = "res://KurumiCustomCards/card_portraits/kurumi/kurumi_first_candle.png";

        CardCatalogEntry[] entries =
        [
            new("StrikeIronclad", ironcladStrike, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
            new("DefendIronclad", ironcladDefend, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true },
            new("Break", ironcladBreak, "ironclad", "ironclad", "ancients") { IsCharacterPool = true },
            new("KurumiFirstCandle", ironcladStrike, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true },
            new("KurumiTryMachine", ironcladDefend, "kurumi", "kurumi", "kurumi") { IsCharacterPool = true },
            new("KurumiReceiptStrike", ironcladStrike, "kurumi", "kurumi", "ancients") { IsCharacterPool = true }
        ];

        var resolvedStem = SkinCatalog.ResolveCardForPortraitStem("strike_ironclad", entries);
        Require(resolvedStem != null && resolvedStem.TypeName == "StrikeIronclad",
            "借用铁甲占位路径的 Mod 卡不得污染 strike_ironclad 的唯一定位。");

        var catalog = NewCatalog(
            NewPckOption("CNRIroncladCards", ironcladStrike, ironcladDefend, ironcladBreak),
            NewPckOption("FlatIroncladCards", flatStrike));
        catalog.FinalizeCardGroups(entries);

        var groups = catalog.CardGroups.ToDictionary(group => group.Id, StringComparer.OrdinalIgnoreCase);
        Require(!groups.ContainsKey("kurumi"),
            "铁甲卡图包与无目录同名占位包都不应生成 kurumi 卡牌分组。");
        Require(groups.TryGetValue("ironclad", out var ironcladGroup) &&
                ironcladGroup.Options.Any(option => option.Id == "CNRIroncladCards"),
            "铁甲卡图包仍应正常归属 ironclad 卡牌分组。");
        Require(groups.TryGetValue("ancients", out var ancientsGroup),
            "铁甲先古牌仍应进入 ancients 分组。");
        var ancientCnr = ancientsGroup!.Options.Single(option => option.Id == "CNRIroncladCards");
        Require(ancientCnr.Assets.ContainsKey(ironcladBreak) &&
                !ancientCnr.Assets.ContainsKey(ironcladStrike) &&
                !ancientCnr.Assets.ContainsKey(ironcladDefend),
            "ancients/CNRIroncladCards 只应包含铁甲自己的先古牌，不能被久留美先古牌带入 strike/defend。");

        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var configSetter = AccessTools.PropertySetter(typeof(SkinService), "Config");
        var lookupCacheField = AccessTools.Field(typeof(SkinService), "_cardLookupCache");
        var previousCache = lookupCacheField.GetValue(null);
        var resolveRequest = AccessTools.Method(typeof(SkinService), "ResolveCardPortraitRequest");
        try
        {
            catalogSetter.Invoke(null, [catalog]);
            lookupCacheField.SetValue(null, Activator.CreateInstance(lookupCacheField.FieldType));
            // 模拟旧版 skin_changer.json 中残留的污染配置。
            var pollutedConfig = AccessTools.Method(typeof(SkinConfig), "Deserialize").Invoke(null,
            ["""
            {
              "Selections": {
                "cards:ironclad": "CNRIroncladCards",
                "cards:kurumi": "CNRIroncladCards",
                "cards:ancients": "CNRIroncladCards"
              },
              "CardSkinPriorities": {
                "kurumi": [
                  { "OptionId": "CNRIroncladCards", "Enabled": true }
                ],
                "ancients": [
                  { "OptionId": "CNRIroncladCards", "Enabled": true }
                ]
              }
            }
            """])!;
            configSetter.Invoke(null, [pollutedConfig]);

            var kurumiPool = new KurumiTestPool();
            var kurumiAttack = InitCard(new KurumiFirstCandle(), kurumiPool, "KURUMI_FIRST_CANDLE");
            var kurumiAncient = InitCard(new KurumiReceiptStrike(), kurumiPool, "KURUMI_RECEIPT_STRIKE");
            var ironcladCard = InitCard(new StrikeIronclad(), new IroncladCardPool(), "STRIKE_IRONCLAD");

            Require(!SkinService.HasKnownCardPool(kurumiAttack),
                "没有久留美专属卡包时，久留美卡不应被识别为已托管卡池。");
            Require(!SkinService.HasCardSkin(kurumiAttack) && !SkinService.HasCardSkin(kurumiAncient),
                "铁甲卡包不得为久留美普通卡或先古卡提供候选皮肤。");
            Require(resolveRequest.Invoke(null, [kurumiAttack]) == null &&
                    resolveRequest.Invoke(null, [kurumiAncient]) == null,
                "即使旧配置残留 cards:kurumi/ancients=CNRIroncladCards，也不得替换久留美卡面。");
            Require(resolveRequest.Invoke(null, [ironcladCard]) != null,
                "铁甲自己的打击卡仍必须正常命中 CNRIroncladCards。");

            // 真正属于 kurumi 卡池的卡图包仍可正常生效。
            var withRealKurumiPack = NewCatalog(
                NewPckOption("CNRIroncladCards", ironcladStrike, ironcladDefend, ironcladBreak),
                NewPckOption("KurumiCustomCards", kurumiOwnArt));
            withRealKurumiPack.FinalizeCardGroups(entries);
            var kurumiGroup = withRealKurumiPack.CardGroups.Single(group => group.Id == "kurumi");
            Require(kurumiGroup.Options.Count == 1 && kurumiGroup.Options[0].Id == "KurumiCustomCards",
                "久留美卡组应只接纳真正属于 kurumi 卡池的卡图包，排除铁甲包。");
        }
        finally
        {
            catalogSetter.Invoke(null, [previousCatalog]);
            configSetter.Invoke(null, [previousConfig]);
            lookupCacheField.SetValue(null, previousCache);
        }
    }

    private static SkinCatalog NewCatalog(params CardSkinOption[] pckOptions)
    {
        var catalog = (SkinCatalog)RuntimeHelpers.GetUninitializedObject(typeof(SkinCatalog));
        foreach (var name in new[] { "_cardGroups", "_configuredCardGroups", "_pckCardOptions", "_providerInstanceIdentities", "_baselineIndexes" })
        {
            var field = AccessTools.Field(typeof(SkinCatalog), name);
            field.SetValue(catalog, Activator.CreateInstance(typeof(List<>).MakeGenericType(field.FieldType.GenericTypeArguments[0])));
        }

        var options = (List<CardSkinOption>)AccessTools.Field(typeof(SkinCatalog), "_pckCardOptions").GetValue(catalog)!;
        options.AddRange(pckOptions);
        return catalog;
    }

    private static CardSkinOption NewPckOption(string id, params string[] assetPaths)
    {
        var assets = assetPaths.ToDictionary(
            path => path,
            path => new ResourceAsset(path),
            StringComparer.OrdinalIgnoreCase);
        return new CardSkinOption(
            id,
            id,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase),
            PckAssets: assets,
            ProviderId: id);
    }

    private static T InitCard<T>(T card, CardPoolModel pool, string id) where T : CardModel
    {
        AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField").SetValue(card, new ModelId("CARD", id));
        AccessTools.Field(typeof(CardModel), "_pool").SetValue(card, pool);
        return card;
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    private sealed class KurumiTestPool : CardPoolModel
    {
        public override string Title => "kurumi";
        public override string EnergyColorName => "ironclad";
        public override string CardFrameMaterialPath => "card_frame_red";
        public override Color DeckEntryCardColor => Colors.White;
        public override bool IsColorless => false;
        protected override CardModel[] GenerateAllCards() => [];
    }

    private sealed class KurumiFirstCandle : CardModel
    {
        public KurumiFirstCandle() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
        public override string PortraitPath => "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
    }

    private sealed class KurumiTryMachine : CardModel
    {
        public KurumiTryMachine() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }
        public override string PortraitPath => "res://images/atlases/card_atlas.sprites/ironclad/defend_ironclad.tres";
    }

    private sealed class KurumiReceiptStrike : CardModel
    {
        public KurumiReceiptStrike() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy) { }
        public override string PortraitPath => "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
    }

    private sealed class KurumiNestedCard : CardModel
    {
        public KurumiNestedCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
        public override string PortraitPath => "res://assets/card_portraits/default/ironclad/strike_ironclad.tres";
    }

    private sealed class IroncladNestedStrike : CardModel
    {
        public IroncladNestedStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
        public override string PortraitPath => "res://assets/card_portraits/default/ironclad/strike_ironclad.tres";
    }

    private sealed class UpgradeableTestCard : CardModel
    {
        public UpgradeableTestCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
        public int FakeUpgradeLevel { get; set; }
        public override string PortraitPath => "res://assets/card_portraits/default/ironclad/strike_ironclad.tres";
    }

    private sealed class FoolTestPool : CardPoolModel
    {
        public override string Title => "fool";
        public override string EnergyColorName => "ironclad";
        public override string CardFrameMaterialPath => "card_frame_red";
        public override Color DeckEntryCardColor => Colors.White;
        public override bool IsColorless => false;
        protected override CardModel[] GenerateAllCards() => [];
    }

    private sealed class StrikeFool : CardModel
    {
        public StrikeFool() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }
        public override string PortraitPath => "res://FoolMod/card_portraits/fool/strike_fool.png";
    }
}
