using System.Reflection;
using System.Text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui.LoadOrderWarning;

internal static class LoadOrderRiskNoticeTests
{
    internal static void Run()
    {
        CheckCardPatchRiskDetection();
        CheckSharedGameAssetRiskDetection();
        CheckExternalLoadOrderManagerDetection();
        CheckPromptPlanMatrix();
        CheckLocalizationCoverage();
        Console.WriteLine("Load order risk notice passed: card patch & shared asset risk detection, LoadOrderManager handling and 15-language texts.");
    }

    private static void CheckCardPatchRiskDetection()
    {
        var ncardEnterTree = AccessTools.Method(typeof(NCard), nameof(NCard._EnterTree)) ??
                             throw new InvalidOperationException("找不到 NCard._EnterTree。");
        var ncardUpdateVisuals = AccessTools.Method(typeof(NCard), "UpdateVisuals") ??
                                 throw new InvalidOperationException("找不到 NCard.UpdateVisuals。");
        var cardPortraitPath = AccessTools.PropertyGetter(typeof(CardModel), "PortraitPath") ??
                               throw new InvalidOperationException("找不到 CardModel.PortraitPath getter。");
        var unrelatedMethod = AccessTools.Method(typeof(LoadOrderRiskNoticeTests), nameof(UnrelatedTarget))!;

        Require(LoadOrderRiskDetector.IsRiskyCardPatchTarget(ncardEnterTree) &&
                LoadOrderRiskDetector.IsRiskyCardPatchTarget(ncardUpdateVisuals) &&
                LoadOrderRiskDetector.IsRiskyCardPatchTarget(cardPortraitPath) &&
                !LoadOrderRiskDetector.IsRiskyCardPatchTarget(unrelatedMethod),
            "危险补丁目标判定应命中 NCard 卡面方法与 CardModel 卡图 getter，不误判无关方法。");

        const string harmonyOwner = "Tests.RiskyCardMod";
        var harmony = new Harmony(harmonyOwner);
        try
        {
            harmony.Patch(
                ncardEnterTree,
                postfix: new HarmonyMethod(typeof(LoadOrderRiskNoticeTests), nameof(NoopNCardPostfix)));
            var patched = Harmony.GetAllPatchedMethods()
                .Select(target => (target, (Patches?)Harmony.GetPatchInfo(target)))
                .ToArray();

            Require(LoadOrderRiskDetector.HasCardPatchRisk(typeof(LoadOrderRiskNoticeTests).Assembly, "OtherId", patched),
                "按程序集应检测到 NCard._EnterTree 的危险补丁（原因 a）。");
            Require(LoadOrderRiskDetector.HasCardPatchRisk(null, harmonyOwner, patched),
                "按 Harmony owner 应检测到 NCard._EnterTree 的危险补丁（原因 a）。");
            Require(!LoadOrderRiskDetector.HasCardPatchRisk(typeof(string).Assembly, "Tests.SafeMod", patched),
                "未给 NCard/CardModel 打补丁的提供者不应触发原因 a。");

            harmony.UnpatchAll(harmonyOwner);
            harmony.Patch(
                cardPortraitPath,
                postfix: new HarmonyMethod(typeof(LoadOrderRiskNoticeTests), nameof(NoopPortraitPathPostfix)));
            patched = Harmony.GetAllPatchedMethods()
                .Select(target => (target, (Patches?)Harmony.GetPatchInfo(target)))
                .ToArray();
            Require(LoadOrderRiskDetector.HasCardPatchRisk(null, harmonyOwner, patched),
                "给 CardModel.PortraitPath 打补丁也必须触发原因 a。");
        }
        finally
        {
            harmony.UnpatchAll(harmonyOwner);
        }
    }

