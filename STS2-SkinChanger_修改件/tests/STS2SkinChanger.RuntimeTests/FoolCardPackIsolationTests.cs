using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class FoolCardPackIsolationTests
{
    private const string SharedAtlasImport = "res://images/atlases/card_atlas_0.png.import";
    private const string SharedAtlasPayload = "res://.godot/imported/card_atlas_0.png-testgamehash.bptc.ctex";
    private const string VanillaCardImport = "res://images/packed/card_portraits/fool/beta/all_is_well.png.import";
    private const string VanillaCardPayload = "res://.godot/imported/all_is_well.png-testgamehash.bptc.ctex";
    private const string FoolStrikeSprite = "res://images/atlases/card_atlas.sprites/fool/strike_fool.tres";
    private const string FoolDefendSprite = "res://images/atlases/card_atlas.sprites/fool/defend_fool.tres";
    private const string FoolAllIsWellBeta = "res://images/packed/card_portraits/fool/beta/all_is_well.png";
    private const string IroncladStrikeSprite = "res://images/atlases/card_atlas.sprites/ironclad/strike_ironclad.tres";
    private const string RealSharedCardAtlasPayload = "res://.godot/imported/card_atlas_0.png-6b947739e98d17beac01a23ccc9a95cd.bptc.ctex";
    private const string RealFoolAllIsWellPayload = "res://.godot/imported/all_is_well.png-73227dfce7ac1224cd4a4dd57fbfec57.bptc.ctex";

    private static readonly string TinyPngBase64 = Convert.ToBase64String(
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x44, 0x41,
        0x54, 0x78, 0x9C, 0x63, 0xF8, 0xCF, 0xC0, 0xF0,
        0x1F, 0x00, 0x05, 0x00, 0x01, 0xFF, 0x89, 0x99,
        0x3D, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45,
        0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ]);

    private static string _cacheDir = null!;

    internal static void Run()
    {
        CheckSyntheticSidecarCardArtPackAndSharedPayloadProtection();
        CheckRealFoolCardPacksIfPresent();
        Console.WriteLine("Fool card pack isolation passed: sidecar .cardartpack.json isolation, shared game payload protection and A/B/default switching.");
    }

    private static void CheckSyntheticSidecarCardArtPackAndSharedPayloadProtection()
    {
        var log = AuditLogSilencer.Install();
        var root = Directory.CreateTempSubdirectory("sc-fool-cardpack-synth-");
        var previousWriter = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            var gamePck = Path.Combine(root.FullName, "game.pck");
            var modRoot = Directory.CreateDirectory(Path.Combine(root.FullName, "sidecar_cardpack")).FullName;
            var modPck = Path.Combine(modRoot, "sidecar_cardpack.pck");
            var sidecarPack = Path.Combine(modRoot, "sidecar_cardpack.cardartpack.json");
            const string privateAsset = "res://sidecar_cardpack/art/keep_marker.txt";

            static byte[] Import(string payload) => Encoding.UTF8.GetBytes($"[remap]\npath=\"{payload}\"\n");
            static byte[] AtlasTres(string payload) => Encoding.UTF8.GetBytes(
                $"[gd_resource type=\"AtlasTexture\" load_steps=2 format=3]\n[ext_resource type=\"Texture2D\" path=\"{payload}\" id=\"1\"]\n[resource]\natlas = ExtResource(\"1\")\n");

            PckArchive.Write(gamePck, new Dictionary<string, byte[]>
            {
                [SharedAtlasImport] = Import(SharedAtlasPayload),
                [SharedAtlasPayload] = FakeTexture(1),
                [VanillaCardImport] = Import(VanillaCardPayload),
                [VanillaCardPayload] = FakeTexture(2),
                [FoolStrikeSprite] = AtlasTres(SharedAtlasPayload),
                [IroncladStrikeSprite] = AtlasTres(SharedAtlasPayload)
            });

            // PCK intentionally contains NO res://images/packed/card_portraits/ or card_atlas.sprites/ entries —
            // only colliding .godot/imported/* payloads plus a private file, paired with a sidecar *.cardartpack.json.
            PckArchive.Write(modPck, new Dictionary<string, byte[]>
            {
                [SharedAtlasPayload] = FakeTexture(9),
                [VanillaCardPayload] = FakeTexture(8),
                [privateAsset] = [7, 7, 7]
            });

            File.WriteAllText(sidecarPack, $$"""
            {
              "card_art_bundle": 1,
              "generated_by": "sidecar_cardpack",
              "overrides": [
                {
                  "source_path": "res://sidecar_cardpack/art/all_is_well.png",
                  "png_base64": "{{TinyPngBase64}}",
                  "display_mode": "original",
                  "type": "static"
                }
              ]
            }
            """);

            var descriptor = new SkinModDescriptor(
                "sidecar_cardpack",
                "sidecar_cardpack",
                modPck,
                AffectsGameplay: false,
                RootPath: modRoot,
                HasDll: false);

            var probes = SkinCatalog.ProbeSkinProviders([descriptor], gamePck);
            Require(probes.Count == 1 && probes[0].CardAssetCount > 0 && probes[0].HasResourceBackedCosmetics,
                "纯资产卡图包带同目录 *.cardartpack.json 时必须被 ProbeSkinProviders 识别并隔离其 .pck。");

            using var catalog = SkinCatalog.Build(gamePck, [descriptor]);
            Require(catalog.ProtectSharedGameImportedPayloadFile(SharedAtlasPayload),
                "共享卡牌图集 card_atlas_0 的 .godot/imported 载荷必须受全局保护。");
            Require(catalog.ProtectSharedGameImportedPayloadFile(VanillaCardPayload),
                "原版卡牌 portrait 的 .godot/imported 载荷必须受全局保护。");
            Require(catalog.ProtectGlobalCanonicalPackFile(SharedAtlasPayload) &&
                    catalog.ProtectGlobalCanonicalPackFile(VanillaCardPayload) &&
                    !catalog.ProtectGlobalCanonicalPackFile(privateAsset),
                "全局保护必须拦截共享游戏导入载荷并保留提供者私有文件。");

            var safePackDir = Directory.CreateDirectory(Path.Combine(root.FullName, "safe-cache")).FullName;
            WithSafePack(catalog, modPck, safePackDir, filtered =>
            {
                Require(!filtered.Contains(SharedAtlasPayload) &&
                        !filtered.Contains(VanillaCardPayload) &&
                        filtered.Contains(privateAsset),
                    "PrepareSelectPortraitSafePack 必须剔除共享游戏卡图/图集导入载荷并保留私有资源。");
            });

            var scan = CardArtPackScanner.Scan([modRoot]);
            Require(scan.Packs.Count == 1, "合成 sidecar *.cardartpack.json 应被扫描器正常识别。");
            CardCatalogEntry[] entries =
            [
                new("AllIsWell", FoolAllIsWellBeta, "fool", "fool", "fool") { IsCharacterPool = true },
                new("StrikeIronclad", IroncladStrikeSprite, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true }
            ];
            catalog.AttachCardArtPacks(scan.Packs, entries);
            catalog.FinalizeCardGroups(entries);

            var overlayDefault = catalog.BuildCardOverlay(
                new Dictionary<string, string>(),
                new Dictionary<string, IReadOnlyList<string>>());
            Require(!overlayDefault.Values.Any(file =>
                        file.ArchivePath.Length > 0 &&
                        Path.GetFullPath(file.ArchivePath) == Path.GetFullPath(modPck)),
                "游戏默认状态下 BuildCardOverlay 不得发布任何来自隔离卡图包 PCK 的文件。");

            var overlaySelected = catalog.BuildCardOverlay(
                new Dictionary<string, string> { ["cards:fool"] = "sidecar_cardpack" },
                new Dictionary<string, IReadOnlyList<string>> { ["fool"] = ["sidecar_cardpack"] });
            Require(!overlaySelected.ContainsKey(SharedAtlasPayload) &&
                    !overlaySelected.ContainsKey(VanillaCardPayload),
                "选中带共享载荷的卡图包时，BuildCardOverlay 不得将共享 .godot/imported 载荷发布到全局路径。");
        }
        finally
        {
            PckArchive.WriteOutput = previousWriter;
            log.UnpatchAll(log.Id);
            try { root.Delete(recursive: true); } catch { }
        }
    }

    private static void CheckRealFoolCardPacksIfPresent()
    {
        var betaGamePck = @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.pck";
        if (!File.Exists(betaGamePck))
        {
            return;
        }

        var (beautyRoot, cuteRoot) = FindRealFoolPackRoots();
        if (beautyRoot == null || cuteRoot == null)
        {
            return;
        }

        var beautyPck = Path.Combine(beautyRoot, "fool_card_beauty.pck");
        var cutePck = Path.Combine(cuteRoot, "CuteFool-Cards.pck");
        if (!File.Exists(beautyPck) || !File.Exists(cutePck))
        {
            return;
        }

        var log = AuditLogSilencer.Install();
        var tempRoot = Directory.CreateTempSubdirectory("sc-fool-cardpack-real-");
        var previousWriter = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            var beautyDescriptor = new SkinModDescriptor(
                "fool_card_beauty",
                "愚者卡面美化",
                beautyPck,
                AffectsGameplay: false,
                RootPath: beautyRoot,
                HasDll: false);
            var cuteDescriptor = new SkinModDescriptor(
                "CuteFool-Cards",
                "CuteFool-Cards",
                cutePck,
                AffectsGameplay: false,
                RootPath: cuteRoot,
                HasDll: false);

            var probes = SkinCatalog.ProbeSkinProviders([beautyDescriptor, cuteDescriptor], betaGamePck);
            Require(probes.Count == 2 &&
                    probes.Any(probe => probe.Id.Equals("fool_card_beauty", StringComparison.OrdinalIgnoreCase) && probe.HasResourceBackedCosmetics) &&
                    probes.Any(probe => probe.Id.Equals("CuteFool-Cards", StringComparison.OrdinalIgnoreCase) && probe.HasResourceBackedCosmetics),
                "真实 fool_card_beauty 与 CuteFool-Cards 都必须被 ProbeSkinProviders 识别并隔离其 .pck。");

            using var catalog = SkinCatalog.Build(betaGamePck, [beautyDescriptor, cuteDescriptor]);
            Require(catalog.ProtectSharedGameImportedPayloadFile(RealSharedCardAtlasPayload),
                "真实 Beta 游戏包中的 card_atlas_0 导入载荷必须命中全局保护。");
            Require(catalog.ProtectSharedGameImportedPayloadFile(RealFoolAllIsWellPayload),
                "真实 Beta 游戏包中的愚者卡牌导入载荷必须命中全局保护。");

            var safeCache = Directory.CreateDirectory(Path.Combine(tempRoot.FullName, "safe-cache")).FullName;
            WithSafePack(catalog, beautyPck, safeCache, filtered =>
            {
                Require(!filtered.Contains(RealSharedCardAtlasPayload) &&
                        !filtered.Contains(RealFoolAllIsWellPayload) &&
                        filtered.Paths.All(path => !catalog.ProtectGlobalCanonicalPackFile(path)),
                    "真实 fool_card_beauty.pck 经安全过滤后不得残留任何受保护的共享图集或原版卡图导入载荷。");
                Require(filtered.Contains("res://fool_card_beauty/art/all_is_well.png"),
                    "真实 fool_card_beauty.pck 经安全过滤后应保留其私有卡图文件。");
            });

            var scan = CardArtPackScanner.Scan([beautyRoot, cuteRoot]);
            Require(scan.Packs.Any(pack => pack.Id.Equals("fool_card_beauty", StringComparison.OrdinalIgnoreCase)),
                "真实 fool_card_beauty.cardartpack.json 必须被 CardArtPackScanner 正常扫描。");

            CardCatalogEntry[] entries =
            [
                new("StrikeFool", FoolStrikeSprite, "fool", "fool", "fool") { IsCharacterPool = true },
                new("DefendFool", FoolDefendSprite, "fool", "fool", "fool") { IsCharacterPool = true },
                new("AllIsWell", FoolAllIsWellBeta, "fool", "fool", "fool") { IsCharacterPool = true },
                new("StrikeIronclad", IroncladStrikeSprite, "ironclad", "ironclad", "ironclad") { IsCharacterPool = true }
            ];
            catalog.AttachCardArtPacks(scan.Packs, entries);
            catalog.FinalizeCardGroups(entries);

            var groups = catalog.CardGroups.ToDictionary(group => group.Id, StringComparer.OrdinalIgnoreCase);
            Require(groups.TryGetValue("fool", out var foolGroup),
                "真实愚者卡包必须注册到 fool 卡牌分组。");
            Require(foolGroup!.Options.Any(option => option.Id.Equals("fool_card_beauty", StringComparison.OrdinalIgnoreCase)) &&
                    foolGroup.Options.Any(option => option.Id.Equals("CuteFool-Cards", StringComparison.OrdinalIgnoreCase)),
                "fool 分组必须同时包含 fool_card_beauty 与 CuteFool-Cards 两个选项。");
            Require(!groups.TryGetValue("ironclad", out var ironcladGroup) ||
                    !ironcladGroup.Options.Any(option =>
                        option.Id.Equals("fool_card_beauty", StringComparison.OrdinalIgnoreCase) ||
                        option.Id.Equals("CuteFool-Cards", StringComparison.OrdinalIgnoreCase)),
                "两个愚者卡包均不得串入非愚者角色（ironclad）卡牌分组。");

            CheckRealRuntimeSwitching(catalog, betaGamePck, beautyPck, cutePck);
        }
        finally
        {
            PckArchive.WriteOutput = previousWriter;
            log.UnpatchAll(log.Id);
            try { tempRoot.Delete(recursive: true); } catch { }
        }
    }

    private static void CheckRealRuntimeSwitching(
        SkinCatalog catalog,
        string betaGamePck,
        string beautyPck,
        string cutePck)
    {
        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var configSetter = AccessTools.PropertySetter(typeof(SkinService), "Config");
        var lookupCacheField = AccessTools.Field(typeof(SkinService), "_cardLookupCache");
        var previousCache = lookupCacheField.GetValue(null);
        var resolveRequest = AccessTools.Method(typeof(SkinService), "ResolveCardPortraitRequest");
        var deserializeConfig = AccessTools.Method(typeof(SkinConfig), "Deserialize");

        var foolPool = new RealFoolTestPool();
        var strikeFool = InitCard(new StrikeFool(), foolPool, "STRIKE_FOOL");
        var defendFool = InitCard(new DefendFool(), foolPool, "DEFEND_FOOL");
        var allIsWell = InitCard(new AllIsWell(), foolPool, "ALL_IS_WELL");
        var strikeIronclad = InitCard(new StrikeIronclad(), new IroncladCardPool(), "STRIKE_IRONCLAD");

        void ApplyFoolSelection(string? optionId)
        {
            lookupCacheField.SetValue(null, Activator.CreateInstance(lookupCacheField.FieldType));
            var json = optionId == null
                ? """
                {
                  "Selections": {
                    "cards:fool": "__base__"
                  },
                  "CardSkinPriorities": {
                    "fool": [
                      { "OptionId": "fool_card_beauty", "Enabled": false },
                      { "OptionId": "CuteFool-Cards", "Enabled": false }
                    ]
                  }
                }
                """
                : $$"""
                {
                  "Selections": {
                    "cards:fool": "{{optionId}}"
                  },
                  "CardSkinPriorities": {
                    "fool": [
                      { "OptionId": "{{optionId}}", "Enabled": true },
                      { "OptionId": "{{(optionId == "fool_card_beauty" ? "CuteFool-Cards" : "fool_card_beauty")}}", "Enabled": false }
                    ]
                  }
                }
                """;
            configSetter.Invoke(null, [deserializeConfig.Invoke(null, [json])!]);
        }

        (string Selection, string ResourcePath, bool UseSelectedProvider)? Resolve(CardModel card)
        {
            var req = resolveRequest.Invoke(null, [card]);
            if (req == null)
            {
                return null;
            }

            var type = req.GetType();
            return (
                (string)AccessTools.Property(type, "Selection").GetValue(req)!,
                (string)AccessTools.Property(type, "ResourcePath").GetValue(req)!,
                (bool)AccessTools.Property(type, "UseSelectedProvider").GetValue(req)!);
        }

        void AssertVanillaDefault(string phase)
        {
            foreach (var card in new CardModel[] { strikeFool, defendFool, allIsWell })
            {
                var req = Resolve(card);
                Require(req != null &&
                        req.Value.Selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                        !req.Value.UseSelectedProvider,
                    $"{phase}：愚者卡 {card.GetType().Name} 在游戏默认下必须走 __base__ 原版回源，不得命中任何提供者。");
            }

            Require(Resolve(strikeIronclad) == null,
                $"{phase}：非愚者卡（铁甲打击）必须保持原版且不被愚者卡包接管。");

            var baseOverlay = catalog.BuildIsolatedCardResources(
                "fool", SkinCatalog.BaseOptionId, [FoolAllIsWellBeta], useSelectedProvider: false, $"fool-test/base-{phase}");
            var baseAliased = baseOverlay.ResourcePaths[FoolAllIsWellBeta];
            using var betaArchive = PckArchive.Open(betaGamePck);
            var vanillaAllIsWellBytes = betaArchive.ReadFile(RealFoolAllIsWellPayload);
            var baseContainsVanilla = baseOverlay.Files.Values.Any(bytes => bytes.SequenceEqual(vanillaAllIsWellBytes));
            Require(baseOverlay.CanonicalDependencyPaths.Count == 0 &&
                    baseOverlay.Files.Keys.All(path => path.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase)) &&
                    baseAliased.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase) &&
                    baseContainsVanilla,
                $"{phase}：游戏默认下愚者卡隔离基线必须完全来自游戏本体 SlayTheSpire2.pck，不得夹带提供者载荷。");
        }

        try
        {
            catalogSetter.Invoke(null, [catalog]);

            // 1. 游戏默认：愚者与非愚者卡牌全为原版，且全局覆盖不泄露隔离包载荷。
            ApplyFoolSelection(null);
            AssertVanillaDefault("初始默认");
            var defaultOverlay = catalog.BuildCardOverlay(
                new Dictionary<string, string>(),
                new Dictionary<string, IReadOnlyList<string>>());
            Require(!defaultOverlay.Values.Any(file =>
                        file.ArchivePath.Length > 0 &&
                        (Path.GetFullPath(file.ArchivePath) == Path.GetFullPath(beautyPck) ||
                         Path.GetFullPath(file.ArchivePath) == Path.GetFullPath(cutePck))),
                "游戏默认状态下 BuildCardOverlay 不得发布 fool_card_beauty 或 CuteFool-Cards 的任何载荷。");

            // 2. 选 A（fool_card_beauty）：只替换愚者卡牌，不改铁甲，不泄露共享图集。
            ApplyFoolSelection("fool_card_beauty");
            var beautyStrike = Resolve(strikeFool);
            var beautyDefend = Resolve(defendFool);
            var beautyAllIsWell = Resolve(allIsWell);
            Require(beautyStrike is { UseSelectedProvider: true, Selection: "fool_card_beauty" } &&
                    beautyDefend is { UseSelectedProvider: true, Selection: "fool_card_beauty" } &&
                    beautyAllIsWell is { UseSelectedProvider: true, Selection: "fool_card_beauty" },
                "选中 fool_card_beauty 时，愚者卡牌必须路由到 fool_card_beauty。");
            Require(Resolve(strikeIronclad) == null,
                "选中 fool_card_beauty 时，非愚者卡牌（铁甲打击）必须保持原版。");
            var beautyCardOverlay = catalog.BuildCardOverlay(
                new Dictionary<string, string> { ["cards:fool"] = "fool_card_beauty" },
                new Dictionary<string, IReadOnlyList<string>> { ["fool"] = ["fool_card_beauty"] });
            Require(!beautyCardOverlay.Any(pair =>
                        catalog.ProtectGlobalCanonicalPackFile(pair.Key) &&
                        pair.Value.ArchivePath.Length > 0 &&
                        Path.GetFullPath(pair.Value.ArchivePath) == Path.GetFullPath(beautyPck)),
                "选中 fool_card_beauty 时，BuildCardOverlay 不得将受保护的共享图集或原版卡图导入载荷发布到全局路径。");
            var beautyStrikePath = beautyStrike!.Value.ResourcePath;
            var beautyStrikeOverlay = catalog.BuildIsolatedCardResources(
                "fool", "fool_card_beauty", [beautyStrikePath], useSelectedProvider: true, "fool-test/a-strike");
            Require(beautyStrikeOverlay.CanonicalDependencyPaths.Count == 0 &&
                    beautyStrikeOverlay.Files.Keys.All(path => path.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase)) &&
                    !beautyStrikeOverlay.Files.ContainsKey(RealSharedCardAtlasPayload),
                "选中 fool_card_beauty 的 strike_fool 时，隔离加载应成功且不得把 card_atlas_0 发布到规范路径。");
            var beautyAllIsWellPath = beautyAllIsWell!.Value.ResourcePath;
            var beautyAliasOverlay = catalog.BuildIsolatedCardResources(
                "fool", "fool_card_beauty", [beautyAllIsWellPath], useSelectedProvider: true, "fool-test/a-001");
            Require(beautyAliasOverlay.CanonicalDependencyPaths.Count == 0 &&
                    beautyAliasOverlay.Files.Keys.All(path => path.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase)) &&
                    beautyAliasOverlay.ResourcePaths[beautyAllIsWellPath]
                        .StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase),
                "fool_card_beauty 卡面资源必须通过隔离别名路径加载且不得产生全局重定向。");
            var beautyBytes = ReadOverlayPayloadBytes(
                beautyAliasOverlay, beautyAliasOverlay.ResourcePaths[beautyAllIsWellPath]);

            // 3. 切 B（CuteFool-Cards）：愚者卡牌切到 B 且载荷与 A 独立，铁甲仍保持原版。
            ApplyFoolSelection("CuteFool-Cards");
            var cuteStrike = Resolve(strikeFool);
            var cuteDefend = Resolve(defendFool);
            var cuteAllIsWell = Resolve(allIsWell);
            Require(cuteStrike is { UseSelectedProvider: true, Selection: "CuteFool-Cards" } &&
                    cuteDefend is { UseSelectedProvider: true, Selection: "CuteFool-Cards" } &&
                    cuteAllIsWell is { UseSelectedProvider: true, Selection: "CuteFool-Cards" },
                "切换到 CuteFool-Cards 时，愚者卡牌必须全部切到 CuteFool-Cards。");
            Require(Resolve(strikeIronclad) == null,
                "切换到 CuteFool-Cards 时，非愚者卡牌（铁甲打击）必须保持原版。");
            var cuteAllIsWellPath = cuteAllIsWell!.Value.ResourcePath;
            var cuteAliasOverlay = catalog.BuildIsolatedCardResources(
                "fool", "CuteFool-Cards", [cuteAllIsWellPath], useSelectedProvider: true, "fool-test/b-001");
            Require(cuteAliasOverlay.CanonicalDependencyPaths.Count == 0 &&
                    cuteAliasOverlay.Files.Keys.All(path => path.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase)) &&
                    cuteAliasOverlay.ResourcePaths[cuteAllIsWellPath]
                        .StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase),
                "CuteFool-Cards 卡面资源必须通过隔离别名路径加载且不得产生全局重定向。");
            var cuteBytes = ReadOverlayPayloadBytes(
                cuteAliasOverlay, cuteAliasOverlay.ResourcePaths[cuteAllIsWellPath]);
            Require(!beautyBytes.SequenceEqual(cuteBytes),
                "fool_card_beauty 与 CuteFool-Cards 对同一愚者卡应产出互不相同的独立资源载荷，不得串图。");

            // 4. 切回默认：全部恢复原版。
            ApplyFoolSelection(null);
            AssertVanillaDefault("切回默认");

            // 5. 再切回 A（fool_card_beauty）：准确切回 A，不串 B 也不影响铁甲。
            ApplyFoolSelection("fool_card_beauty");
            Require(Resolve(strikeFool) == beautyStrike &&
                    Resolve(defendFool) == beautyDefend &&
                    Resolve(allIsWell) == beautyAllIsWell &&
                    Resolve(strikeIronclad) == null,
                "A -> B -> 默认 -> A 切换后必须准确恢复到 A 且无跨包残留。");
        }
        finally
        {
            catalogSetter.Invoke(null, [previousCatalog]);
            configSetter.Invoke(null, [previousConfig]);
            lookupCacheField.SetValue(null, previousCache);
        }
    }

    private static byte[] ReadOverlayPayloadBytes(RuntimeResourceOverlay overlay, string aliasedPath)
    {
        if (overlay.Files.TryGetValue(aliasedPath, out var direct))
        {
            return direct;
        }

        foreach (var suffix in new[] { ".remap", ".import" })
        {
            if (overlay.Files.TryGetValue(aliasedPath + suffix, out var remap))
            {
                return remap;
            }
        }

        throw new InvalidOperationException($"隔离覆盖中缺少 {aliasedPath} 的载荷。");
    }

    private static (string? BeautyRoot, string? CuteRoot) FindRealFoolPackRoots()
    {
        string[] candidateParents =
        [
            @"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods\_test_新增_愚者卡图",
            @"C:\Users\yuinn\Desktop\愚者卡面美化_v5.01等2项文件"
        ];
        foreach (var parent in candidateParents)
        {
            var beauty = Path.Combine(parent, "愚者卡面美化_v5.01");
            var cute = Path.Combine(parent, "CuteFool-Cards");
            if (Directory.Exists(beauty) && Directory.Exists(cute))
            {
                return (beauty, cute);
            }
        }

        return (null, null);
    }

    private static void WithSafePack(SkinCatalog catalog, string sourcePck, string cacheDir, Action<PckArchive> check)
    {
        var harmony = new Harmony("Gurio.SkinChanger.Tests.FoolCardPackSafePack");
        try
        {
            _cacheDir = cacheDir;
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), "CacheDirectory"),
                prefix: new HarmonyMethod(typeof(FoolCardPackIsolationTests), nameof(CacheDirPrefix)));
            harmony.Patch(AccessTools.Method(typeof(SkinChangerPaths), "WriteCache"),
                prefix: new HarmonyMethod(typeof(FoolCardPackIsolationTests), nameof(WriteCachePrefix)));
            var prepare = AccessTools.Method(typeof(SkinService), "PrepareSelectPortraitSafePack");
            var filteredPath = (string)prepare.Invoke(null, [catalog, sourcePck])!;
            Require(!string.Equals(filteredPath, sourcePck, StringComparison.OrdinalIgnoreCase),
                "包含受保护共享导入载荷的 PCK 必须生成过滤后的安全包，不能直接透传原包。");
            using var archive = PckArchive.Open(filteredPath);
            check(archive);
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static bool CacheDirPrefix(ref string __result)
    {
        __result = _cacheDir;
        return false;
    }

    private static bool WriteCachePrefix(Action write)
    {
        write();
        return false;
    }

    private static byte[] FakeTexture(byte marker)
    {
        var data = new byte[52];
        "GST2"u8.CopyTo(data);
        data[40] = data[42] = 8;
        data[51] = marker;
        return data;
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

    private sealed class RealFoolTestPool : CardPoolModel
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
        public override string PortraitPath => FoolStrikeSprite;
    }

    private sealed class DefendFool : CardModel
    {
        public DefendFool() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }
        public override string PortraitPath => FoolDefendSprite;
    }

    private sealed class AllIsWell : CardModel
    {
        public AllIsWell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }
        public override string PortraitPath => FoolAllIsWellBeta;
    }
}