    private static void CheckSharedGameAssetRiskDetection()
    {
        var log = AuditLogSilencer.Install();
        var root = Directory.CreateTempSubdirectory("sc-load-order-risk-");
        var previousWriter = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            const string atlasImport = "res://images/atlases/card_atlas_0.png.import";
            const string atlasPayload = "res://.godot/imported/card_atlas_0.png-testhash.bptc.ctex";
            const string cardImport = "res://images/packed/card_portraits/fool/beta/all_is_well.png.import";
            const string cardPayload = "res://.godot/imported/all_is_well.png-testhash.ctex";
            const string privateAsset = "res://risk_mod/art/private.png";
            static byte[] Import(string payload) => Encoding.UTF8.GetBytes($"[remap]\npath=\"{payload}\"\n");

            var gamePck = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(gamePck, new Dictionary<string, byte[]>
            {
                [atlasImport] = Import(atlasPayload),
                [atlasPayload] = [1, 2, 3],
                [cardImport] = Import(cardPayload),
                [cardPayload] = [4, 5, 6]
            });

            var riskyDir = Directory.CreateDirectory(Path.Combine(root.FullName, "risky_atlas_mod")).FullName;
            var riskyPck = Path.Combine(riskyDir, "risky_atlas_mod.pck");
            PckArchive.Write(riskyPck, new Dictionary<string, byte[]>
            {
                [atlasPayload] = [9, 9, 9],
                [privateAsset] = [7]
            });

            var safeDir = Directory.CreateDirectory(Path.Combine(root.FullName, "safe_mod")).FullName;
            var safePck = Path.Combine(safeDir, "safe_mod.pck");
            PckArchive.Write(safePck, new Dictionary<string, byte[]>
            {
                [privateAsset] = [7]
            });

            using var catalog = SkinCatalog.Build(gamePck, []);
            Require(LoadOrderRiskDetector.HasSharedGameAssetRisk([atlasPayload, privateAsset], catalog) &&
                    LoadOrderRiskDetector.HasSharedGameAssetRisk([cardPayload], catalog) &&
                    !LoadOrderRiskDetector.HasSharedGameAssetRisk([privateAsset], catalog),
                "含共享图集或原版卡图导入载荷的 PCK 必须命中原因 b，纯私有路径不得误报。");
            Require(LoadOrderRiskDetector.HasSharedGameAssetRisk(["res://images/atlases/card_atlas_0.png"], catalog: null) &&
                    !LoadOrderRiskDetector.HasSharedGameAssetRisk([privateAsset], catalog: null),
                "即使 catalog 为空，显式共享图集路径也应命中原因 b。");

            var riskyMod = MakeMod("risky_atlas_mod", "共享图集包", riskyDir);
            var safeMod = MakeMod("safe_mod", "安全皮肤包", safeDir);
            var detected = LoadOrderRiskDetector.DetectDangerousProviders([riskyMod, safeMod], catalog);
            Require(detected.Count == 1 &&
                    ReferenceEquals(detected[0].Mod, riskyMod) &&
                    detected[0].Reasons == LoadOrderRiskReasons.SharedGameAsset,
                "DetectDangerousProviders 应仅检出含共享游戏导入产物的提前加载提供者。");

            var both = new DangerousLoadOrderProvider(
                riskyMod, "共享图集包", LoadOrderRiskReasons.CardPatch | LoadOrderRiskReasons.SharedGameAsset);
            var logLine = LoadOrderRiskDetector.FormatRiskLogLine([detected[0], both]);
            Require(logLine == "[加载顺序] 提前加载的危险提供者: 共享图集包(原因b)、共享图集包(原因a/b)",
                "结构化风险日志格式不符：" + logLine);
        }
        finally
        {
            PckArchive.WriteOutput = previousWriter;
            log.UnpatchAll(log.Id);
            try { root.Delete(recursive: true); } catch { }
        }
    }

    private static void CheckExternalLoadOrderManagerDetection()
    {
        var loadedManager = MakeMod("LoadOrderManager", "模组加载顺序管理", state: ModLoadState.Loaded);
        var unloadedManager = MakeMod("LoadOrderManager", "模组加载顺序管理", state: ModLoadState.None);
        var lookalikeByName = MakeMod("OtherModId", "LoadOrderManager", state: ModLoadState.Loaded);
        var normalSkin = MakeMod("SomeSkin", "SomeSkin", state: ModLoadState.Loaded);

        Require(LoadOrderRiskDetector.IsExternalLoadOrderManagerLoaded([normalSkin, loadedManager]),
            "已加载 manifest id 为 LoadOrderManager 的 Mod 时必须识别。");
        Require(!LoadOrderRiskDetector.IsExternalLoadOrderManagerLoaded([normalSkin, unloadedManager]),
            "未加载的 LoadOrderManager 不应判定为生效。");
        Require(!LoadOrderRiskDetector.IsExternalLoadOrderManagerLoaded([normalSkin, lookalikeByName]),
            "必须严格按 manifest id 识别，不得按显示名猜测 LoadOrderManager。");
    }

    private static void CheckPromptPlanMatrix()
    {
        var riskyMod = MakeMod("AI_silent_NSFW_MOD", "AI_silent_NSFW_MOD");
        var safeMod = MakeMod("SafeSkin", "SafeSkin");
        var dangerous = new[]
        {
            new DangerousLoadOrderProvider(riskyMod, "AI_silent_NSFW_MOD", LoadOrderRiskReasons.CardPatch)
        };

        // 1. 危险包 + 排序未变 + 无 LoadOrderManager：每次启动仍必须弹风险提示，给优先并重启。
        var riskUnchanged = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: false,
            isFirstInstall: false,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: false,
            shouldShowWarning: true,
            providersAboveSelf: [riskyMod, safeMod],
            dangerousProviders: dangerous);
        Require(riskUnchanged.Kind == LoadOrderPromptKind.RiskChoice &&
                riskUnchanged.MessageKey == ModText.LoadOrderRiskMessage &&
                riskUnchanged.ShowRestartButton &&
                !riskUnchanged.ShouldAutoMove &&
                !riskUnchanged.SaveSnapshotOnDecline &&
                riskUnchanged.ListedProviders.SequenceEqual([riskyMod]),
            "有危险包且未装 LoadOrderManager 时，即便排序未变也必须每次启动提醒并提供重启按钮。");

        // 2. 危险包 + 排序未变 + 有 LoadOrderManager：每次启动提醒，不自动移动、不给优先并重启，只给知道了。
        var riskWithManager = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: true,
            isFirstInstall: false,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: false,
            shouldShowWarning: true,
            providersAboveSelf: [riskyMod, safeMod],
            dangerousProviders: dangerous);
        Require(riskWithManager.Kind == LoadOrderPromptKind.RiskWithExternalManager &&
                riskWithManager.MessageKey == ModText.LoadOrderRiskWithManagerMessage &&
                !riskWithManager.ShowRestartButton &&
                !riskWithManager.ShouldAutoMove &&
                riskWithManager.ListedProviders.SequenceEqual([riskyMod]),
            "检测到 LoadOrderManager 且有危险包时，必须改为指导在 Load Order Manager 中拖动且不做自动移动、不显示优先并重启。");

        // 3. 首次安装 + 有 LoadOrderManager + 无危险包：不做自动移动、不给优先并重启。
        var firstInstallWithManager = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: true,
            isFirstInstall: true,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: false,
            shouldShowWarning: true,
            providersAboveSelf: [safeMod],
            dangerousProviders: []);
        Require(firstInstallWithManager.Kind == LoadOrderPromptKind.OrderWithExternalManager &&
                firstInstallWithManager.MessageKey == ModText.LoadOrderWithManagerMessage &&
                !firstInstallWithManager.ShowRestartButton &&
                !firstInstallWithManager.ShouldAutoMove,
            "检测到 LoadOrderManager 时，首次安装也不能自动移动或显示优先并重启。");

        // 4. 首次安装 + 无 LoadOrderManager + 无危险包：保留原有自动移动与提示流程。
        var firstInstallOriginal = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: false,
            isFirstInstall: true,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: false,
            shouldShowWarning: true,
            providersAboveSelf: [safeMod],
            dangerousProviders: []);
        Require(firstInstallOriginal.Kind == LoadOrderPromptKind.FirstInstallAutoMoved &&
                firstInstallOriginal.MessageKey == ModText.LoadOrderMessage &&
                firstInstallOriginal.ShowRestartButton &&
                firstInstallOriginal.ShouldAutoMove,
            "无 LoadOrderManager 且无危险包的首次安装必须原样保留自动移动与重启提示。");

        // 5. 无危险包 + 排序未变：保持静默，行为不变。
        var safeUnchanged = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: false,
            isFirstInstall: false,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: false,
            shouldShowWarning: true,
            providersAboveSelf: [safeMod],
            dangerousProviders: []);
        Require(safeUnchanged.Kind == LoadOrderPromptKind.None && !safeUnchanged.ShouldAutoMove,
            "无危险包且排序未变时不得弹出提示。");

        // 6. 无危险包 + 排序变更 + 无 LoadOrderManager：保持原有询问框行为。
        var safeChanged = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalLoadOrderManager: false,
            isFirstInstall: false,
            isUpgradeBaselineOrLegacySnapshot: false,
            orderChanged: true,
            shouldShowWarning: true,
            providersAboveSelf: [safeMod],
            dangerousProviders: []);
        Require(safeChanged.Kind == LoadOrderPromptKind.OrderChangedChoice &&
                safeChanged.MessageKey == ModText.LoadOrderChoiceMessage &&
                safeChanged.ShowRestartButton &&
                !safeChanged.ShouldAutoMove &&
                safeChanged.SaveSnapshotOnDecline,
            "无危险包且排序变更时必须保留原有选择询问框。");

        var renderedRisk = LoadOrderWarningController.BuildPromptMessage(riskUnchanged);
        var renderedManager = LoadOrderWarningController.BuildPromptMessage(riskWithManager);
        Require(renderedRisk.Contains("AI_silent_NSFW_MOD", StringComparison.Ordinal) &&
                renderedManager.Contains("Load Order Manager", StringComparison.Ordinal),
            "渲染后的提示正文应列出危险包名并在存在 LoadOrderManager 时指引使用 Load Order Manager。");
    }

    private static void CheckLocalizationCoverage()
    {
        string[] languages =
        [
            "eng", "zhs", "zht", "deu", "esp", "spa", "fra", "ita",
            "jpn", "kor", "pol", "ptb", "rus", "tha", "tur"
        ];
        ModText[] keys =
        [
            ModText.LoadOrderRiskMessage,
            ModText.LoadOrderRiskWithManagerMessage,
            ModText.LoadOrderWithManagerMessage,
            ModText.LoadOrderRiskFirstInstallMessage
        ];
        var previousLanguage = ModLocalization.CurrentLanguage;
        var setLanguage = AccessTools.PropertySetter(typeof(ModLocalization), nameof(ModLocalization.CurrentLanguage)) ??
                          AccessTools.Field(typeof(ModLocalization), "<CurrentLanguage>k__BackingField") as MemberInfo;
        try
        {
            foreach (var lang in languages)
            {
                if (setLanguage is PropertyInfo prop) prop.SetValue(null, lang);
                else if (setLanguage is MethodInfo setter) setter.Invoke(null, [lang]);
                else if (setLanguage is FieldInfo field) field.SetValue(null, lang);

                foreach (var key in keys)
                {
                    var text = ModLocalization.Get(key);
                    Require(!string.IsNullOrWhiteSpace(text) && text.Contains("{0}", StringComparison.Ordinal),
                        $"语言 {lang} 的 {key} 文案不能为空且必须包含 {{0}} 占位符。");
                }
            }
        }
        finally
        {
            if (setLanguage is PropertyInfo prop) prop.SetValue(null, previousLanguage);
            else if (setLanguage is MethodInfo setter) setter.Invoke(null, [previousLanguage]);
            else if (setLanguage is FieldInfo field) field.SetValue(null, previousLanguage);
        }
    }

    private static Mod MakeMod(
        string id,
        string name,
        string path = "unused-risk-test",
        ModLoadState state = ModLoadState.Loaded) => new()
    {
        path = path,
        manifest = new ModManifest { id = id, name = name, affectsGameplay = false },
        modSource = ModSource.SteamWorkshop,
        state = state
    };

    private static void UnrelatedTarget() { }
    private static void NoopNCardPostfix() { }
    private static void NoopPortraitPathPostfix() { }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
