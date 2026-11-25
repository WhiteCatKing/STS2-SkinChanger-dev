using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

internal static class PostReviewFixesTests
{
    private static readonly string[] NineAncientIds =
    [
        "darv",
        "neow",
        "nonupeipe",
        "orobas",
        "pael",
        "tanx",
        "tezcatara",
        "vakuu",
        "the_architect"
    ];

    private static readonly Dictionary<string, object> FakeActiveHurtCache = new(StringComparer.Ordinal);
    private static PackedScene? _stubReplacementScene;
    private static Texture2D? _stubLoadedEventTexture;
    private static Assembly? _stubActiveMultiAssembly;
    private static readonly Dictionary<string, Assembly> StubActiveAssembliesByProvider = new(StringComparer.OrdinalIgnoreCase);
    private static string? _stubConfigPath;
    private static string? _stubLanguage;
    private static int _runtimeSceneLoadCalls;
    private static int _runtimeResourceLoadCalls;
    private static int _eventLoadTextureCalls;
    private static int _mountOverlayCalls;
    private static int _prepareOverlayCalls;
    private static bool _suppressMountOverlay;
    private static bool _throwInLeave;
    private static bool _throwInContinueRun;
    private static bool _throwInCapture;
    private static bool _throwInApply;
    private static bool _throwInCurrentEncounter;

    internal static void Run()
    {
        var silencer = AuditLogSilencer.Install();
        try
        {
            VerifyEventBackgroundSceneDoubleGuardAndNineAncients();
            VerifyTextOnlyEventOptionsDisplayNoRepackAndFollowCategory();
            VerifyHybridFullRuntimeDllTextOnlyEventIsolation();
            VerifyFakeMerchantBattleGroupClearsTextOnlyWhenTentAssetsMerged();
            VerifyEventRoomVfxDoesNotDuplicateAcrossTextOnlySwitches();
            VerifyApplySelectionUsesPreviousSelectionsForReassertAndCacheReset();
            VerifyNewlyGuardedHooksIsolateExceptions();
            VerifyFakeMerchantBackgroundNudgeRequiresDebugEnv();
            Console.WriteLine("Post-review fixes passed: event background double guard (ordinary skipped + 9 Ancients preserved), text-only event detection/localization/no-repack/follow-category, hybrid full-runtime DLL text-only isolation, fake-merchant battle/sale/punch-off text-only accuracy, event room VFX non-duplication, ApplySelection previous-selection reassert/cache-reset, newly guarded hook isolation, and STS2SC_DEBUG nudge gate.");
        }
        finally
        {
            silencer.UnpatchAll(silencer.Id);
        }
    }

    private static void VerifyEventBackgroundSceneDoubleGuardAndNineAncients()
    {
        var directory = Directory.CreateTempSubdirectory("sc-event-bg-guard-");
        var harmony = new Harmony("Gurio.SkinChanger.Tests.EventBgGuard");
        var oldCatalog = SkinService.Catalog;
        var oldConfig = SkinService.Config;
        try
        {
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var skinFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var sceneBytes = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Background\" type=\"Control\"]\n");
            var skinSceneBytes = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"BackgroundSkin\" type=\"Control\"]\n");

            foreach (var ancientId in NineAncientIds)
            {
                var ancientScene = $"res://scenes/events/background_scenes/{ancientId}.tscn";
                baseline[ancientScene] = sceneBytes;
                skinFiles[ancientScene] = skinSceneBytes;
            }

            baseline["res://images/events/punch_off.png.import"] = Encoding.UTF8.GetBytes("path=\"res://.godot/imported/punch_off.ctex\"\n");
            baseline["res://.godot/imported/punch_off.ctex"] = [1, 2, 3, 4];
            baseline["res://localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Original description"
            }));
            skinFiles["res://nsfw/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Modified text only"
            }));

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var skinPath = Path.Combine(directory.FullName, "skin.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(skinPath, skinFiles);

            var descriptors = new[]
            {
                new SkinModDescriptor("NSFW-Event", "NSFW-Event", skinPath, false, directory.FullName, false)
            };
            using var catalog = SkinCatalog.Build(gamePath, descriptors);

            var config = new SkinConfig();
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);

            // 1) Ordinary event PUNCH_OFF has no background_scenes/punch_off.tscn in baseline or skin.
            var punchModel = CreateSyntheticEventModel("PUNCH_OFF");
            foreach (var choice in new[] { SkinCatalog.BaseOptionId, "NSFW-Event" })
            {
                config.Selections["event:punch_off"] = choice;
                Require(!EventCompendiumPreview.ShouldRequestBackgroundScene(punchModel, "event:punch_off", _ => false),
                    $"普通事件 PUNCH_OFF（选择={choice}）没有背景场景时不应请求 background_scenes/punch_off.tscn。");
            }

            // If an event actually exists in ResourceLoader (e.g. modded event scene), caller allows it.
            Require(EventCompendiumPreview.ShouldRequestBackgroundScene(
                    punchModel,
                    "event:punch_off",
                    path => path.Equals("res://scenes/events/background_scenes/punch_off.tscn", StringComparison.OrdinalIgnoreCase)),
                "实际存在背景场景的自定义事件应允许请求 CreateBackgroundScene。");

            // 2) Interceptor layer: ReplaceCachedScene skips null result AND skips event scenes absent from HasEventResource.
            _stubReplacementScene = FakeGodot<PackedScene>();
            _runtimeSceneLoadCalls = 0;
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), nameof(SkinService.GetOrLoadRuntimeScene)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubGetOrLoadRuntimeScene)));

            var punchScenePath = "res://scenes/events/background_scenes/punch_off.tscn";
            foreach (var choice in new[] { SkinCatalog.BaseOptionId, "NSFW-Event" })
            {
                config.Selections["event:punch_off"] = choice;
                PackedScene nullResult = null!;
                ContextualSkinControls.ReplaceCachedScene(punchScenePath, ref nullResult);
                Require(nullResult == null && _runtimeSceneLoadCalls == 0,
                    $"ReplaceCachedScene 遇到 null 结果时不得调用 GetOrLoadRuntimeScene（选择={choice}）。");

                var dummyExisting = FakeGodot<PackedScene>();
                var nonNullResult = dummyExisting;
                ContextualSkinControls.ReplaceCachedScene(punchScenePath, ref nonNullResult);
                Require(ReferenceEquals(nonNullResult, dummyExisting) && _runtimeSceneLoadCalls == 0,
                    $"ReplaceCachedScene 对无 HasEventResource 的事件背景场景不得接管（选择={choice}）。");
            }

            // 3) All 9 Ancients MUST still be replaceable normally when AssetCache returns their vanilla scene.
            foreach (var ancientId in NineAncientIds)
            {
                var ancientPath = $"res://scenes/events/background_scenes/{ancientId}.tscn";
                var resolvedGroup = catalog.FindGroupIdForResourcePath(ancientPath);
                Require(string.Equals(resolvedGroup, ancientId, StringComparison.OrdinalIgnoreCase),
                    $"先古 {ancientId} 应归属先古分组而非普通事件分组，实际：{resolvedGroup}");

                var vanillaAncientScene = FakeGodot<PackedScene>();
                Require(ContextualSkinControls.ShouldReplaceCachedScene(ancientId, ancientPath, vanillaAncientScene),
                    $"先古 {ancientId} 存在原版背景场景时应允许接管。");

                var beforeCalls = _runtimeSceneLoadCalls;
                var result = vanillaAncientScene;
                ContextualSkinControls.ReplaceCachedScene(ancientPath, ref result);
                Require(ReferenceEquals(result, _stubReplacementScene) && _runtimeSceneLoadCalls == beforeCalls + 1,
                    $"先古 {ancientId} 的 background_scenes/{ancientId}.tscn 必须照常被替换。");
            }
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, oldConfig);
            _stubReplacementScene = null;
            _runtimeSceneLoadCalls = 0;
            directory.Delete(true);
        }
    }

    private static void VerifyTextOnlyEventOptionsDisplayNoRepackAndFollowCategory()
    {
        var directory = Directory.CreateTempSubdirectory("sc-event-text-only-");
        var harmony = new Harmony("Gurio.SkinChanger.Tests.EventTextOnly");
        var oldCatalog = SkinService.Catalog;
        var oldConfig = SkinService.Config;
        try
        {
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(baseline, "res://images/events/punch_off.png", 1);
            AddSyntheticEventTexture(baseline, "res://images/events/alpha.png", 2);
            baseline["res://localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Original punch body",
                ["ALPHA.title"] = "Alpha",
                ["ALPHA.pages.INITIAL.description"] = "Original alpha body"
            }));
            baseline["res://localization/zhs/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "重拳出击",
                ["PUNCH_OFF.pages.INITIAL.description"] = "原版重拳正文",
                ["ALPHA.title"] = "事件甲",
                ["ALPHA.pages.INITIAL.description"] = "原版甲正文"
            }));

            // MixedPack has image + text for ALPHA, but ONLY text for PUNCH_OFF (must be decided by resources per event).
            var mixedPack = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(mixedPack, "res://images/events/alpha.png", 9);
            mixedPack["res://mixed/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["ALPHA.pages.INITIAL.description"] = "Mixed alpha body",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Modified text only"
            }));
            mixedPack["res://mixed/localization/zhs/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.pages.INITIAL.description"] = "改写后的正文"
            }));

            // PureTextPack has ONLY text across every event it touches.
            var pureTextPack = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://pure/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["PUNCH_OFF.pages.INITIAL.description"] = "Pure text only"
                }))
            };

            // VisualPunchPack ships an actual portrait image + text for PUNCH_OFF.
            var visualPunchPack = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(visualPunchPack, "res://images/events/punch_off.png", 11);
            visualPunchPack["res://visual/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.pages.INITIAL.description"] = "Visual punch text"
            }));

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var mixedPath = Path.Combine(directory.FullName, "mixed.pck");
            var purePath = Path.Combine(directory.FullName, "pure.pck");
            var visualPath = Path.Combine(directory.FullName, "visual.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(mixedPath, mixedPack);
            PckArchive.Write(purePath, pureTextPack);
            PckArchive.Write(visualPath, visualPunchPack);

            // 1) Localization labels: zhs, eng, and all 15 supported languages + UI IL wiring.
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(ModLocalization), nameof(ModLocalization.CurrentLanguage)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubCurrentLanguage)));
            _stubLanguage = "zhs";
            Require(ModLocalization.FormatEventOptionLabel("MixedPack", isTextOnly: true) == "MixedPack（仅文本）",
                "中文界面仅文本事件项必须后缀“（仅文本）”。");
            Require(ModLocalization.FormatEventOptionLabel("MixedPack", isTextOnly: false) == "MixedPack",
                "非仅文本项不得附加仅文本后缀。");
            _stubLanguage = "eng";
            Require(ModLocalization.FormatEventOptionLabel("MixedPack", isTextOnly: true) == "MixedPack (text only)",
                "英文界面仅文本事件项必须后缀“ (text only)”。");
            Require(ModLocalization.FormatEventOptionLabel("MixedPack", isTextOnly: false) == "MixedPack",
                "英文界面非仅文本项不得附加仅文本后缀。");
            var suffixMap = (IReadOnlyDictionary<string, string>)AccessTools.Field(typeof(ModLocalization), "EventTextOnlySuffixTexts")!.GetValue(null)!;
            Require(ModLocalization.SupportedLanguages.All(lang => suffixMap.TryGetValue(lang, out var val) && !string.IsNullOrWhiteSpace(val)),
                "仅文本后缀必须覆盖全部 15 种支持语言。");
            foreach (var lang in ModLocalization.SupportedLanguages)
            {
                _stubLanguage = lang;
                Require(!string.IsNullOrWhiteSpace(ModLocalization.Get(ModText.EventTextOnlySuffix)),
                    $"ModLocalization.Get(ModText.EventTextOnlySuffix) 在语言 {lang} 下为空。");
            }
            _stubLanguage = null;

            var compendiumScreen = typeof(AncientCompendiumEntry).Assembly.GetType("STS2SkinChanger.Ui.AncientCompendiumScreen", true)!;
            foreach (var uiMethodName in new[] { "PopulateSkinDropdown", "BuildEventPriorityList" })
            {
                var uiMethod = AccessTools.Method(compendiumScreen, uiMethodName)
                               ?? throw new InvalidOperationException($"找不到 AncientCompendiumScreen.{uiMethodName}。");
                var callsFormat = PatchProcessor.GetOriginalInstructions(uiMethod)
                    .Select(i => i.operand)
                    .OfType<MethodInfo>()
                    .Any(m => m.DeclaringType == typeof(ModLocalization) &&
                              m.Name == nameof(ModLocalization.FormatEventOptionLabel));
                Require(callsFormat, $"AncientCompendiumScreen.{uiMethodName} 必须调用 ModLocalization.FormatEventOptionLabel。");
            }

            var resolvePortraitMethod = AccessTools.Method(typeof(EventSkinRuntime), nameof(EventSkinRuntime.ResolvePortraitArgument))
                                        ?? throw new InvalidOperationException("找不到 EventSkinRuntime.ResolvePortraitArgument。");
            Require(PatchProcessor.GetOriginalInstructions(resolvePortraitMethod)
                    .Select(i => i.operand)
                    .OfType<MethodInfo>()
                    .Any(m => m.DeclaringType == typeof(EventSkinRuntime) &&
                              m.Name == nameof(EventSkinRuntime.ShouldUseIsolatedVisuals)),
                "EventSkinRuntime.ResolvePortraitArgument 必须先检查 ShouldUseIsolatedVisuals，避免给仅文本项走隔离加载。");

            // Stub runtime/mount boundaries so we can prove zero private PCK & zero vanilla portrait repack.
            _stubLoadedEventTexture = FakeGodot<CompressedTexture2D>();
            _stubConfigPath = Path.Combine(directory.FullName, "event_text_only_config.json");
            _suppressMountOverlay = true;
            _runtimeResourceLoadCalls = 0;
            _eventLoadTextureCalls = 0;
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(SkinService), "ConfigPath"),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubConfigPath)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "MountOverlay", [typeof(IReadOnlySet<string>), typeof(bool)]),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(CountAndSuppressMountOverlay)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "GetOrPrepareRuntimeOverlay"),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(FailIfPrepareRuntimeOverlayCalled)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), nameof(SkinService.GetOrLoadRuntimeResource)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(CountRuntimeResourceLoad)));
            harmony.Patch(
                AccessTools.Method(typeof(EventSkinRuntime), nameof(EventSkinRuntime.LoadTexture)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubEventLoadTexture)));

            // 2) Catalog #1: MixedPack + PureTextPack (no visual pack for punch_off).
            var punchModel = CreateSyntheticEventModel("PUNCH_OFF");
            var alphaModel = CreateSyntheticEventModel("ALPHA");
            var descriptors1 = new[]
            {
                new SkinModDescriptor("MixedPack", "MixedPack", mixedPath, false, directory.FullName, false),
                new SkinModDescriptor("PureTextPack", "PureTextPack", purePath, false, directory.FullName, false)
            };
            using (var catalog = SkinCatalog.Build(gamePath, descriptors1))
            {
                var alphaGroup = catalog.Groups.Single(g => g.Id == "event:alpha");
                var punchGroup = catalog.Groups.Single(g => g.Id == "event:punch_off");
                Require(!alphaGroup.Options.Single(o => o.Id == "MixedPack").IsTextOnly,
                    "同时有图和文本的 MixedPack 在 event:alpha 上不得标为仅文本。");
                Require(punchGroup.Options.Single(o => o.Id == "MixedPack").IsTextOnly,
                    "只有文本、无资源的 MixedPack 在 event:punch_off 上必须标为仅文本。");
                Require(punchGroup.Options.Single(o => o.Id == "PureTextPack").IsTextOnly,
                    "纯文本包 PureTextPack 在 event:punch_off 上必须标为仅文本。");
                Require(!catalog.HasVisualEventOption("event:punch_off") && catalog.HasVisualEventOption("event:alpha"),
                    "HasVisualEventOption 必须按事件实际资源区分。");

                var config = new SkinConfig();
                config.EventSkinPriorities.RegionGroups["act1"] = ["event:alpha", "event:punch_off"];
                config.EventSkinPriorities.Priorities["act1"] =
                [
                    new EventSkinPriorityEntry("MixedPack", true),
                    new EventSkinPriorityEntry("PureTextPack", true)
                ];
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalog);
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);

                // Region priority metadata marks PureTextPack as text-only, while MixedPack (has visual alpha) is not.
                var regionOptions = SkinService.GetEventPriorityOptions("act1");
                Require(!regionOptions.Single(o => o.OptionId == "MixedPack").IsTextOnly &&
                        regionOptions.Single(o => o.OptionId == "PureTextPack").IsTextOnly,
                    "地区优先级列表应仅对全覆盖皆为仅文本的包标仅文本。");

                // 2a) Direct selection of text-only MixedPack on event:punch_off: zero PCK / zero portrait repack / text works.
                Require(SkinService.ApplySelection("event:punch_off", "MixedPack"),
                    "直接选择仅文本事件项应成功：" + SkinService.LastError);
                Require(_mountOverlayCalls == 0 && _prepareOverlayCalls == 0,
                    "选择仅文本事件项不得生成私有包或触发无意义覆盖挂载。");
                Require(!config.VisualProviderPriority.Contains("MixedPack", StringComparer.OrdinalIgnoreCase),
                    "仅文本事件项不得记入 VisualProviderPriority 视觉生效列表。");
                Require(!EventSkinRuntime.HasActiveVisualSelection("event:punch_off") &&
                        !EventSkinRuntime.ShouldUseIsolatedVisuals("event:punch_off"),
                    "仅文本事件项不得视为视觉已生效或启用隔离视觉加载。");
                Require(!EventSkinRuntime.TryLoadInitialPortrait(punchModel, false, out _) && _eventLoadTextureCalls == 0,
                    "选中仅文本事件项时 TryLoadInitialPortrait 必须返回 false 且不加载原版图。");

                var vanillaPunchTex = (Texture2D)FakeGodot<CompressedTexture2D>();
                var cachedTex = vanillaPunchTex;
                ContextualSkinControls.ReplaceCachedTexture("res://images/events/punch_off.png", ref cachedTex);
                Require(ReferenceEquals(cachedTex, vanillaPunchTex) && _runtimeResourceLoadCalls == 0,
                    "ReplaceCachedTexture 对仅文本事件不得调 GetOrLoadRuntimeResource 重打包原版图。");

                Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "zhs", config.Selections, out var zhText) &&
                        zhText == "改写后的正文",
                    $"仅文本事件中文正文仍须生效，实际：{zhText}");
                Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "eng", config.Selections, out var enText) &&
                        enText == "Modified text only",
                    $"仅文本事件英文正文仍须生效，实际：{enText}");

                // Meanwhile MixedPack on event:alpha (which HAS an image) still behaves as visual active.
                Require(SkinService.ApplySelection("event:alpha", "MixedPack"),
                    "选择带图事件项应成功：" + SkinService.LastError);
                Require(EventSkinRuntime.HasActiveVisualSelection("event:alpha") &&
                        EventSkinRuntime.ShouldUseIsolatedVisuals("event:alpha") &&
                        EventSkinRuntime.TryLoadInitialPortrait(alphaModel, false, out var alphaPortrait) &&
                        ReferenceEquals(alphaPortrait, _stubLoadedEventTexture),
                    "同包带图事件 event:alpha 必须照常走隔离图加载并视为视觉已生效。");

                // 2b) Follow Category resolving to a text-only pack on event:punch_off.
                config.Selections.Remove("event:alpha");
                config.Selections["event:punch_off"] = SkinCatalog.BaseOptionId;
                config.VisualProviderPriority.Clear();
                config.EventSkinPriorities.RegionGroups["act1"] = ["event:punch_off"];
                config.EventSkinPriorities.ManualGroups = ["event:punch_off"];
                _mountOverlayCalls = 0;
                _eventLoadTextureCalls = 0;
                _runtimeResourceLoadCalls = 0;

                Require(SkinService.FollowEventCategoryPriority("event:punch_off"),
                    "事件切回跟随分类应成功：" + SkinService.LastError);
                Require(config.Selections["event:punch_off"] == "MixedPack",
                    "跟随分类应按优先级解析到 MixedPack。");
                Require(_mountOverlayCalls == 0 && _prepareOverlayCalls == 0,
                    "跟随分类解析到仅文本包时不得生成私有包或无意义重挂覆盖。");
                Require(!config.VisualProviderPriority.Contains("MixedPack", StringComparer.OrdinalIgnoreCase),
                    "跟随分类解析到仅文本包时不得把仅文本当成视觉已生效写入 VisualProviderPriority。");
                Require(!EventSkinRuntime.HasActiveVisualSelection("event:punch_off") &&
                        !EventSkinRuntime.ShouldUseIsolatedVisuals("event:punch_off"),
                    "跟随分类解析到仅文本包时 HasActiveVisualSelection / ShouldUseIsolatedVisuals 必须为 false。");
                Require(!EventSkinRuntime.TryLoadInitialPortrait(punchModel, false, out _) && _eventLoadTextureCalls == 0,
                    "跟随分类解析到仅文本包时 TryLoadInitialPortrait 必须直接跳过、不打包原版图。");
                Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "zhs", config.Selections, out var followZh) &&
                        followZh == "改写后的正文",
                    $"跟随分类解析到仅文本包时文本替换仍须生效，实际：{followZh}");
            }

            // 3) Catalog #2: event:punch_off has BOTH VisualPunchPack (image+text) and PureTextPack (text-only).
            var descriptors2 = new[]
            {
                new SkinModDescriptor("VisualPunchPack", "VisualPunchPack", visualPath, false, directory.FullName, false),
                new SkinModDescriptor("PureTextPack", "PureTextPack", purePath, false, directory.FullName, false)
            };
            using (var catalog2 = SkinCatalog.Build(gamePath, descriptors2))
            {
                var group = catalog2.Groups.Single(g => g.Id == "event:punch_off");
                Require(!group.Options.Single(o => o.Id == "VisualPunchPack").IsTextOnly &&
                        group.Options.Single(o => o.Id == "PureTextPack").IsTextOnly,
                    "同事件下有图包与仅文本包并存时，仅文本标记必须准确区分。");

                var config2 = new SkinConfig();
                config2.EventSkinPriorities.RegionGroups["act1"] = ["event:punch_off"];
                config2.EventSkinPriorities.Priorities["act1"] =
                [
                    new EventSkinPriorityEntry("PureTextPack", true),
                    new EventSkinPriorityEntry("VisualPunchPack", true)
                ];
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalog2);
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config2);

                _mountOverlayCalls = 0;
                _eventLoadTextureCalls = 0;
                _runtimeResourceLoadCalls = 0;

                // Selecting VisualPunchPack activates visuals normally.
                Require(SkinService.ApplySelection("event:punch_off", "VisualPunchPack"),
                    "选择带图事件包应成功：" + SkinService.LastError);
                Require(_mountOverlayCalls == 1 &&
                        config2.VisualProviderPriority.Contains("VisualPunchPack", StringComparer.OrdinalIgnoreCase) &&
                        EventSkinRuntime.HasActiveVisualSelection("event:punch_off") &&
                        EventSkinRuntime.TryLoadInitialPortrait(punchModel, false, out _) &&
                        _eventLoadTextureCalls == 1,
                    "选择带图事件包时应挂载覆盖、记录视觉生效并走隔离图加载。");

                // Switching via Follow Category from VisualPunchPack -> PureTextPack must restore baseline once,
                // drop visual-active status, and stop isolated portrait loads for PureTextPack while applying its text.
                _mountOverlayCalls = 0;
                _eventLoadTextureCalls = 0;
                _runtimeResourceLoadCalls = 0;
                Require(SkinService.FollowEventCategoryPriority("event:punch_off"),
                    "从带图包切回跟随分类（解析到仅文本包）应成功：" + SkinService.LastError);
                Require(config2.Selections["event:punch_off"] == "PureTextPack",
                    "跟随分类应解析到更高优先级的 PureTextPack。");
                Require(_mountOverlayCalls == 1,
                    "从带图包切到仅文本包时必须重挂覆盖以还原带图包改动。");
                Require(!config2.VisualProviderPriority.Contains("PureTextPack", StringComparer.OrdinalIgnoreCase) &&
                        !config2.VisualProviderPriority.Contains("VisualPunchPack", StringComparer.OrdinalIgnoreCase),
                    "切到仅文本包后 VisualProviderPriority 不得残留或新增仅文本提供者。");
                Require(!EventSkinRuntime.HasActiveVisualSelection("event:punch_off") &&
                        !EventSkinRuntime.ShouldUseIsolatedVisuals("event:punch_off"),
                    "存在其它带图包时，选中仅文本包仍不得把仅文本视为视觉已生效或启用隔离打包。");
                Require(!EventSkinRuntime.TryLoadInitialPortrait(punchModel, false, out _) && _eventLoadTextureCalls == 0,
                    "存在其它带图包时，选中仅文本包仍不得为它重打包原版图。");

                var vanillaTex = (Texture2D)FakeGodot<CompressedTexture2D>();
                var resultTex = vanillaTex;
                ContextualSkinControls.ReplaceCachedTexture("res://images/events/punch_off.png", ref resultTex);
                Require(ReferenceEquals(resultTex, vanillaTex) && _runtimeResourceLoadCalls == 0,
                    "存在其它带图包时，仅文本包仍不得触发 ReplaceCachedTexture 隔离加载。");
                Require(catalog2.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "eng", config2.Selections, out var pureText) &&
                        pureText == "Pure text only",
                    $"切到 PureTextPack 后英文文本必须生效，实际：{pureText}");
            }
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, oldConfig);
            _stubLoadedEventTexture = null;
            _stubConfigPath = null;
            _stubLanguage = null;
            _suppressMountOverlay = false;
            _runtimeResourceLoadCalls = 0;
            _eventLoadTextureCalls = 0;
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            directory.Delete(true);
        }
    }

    private static void VerifyHybridFullRuntimeDllTextOnlyEventIsolation()
    {
        // Regression for hybrid full-runtime DLL packs that own non-event visual groups AND also
        // modify only text for an ordinary event (IsTextOnly=true):
        // 1) Direct selection of the text-only event option must not link/mutate non-event groups or mount overlays.
        // 2) Switching away to another text-only or base option must not displace the provider's non-event groups or request full reassert.
        // 3) Region-follow must still select the text-only option without excluding it for owning non-event groups, while still excluding visual event options of that same full-runtime provider.
        var directory = Directory.CreateTempSubdirectory("sc-hybrid-dll-text-only-event-");
        var harmony = new Harmony("Gurio.SkinChanger.Tests.HybridDllTextOnlyEvent");
        var oldCatalog = SkinService.Catalog;
        var oldConfig = SkinService.Config;
        var reassertField = AccessTools.Field(typeof(SkinService), "_needsFullOverlayReassert")
                            ?? throw new InvalidOperationException("找不到 SkinService._needsFullOverlayReassert。");
        var oldReassert = (bool)reassertField.GetValue(null)!;
        try
        {
            var sceneBytes = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Visuals\" type=\"Node2D\"]\n");
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = sceneBytes,
                ["res://scenes/creature_visuals/gremlin_nob.tscn"] = sceneBytes
            };
            AddSyntheticEventTexture(baseline, "res://images/events/punch_off.png", 1);
            AddSyntheticEventTexture(baseline, "res://images/events/alpha.png", 2);
            baseline["res://localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Original punch body",
                ["ALPHA.title"] = "Alpha",
                ["ALPHA.pages.INITIAL.description"] = "Original alpha body"
            }));
            baseline["res://localization/zhs/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "重拳出击",
                ["PUNCH_OFF.pages.INITIAL.description"] = "原版重拳正文",
                ["ALPHA.title"] = "事件甲",
                ["ALPHA.pages.INITIAL.description"] = "原版甲正文"
            }));

            // HybridFullDll: full-runtime DLL owning jaw_worm + gremlin_nob, visual+text on event:alpha,
            // and TEXT-ONLY on event:punch_off.
            var hybridFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"HybridJaw\" type=\"Node2D\"]\n"),
                ["res://scenes/creature_visuals/gremlin_nob.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"HybridNob\" type=\"Node2D\"]\n")
            };
            AddSyntheticEventTexture(hybridFiles, "res://images/events/alpha.png", 7);
            hybridFiles["res://hybrid/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["ALPHA.pages.INITIAL.description"] = "Hybrid visual alpha body",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Hybrid text-only punch body"
            }));
            hybridFiles["res://hybrid/localization/zhs/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.pages.INITIAL.description"] = "混合包仅文本重拳正文"
            }));

            // OtherFullDll: another full-runtime DLL owning jaw_worm + gremlin_nob (active on non-event groups).
            var otherFullFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"OtherJaw\" type=\"Node2D\"]\n"),
                ["res://scenes/creature_visuals/gremlin_nob.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"OtherNob\" type=\"Node2D\"]\n")
            };

            // PureTextPack: non-DLL text-only option for event:punch_off.
            var pureTextFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://pure/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["PUNCH_OFF.pages.INITIAL.description"] = "Pure text punch body"
                }))
            };

            // VisualPunchPack: visual (portrait + text) event pack for event:punch_off.
            var visualPunchFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(visualPunchFiles, "res://images/events/punch_off.png", 12);
            visualPunchFiles["res://vispunch/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.pages.INITIAL.description"] = "Visual punch body"
            }));

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var hybridPath = Path.Combine(directory.FullName, "hybrid.pck");
            var otherFullPath = Path.Combine(directory.FullName, "other_full.pck");
            var pureTextPath = Path.Combine(directory.FullName, "pure_text.pck");
            var visualPunchPath = Path.Combine(directory.FullName, "visual_punch.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(hybridPath, hybridFiles);
            PckArchive.Write(otherFullPath, otherFullFiles);
            PckArchive.Write(pureTextPath, pureTextFiles);
            PckArchive.Write(visualPunchPath, visualPunchFiles);

            var descriptors = new[]
            {
                new SkinModDescriptor("HybridFullDll", "HybridFullDll", hybridPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: true),
                new SkinModDescriptor("OtherFullDll", "OtherFullDll", otherFullPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: true),
                new SkinModDescriptor("PureTextPack", "PureTextPack", pureTextPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: false),
                new SkinModDescriptor("VisualPunchPack", "VisualPunchPack", visualPunchPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: false)
            };
            using var catalog = SkinCatalog.Build(gamePath, descriptors);

            Require(catalog.ProviderUsesFullRuntime("HybridFullDll") &&
                    catalog.GetFullRuntimeProviderGroups("HybridFullDll").Count == 2,
                $"夹具中的 HybridFullDll 必须是非事件多分组全量运行时提供者，实际分组数={catalog.GetFullRuntimeProviderGroups("HybridFullDll").Count}。");
            Require(catalog.IsEventOptionTextOnly("event:punch_off", "HybridFullDll") &&
                    !catalog.IsEventOptionTextOnly("event:alpha", "HybridFullDll"),
                "HybridFullDll 在 event:punch_off 上必须是仅文本项，在 event:alpha 上必须是有图非仅文本项。");

            _stubConfigPath = Path.Combine(directory.FullName, "hybrid_config.json");
            _suppressMountOverlay = true;
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(SkinService), "ConfigPath"),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubConfigPath)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "MountOverlay", [typeof(IReadOnlySet<string>), typeof(bool)]),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(CountAndSuppressMountOverlay)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "GetOrPrepareRuntimeOverlay"),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(FailIfPrepareRuntimeOverlayCalled)));

            var config = new SkinConfig();
            config.Selections["jaw_worm"] = "OtherFullDll";
            config.Selections["gremlin_nob"] = "OtherFullDll";
            config.Selections["event:punch_off"] = SkinCatalog.BaseOptionId;
            config.Selections["event:alpha"] = SkinCatalog.BaseOptionId;
            config.VisualProviderPriority.Add("OtherFullDll");
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);
            reassertField.SetValue(null, false);

            // Case 1: Direct selection of HybridFullDll's text-only event option.
            Require(SkinService.ApplySelection("event:punch_off", "HybridFullDll"),
                "直接选择混合 DLL 包的仅文本事件项应成功：" + SkinService.LastError);
            Require(config.Selections["event:punch_off"] == "HybridFullDll",
                "直接选择后 event:punch_off 应切到 HybridFullDll。");
            Require(config.Selections["jaw_worm"] == "OtherFullDll" &&
                    config.Selections["gremlin_nob"] == "OtherFullDll",
                $"选择混合 DLL 包的仅文本事件项绝不能联动或重置其它非事件分组：jaw_worm={config.Selections["jaw_worm"]}, gremlin_nob={config.Selections["gremlin_nob"]}");
            Require(_mountOverlayCalls == 0 && _prepareOverlayCalls == 0 && !(bool)reassertField.GetValue(null)!,
                $"直接选择仅文本事件项不得挂载覆盖层、生成私有包或触发全量重申：mounts={_mountOverlayCalls}, prepares={_prepareOverlayCalls}, reassert={reassertField.GetValue(null)}");
            Require(!config.VisualProviderPriority.Contains("HybridFullDll", StringComparer.OrdinalIgnoreCase),
                "仅文本事件项不得写入 VisualProviderPriority。");
            Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "zhs", config.Selections, out var directZh) &&
                    directZh == "混合包仅文本重拳正文",
                $"直接选择后事件文本必须正常生效，实际：{directZh}");

            // Case 2: Switch away from HybridFullDll's text-only event option while HybridFullDll IS selected on non-event groups.
            config.Selections["jaw_worm"] = "HybridFullDll";
            config.Selections["gremlin_nob"] = "HybridFullDll";
            config.VisualProviderPriority.Clear();
            config.VisualProviderPriority.Add("HybridFullDll");
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            reassertField.SetValue(null, false);

            Require(SkinService.ApplySelection("event:punch_off", "PureTextPack"),
                "从混合 DLL 仅文本事件项切到另一个仅文本项应成功：" + SkinService.LastError);
            Require(config.Selections["event:punch_off"] == "PureTextPack" &&
                    config.Selections["jaw_worm"] == "HybridFullDll" &&
                    config.Selections["gremlin_nob"] == "HybridFullDll",
                $"切走混合 DLL 仅文本事件项时不得把该包拥有的非事件分组退回原版：jaw_worm={config.Selections["jaw_worm"]}, gremlin_nob={config.Selections["gremlin_nob"]}");
            Require(_mountOverlayCalls == 0 && _prepareOverlayCalls == 0 && !(bool)reassertField.GetValue(null)!,
                $"切走混合 DLL 仅文本事件项不得触发覆盖挂载或 FindReplacedMultiGroupFullRuntimeProvider 全量重申：mounts={_mountOverlayCalls}, reassert={reassertField.GetValue(null)}");
            Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "eng", config.Selections, out var switchedEn) &&
                    switchedEn == "Pure text punch body",
                $"切到 PureTextPack 后事件文本应生效，实际：{switchedEn}");

            // Also verify switching HybridFullDll (text-only) -> Game Default leaves non-event groups untouched.
            Require(SkinService.ApplySelection("event:punch_off", "HybridFullDll"), "重选混合包仅文本项应成功。");
            Require(SkinService.ApplySelection("event:punch_off", SkinCatalog.BaseOptionId), "切回原版应成功。");
            Require(config.Selections["jaw_worm"] == "HybridFullDll" &&
                    config.Selections["gremlin_nob"] == "HybridFullDll" &&
                    _mountOverlayCalls == 0 &&
                    !(bool)reassertField.GetValue(null)!,
                "混合包仅文本事件项切回原版不得影响非事件分组或触发覆盖挂载/全量重申。");
            Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "eng", config.Selections, out var baseEn) &&
                    baseEn == "Original punch body",
                $"切回原版后事件文本应恢复基线，实际：{baseEn}");

            // Case 3: Region-follow with HybridFullDll enabled at top priority in act1 (containing both visual event:alpha and text-only event:punch_off).
            config.Selections["jaw_worm"] = "OtherFullDll";
            config.Selections["gremlin_nob"] = "OtherFullDll";
            config.Selections["event:punch_off"] = SkinCatalog.BaseOptionId;
            config.Selections["event:alpha"] = SkinCatalog.BaseOptionId;
            config.EventSkinPriorities.RegionGroups["act1"] = ["event:alpha", "event:punch_off"];
            config.EventSkinPriorities.Priorities["act1"] =
            [
                new EventSkinPriorityEntry("HybridFullDll", true),
                new EventSkinPriorityEntry("PureTextPack", true)
            ];
            config.EventSkinPriorities.ManualGroups = ["event:punch_off"];
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            reassertField.SetValue(null, false);

            Require(SkinService.FollowEventCategoryPriority("event:punch_off"),
                "事件跟随地区优先级选择混合 DLL 包的仅文本选项应成功：" + SkinService.LastError);
            Require(config.Selections["event:punch_off"] == "HybridFullDll",
                $"混合 DLL 包的仅文本事件项在按地区跟随时不得因拥有非事件分组而被静默排除，实际：{config.Selections["event:punch_off"]}");
            Require(config.Selections["event:alpha"] == SkinCatalog.BaseOptionId,
                $"同包带图事件 event:alpha 仍须因拥有非事件全量分组而被地区跟随安全排除，实际：{config.Selections["event:alpha"]}");
            Require(config.Selections["jaw_worm"] == "OtherFullDll" &&
                    config.Selections["gremlin_nob"] == "OtherFullDll",
                $"按地区跟随仅文本事件项不得改动非事件分组：jaw_worm={config.Selections["jaw_worm"]}, gremlin_nob={config.Selections["gremlin_nob"]}");
            Require(_mountOverlayCalls == 0 && _prepareOverlayCalls == 0 && !(bool)reassertField.GetValue(null)!,
                $"按地区跟随解析到混合包仅文本项不得触发覆盖挂载或私有打包：mounts={_mountOverlayCalls}, prepares={_prepareOverlayCalls}");
            Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "zhs", config.Selections, out var followZh) &&
                    followZh == "混合包仅文本重拳正文",
                $"按地区跟随命中混合包仅文本项后事件语言表应生效，实际：{followZh}");

            // Case 4: Visual <-> Hybrid text-only round-trip on the SAME event group (event:punch_off).
            // The visual side requires MountOverlay, but ProviderRuntimeCacheReset must ONLY receive the
            // real visual provider (VisualPunchPack) and MUST NOT wipe HybridFullDll's active cache used
            // by its non-event groups (jaw_worm / gremlin_nob).
            var hybridAsm = BuildSyntheticCacheAssembly(out var hybridCacheField);
            var visualPunchAsm = BuildSyntheticCacheAssembly(out var visualPunchCacheField);
            var hybridCacheDict = (Dictionary<string, object>)hybridCacheField.GetValue(null)!;
            var visualPunchCacheDict = (Dictionary<string, object>)visualPunchCacheField.GetValue(null)!;
            StubActiveAssembliesByProvider["HybridFullDll"] = hybridAsm;
            StubActiveAssembliesByProvider["VisualPunchPack"] = visualPunchAsm;
            harmony.Patch(
                AccessTools.Method(typeof(ManagedSkinModLoader), nameof(ManagedSkinModLoader.GetActiveProviderAssembly)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubGetActiveProviderAssembly)));

            config.Selections["jaw_worm"] = "HybridFullDll";
            config.Selections["gremlin_nob"] = "HybridFullDll";
            config.Selections["event:punch_off"] = "VisualPunchPack";
            hybridCacheDict["jaw_worm_hurt"] = new object();
            visualPunchCacheDict["punch_off_art"] = new object();
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            reassertField.SetValue(null, false);

            // 4a) VisualPunchPack -> HybridFullDll (text-only)
            Require(SkinService.ApplySelection("event:punch_off", "HybridFullDll"),
                "同事件从带图项切到混合包仅文本项应成功：" + SkinService.LastError);
            Require(_mountOverlayCalls == 1 &&
                    config.Selections["event:punch_off"] == "HybridFullDll" &&
                    config.Selections["jaw_worm"] == "HybridFullDll" &&
                    config.Selections["gremlin_nob"] == "HybridFullDll" &&
                    !(bool)reassertField.GetValue(null)!,
                $"带图 -> 混合仅文本互切应仅挂载一次覆盖还原带图项，不得改动非事件组或触发全量重申：mounts={_mountOverlayCalls}, reassert={reassertField.GetValue(null)}");
            Require(visualPunchCacheDict.Count == 0,
                "带图 -> 混合仅文本互切时，真正退出的带图提供者 VisualPunchPack 仍应正常清空其第三方缓存。");
            Require(hybridCacheDict.Count == 1,
                "带图 -> 混合仅文本互切时，纯文本侧 HybridFullDll 的原始 ID 不得送入第三方缓存重置，其在非事件组仍在用的缓存必须保留。");

            var filteredOutgoing = SkinService.CollectSelectionTransitionOptions(
                catalog,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["event:punch_off"] = "HybridFullDll" },
                new Dictionary<string, (bool HadValue, string? Value)>(StringComparer.OrdinalIgnoreCase) { ["event:punch_off"] = (true, "VisualPunchPack") });
            Require(filteredOutgoing.SequenceEqual(["VisualPunchPack"], StringComparer.OrdinalIgnoreCase),
                $"按视觉所有权过滤后仅应提交带图提供者 VisualPunchPack，实际：{string.Join(",", filteredOutgoing)}");

            // 4b) HybridFullDll (text-only) -> VisualPunchPack
            visualPunchCacheDict["punch_off_art"] = new object();
            _mountOverlayCalls = 0;
            reassertField.SetValue(null, false);
            Require(SkinService.ApplySelection("event:punch_off", "VisualPunchPack"),
                "同事件从混合包仅文本项切回带图项应成功：" + SkinService.LastError);
            Require(_mountOverlayCalls == 1 &&
                    config.Selections["event:punch_off"] == "VisualPunchPack" &&
                    config.Selections["jaw_worm"] == "HybridFullDll" &&
                    config.Selections["gremlin_nob"] == "HybridFullDll" &&
                    !(bool)reassertField.GetValue(null)!,
                $"混合仅文本 -> 带图互切应仅挂载一次覆盖且不改动非事件组或触发全量重申：mounts={_mountOverlayCalls}, reassert={reassertField.GetValue(null)}");
            Require(visualPunchCacheDict.Count == 0,
                "混合仅文本 -> 带图互切时，切入的带图提供者 VisualPunchPack 仍应正常清空其第三方缓存。");
            Require(hybridCacheDict.Count == 1,
                "混合仅文本 -> 带图互切时，切出的仅文本侧 HybridFullDll 不得送入第三方缓存重置，其在非事件组仍在用的缓存必须保留。");

            var filteredIncoming = SkinService.CollectSelectionTransitionOptions(
                catalog,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["event:punch_off"] = "VisualPunchPack" },
                new Dictionary<string, (bool HadValue, string? Value)>(StringComparer.OrdinalIgnoreCase) { ["event:punch_off"] = (true, "HybridFullDll") });
            Require(filteredIncoming.SequenceEqual(["VisualPunchPack"], StringComparer.OrdinalIgnoreCase),
                $"按视觉所有权过滤后仅应提交带图提供者 VisualPunchPack，实际：{string.Join(",", filteredIncoming)}");
            Require(catalog.TryResolveEventText("PUNCH_OFF.pages.INITIAL.description", "eng", config.Selections, out var visualEn) &&
                    visualEn == "Visual punch body",
                $"切回带图项后事件语言表应切回 VisualPunchPack，实际：{visualEn}");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            StubActiveAssembliesByProvider.Clear();
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, oldConfig);
            reassertField.SetValue(null, oldReassert);
            _stubConfigPath = null;
            _suppressMountOverlay = false;
            _mountOverlayCalls = 0;
            _prepareOverlayCalls = 0;
            directory.Delete(true);
        }
    }

    private static void VerifyFakeMerchantBattleGroupClearsTextOnlyWhenTentAssetsMerged()
    {
        var directory = Directory.CreateTempSubdirectory("sc-fake-merchant-textonly-");
        try
        {
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(baseline, "res://images/events/punch_off.png", 1);
            AddSyntheticEventTexture(baseline, FakeMerchantEventArtPolicy.EventImage, 2);
            AddSyntheticEventTexture(baseline, "res://animations/backgrounds/fake_merchant_room/bottom/shop_fake_merchant_bottom.png", 3);
            baseline["res://localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["FAKE_MERCHANT.title"] = "Merchant?",
                ["FAKE_MERCHANT.pages.INITIAL.description"] = "Original fake merchant",
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Original punch off"
            }));

            // Single pack that mirrors real Workshop 3765802910 (NSFW-Event):
            // - modifies FAKE_MERCHANT + PUNCH_OFF text in events.json
            // - ships 2 fake-merchant tent/event background textures
            // - ships NO punch_off texture
            var eventPack = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            AddSyntheticEventTexture(eventPack, FakeMerchantEventArtPolicy.EventImage, 21);
            AddSyntheticEventTexture(eventPack, "res://animations/backgrounds/fake_merchant_room/bottom/shop_fake_merchant_bottom.png", 22);
            eventPack["res://nsfw/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["FAKE_MERCHANT.title"] = "Modified Merchant?",
                ["FAKE_MERCHANT.pages.INITIAL.description"] = "Modified fake merchant",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Modified punch off text only"
            }));

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var packPath = Path.Combine(directory.FullName, "nsfw_event.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(packPath, eventPack);

            var descriptors = new[]
            {
                new SkinModDescriptor("NSFW-Event", "NSFW-Event", packPath, false, directory.FullName, false)
            };
            using var catalog = SkinCatalog.Build(gamePath, descriptors);

            // 1) Battle group fake_merchant_monster: has 2 tent/event art assets -> MUST NOT be IsTextOnly,
            //    and GetSelectionProviderIds MUST return ["NSFW-Event"].
            var battleGroup = catalog.Groups.Single(g => g.Id == FakeMerchantEventArtPolicy.MonsterGroupId);
            var battleOption = battleGroup.Options.Single(o => o.Id == "NSFW-Event");
            Require(battleOption.Assets.Count == 2 && !battleOption.IsTextOnly,
                $"假商人战斗分组并入 2 张帐篷/背景图后不得标为仅文本：assets={battleOption.Assets.Count}, isTextOnly={battleOption.IsTextOnly}");
            Require(catalog.GetSelectionProviderIds(battleGroup.Id, "NSFW-Event")
                    .SequenceEqual(["NSFW-Event"], StringComparer.OrdinalIgnoreCase),
                "假商人战斗分组有真实背景图时 GetSelectionProviderIds 必须返回提供者 ID。");

            // 2) Sale background group event:fake_merchant_sale_background: has 2 assets -> NOT IsTextOnly.
            var saleGroup = catalog.Groups.Single(g => g.Id == FakeMerchantEventFrame.GroupId);
            var saleOption = saleGroup.Options.Single(o => o.Id == "NSFW-Event");
            Require(saleOption.Assets.Count == 2 && !saleOption.IsTextOnly,
                $"假商人售货背景分组有 2 张图时不得标为仅文本：assets={saleOption.Assets.Count}, isTextOnly={saleOption.IsTextOnly}");
            Require(catalog.GetSelectionProviderIds(saleGroup.Id, "NSFW-Event")
                    .SequenceEqual(["NSFW-Event"], StringComparer.OrdinalIgnoreCase),
                "假商人售货背景分组有图时 GetSelectionProviderIds 必须返回提供者 ID。");

            // 3) Ordinary text-only event event:punch_off: 0 assets -> MUST be IsTextOnly and empty visual providers.
            var punchGroup = catalog.Groups.Single(g => g.Id == "event:punch_off");
            var punchOption = punchGroup.Options.Single(o => o.Id == "NSFW-Event");
            Require(punchOption.Assets.Count == 0 && punchOption.IsTextOnly,
                $"普通无图事件 event:punch_off 必须继续标为仅文本：assets={punchOption.Assets.Count}, isTextOnly={punchOption.IsTextOnly}");
            Require(catalog.GetSelectionProviderIds(punchGroup.Id, "NSFW-Event").Count == 0,
                "普通无图事件 event:punch_off 的 GetSelectionProviderIds 必须为空。");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    private sealed class FakeRoomNode(string kind, string? source = null, string? origin = null)
    {
        public string Kind { get; } = kind;
        public string? Source { get; set; } = source;
        public string? Origin { get; set; } = origin;
    }

    private static void VerifyEventRoomVfxDoesNotDuplicateAcrossTextOnlySwitches()
    {
        // 1) Verify EventSkinVfxPatch registers BOTH Prefix and Postfix and marks native VFX instances.
        var vfxPostfix = AccessTools.Method(typeof(EventSkinVfxPatch), "Postfix")
                         ?? throw new InvalidOperationException("EventSkinVfxPatch 必须包含 Postfix 以统一登记原生特效。");
        var postfixCalls = PatchProcessor.GetOriginalInstructions(vfxPostfix)
            .Select(i => i.operand)
            .OfType<MethodInfo>()
            .ToArray();
        Require(postfixCalls.Any(m => m.DeclaringType == typeof(EventSkinRuntime) &&
                                      m.Name == nameof(EventSkinRuntime.MarkVfxInstance)),
            "EventSkinVfxPatch.Postfix 必须调用 EventSkinRuntime.MarkVfxInstance 标记原生特效。");

        // 2) Exercise the actual EventSkinRuntime.ReconcileTrackedVfxNodes + ResolveVfxOrigin + ShouldUseIsolatedVisuals
        //    across repeated in-room switches: Game Default / Text-Only / Follow Category / Visual packs.
        var directory = Directory.CreateTempSubdirectory("sc-event-room-vfx-");
        var oldCatalog = SkinService.Catalog;
        var oldConfig = SkinService.Config;
        try
        {
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            var vfxScene = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Vfx\" type=\"Node2D\"]\n");
            baseline["res://scenes/vfx/events/punch_off_vfx.tscn"] = vfxScene;
            AddSyntheticEventTexture(baseline, "res://images/events/punch_off.png", 1);
            baseline["res://localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PUNCH_OFF.title"] = "Punch Off",
                ["PUNCH_OFF.pages.INITIAL.description"] = "Original punch"
            }));

            var textOnlyPack = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://textonly/localization/eng/events.json"] = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["PUNCH_OFF.pages.INITIAL.description"] = "Text only punch"
                }))
            };
            var visualPackA = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/vfx/events/punch_off_vfx.tscn"] = vfxScene
            };
            var visualPackB = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/vfx/events/punch_off_vfx.tscn"] = vfxScene
            };

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var textPath = Path.Combine(directory.FullName, "text.pck");
            var visAPath = Path.Combine(directory.FullName, "vis_a.pck");
            var visBPath = Path.Combine(directory.FullName, "vis_b.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(textPath, textOnlyPack);
            PckArchive.Write(visAPath, visualPackA);
            PckArchive.Write(visBPath, visualPackB);

            const string groupId = "event:punch_off";
            const string vfxPath = "res://scenes/vfx/events/punch_off_vfx.tscn";

            // Case A: Text-only pack only (exact real-machine scenario for NSFW-Event on punch_off).
            using (var catalogTextOnly = SkinCatalog.Build(gamePath,
                       [new SkinModDescriptor("NSFW-Event", "NSFW-Event", textPath, false, directory.FullName, false)]))
            {
                var config = new SkinConfig();
                config.EventSkinPriorities.RegionGroups["act1"] = [groupId];
                config.EventSkinPriorities.Priorities["act1"] = [new EventSkinPriorityEntry("NSFW-Event", true)];
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalogTextOnly);
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);

                var authorNode = new FakeRoomNode("author_decoration");
                var initialNativeVfx = new FakeRoomNode("vfx", vfxPath, EventSkinRuntime.NativeVfxOrigin);
                var roomChildren = new List<FakeRoomNode> { authorNode, initialNativeVfx };
                var nativeCreates = 0;
                var isolatedReplaces = 0;

                void SimulateRefresh(string selection)
                {
                    config.Selections[groupId] = selection;
                    var isolate = EventSkinRuntime.ShouldUseIsolatedVisuals(groupId);
                    var expectedOrigin = EventSkinRuntime.ResolveVfxOrigin(groupId, isolate);
                    var hadVfx = EventSkinRuntime.ReconcileTrackedVfxNodes(
                        roomChildren.Where(n => n.Source != null),
                        n => n.Source!,
                        n => n.Origin ?? string.Empty,
                        (old, path, origin) =>
                        {
                            isolatedReplaces++;
                            roomChildren.Insert(roomChildren.IndexOf(old), new FakeRoomNode("vfx", path, origin));
                            return true;
                        },
                        old => roomChildren.Remove(old),
                        enabled: true,
                        isolate,
                        expectedOrigin);
                    if (!hadVfx)
                    {
                        nativeCreates++;
                        roomChildren.Add(new FakeRoomNode("vfx", vfxPath, EventSkinRuntime.NativeVfxOrigin));
                    }
                }

                // Switch repeatedly across Game Default / Text-Only / Follow Category (resolved to Text-Only).
                foreach (var choice in new[] { SkinCatalog.BaseOptionId, "NSFW-Event", "NSFW-Event", SkinCatalog.BaseOptionId, "NSFW-Event" })
                {
                    SimulateRefresh(choice);
                    var vfxNodes = roomChildren.Where(n => n.Kind == "vfx").ToArray();
                    Require(vfxNodes.Length == 1 && ReferenceEquals(vfxNodes[0], initialNativeVfx),
                        $"事件房内切换 {choice} 时原生特效必须复用且数量始终为 1，实际={vfxNodes.Length}");
                    Require(roomChildren.Contains(authorNode),
                        "刷新事件特效不得清除作者挂在立绘上的其它节点。");
                }
                Require(nativeCreates == 0 && isolatedReplaces == 0,
                    $"仅文本/原版/跟随分类间切换不得重复创建特效：nativeCreates={nativeCreates}, isolatedReplaces={isolatedReplaces}");
            }

            // Case B: Mixed room with visual packs + text-only pack: transitions swap cleanly and always keep count == 1.
            using (var catalogMixed = SkinCatalog.Build(gamePath,
                       [
                           new SkinModDescriptor("NSFW-Event", "NSFW-Event", textPath, false, directory.FullName, false),
                           new SkinModDescriptor("VisA", "VisA", visAPath, false, directory.FullName, false),
                           new SkinModDescriptor("VisB", "VisB", visBPath, false, directory.FullName, false)
                       ]))
            {
                var config = new SkinConfig();
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalogMixed);
                AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);

                var authorNode = new FakeRoomNode("author_decoration");
                var roomChildren = new List<FakeRoomNode>
                {
                    authorNode,
                    new FakeRoomNode("vfx", vfxPath, EventSkinRuntime.NativeVfxOrigin)
                };

                void SimulateRefresh(string selection, bool hasVfx = true)
                {
                    config.Selections[groupId] = selection;
                    var isolate = EventSkinRuntime.ShouldUseIsolatedVisuals(groupId);
                    var expectedOrigin = EventSkinRuntime.ResolveVfxOrigin(groupId, isolate);
                    var hadVfx = EventSkinRuntime.ReconcileTrackedVfxNodes(
                        roomChildren.Where(n => n.Source != null),
                        n => n.Source!,
                        n => n.Origin ?? string.Empty,
                        (old, path, origin) =>
                        {
                            roomChildren.Insert(roomChildren.IndexOf(old), new FakeRoomNode("vfx", path, origin));
                            return true;
                        },
                        old => roomChildren.Remove(old),
                        enabled: hasVfx,
                        isolate,
                        expectedOrigin);
                    if (!hadVfx && hasVfx)
                    {
                        roomChildren.Add(new FakeRoomNode("vfx", vfxPath, EventSkinRuntime.NativeVfxOrigin));
                    }
                }

                var sequence = new[]
                {
                    ("NSFW-Event", EventSkinRuntime.NativeVfxOrigin),
                    ("VisA", "isolated:VisA"),
                    ("VisB", "isolated:VisB"),
                    ("NSFW-Event", EventSkinRuntime.NativeVfxOrigin),
                    ("NSFW-Event", EventSkinRuntime.NativeVfxOrigin),
                    (SkinCatalog.BaseOptionId, "isolated:__base__"),
                    ("NSFW-Event", EventSkinRuntime.NativeVfxOrigin)
                };
                foreach (var (choice, expectedOrigin) in sequence)
                {
                    SimulateRefresh(choice);
                    var vfxNodes = roomChildren.Where(n => n.Kind == "vfx").ToArray();
                    Require(vfxNodes.Length == 1 && vfxNodes[0].Origin == expectedOrigin,
                        $"事件房内切到 {choice} 后特效节点数必须为 1 且来源={expectedOrigin}，实际数量={vfxNodes.Length}，来源={vfxNodes.FirstOrDefault()?.Origin}");
                    Require(roomChildren.Contains(authorNode),
                        "来源切换替换特效节点时不得误删作者挂载节点。");
                }

                // When an event no longer has VFX, tracked VFX is removed while author node stays.
                SimulateRefresh("NSFW-Event", hasVfx: false);
                Require(roomChildren.Count == 1 && ReferenceEquals(roomChildren[0], authorNode),
                    "HasVfx=false 时应只移除受管特效并保留作者节点。");
            }
        }
        finally
        {
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, oldConfig);
            directory.Delete(true);
        }
    }

    private static void VerifyApplySelectionUsesPreviousSelectionsForReassertAndCacheReset()
    {
        var directory = Directory.CreateTempSubdirectory("sc-apply-prev-");
        var harmony = new Harmony("Gurio.SkinChanger.Tests.ApplySelectionPrev");
        var oldCatalog = SkinService.Catalog;
        var oldConfig = SkinService.Config;
        var reassertField = AccessTools.Field(typeof(SkinService), "_needsFullOverlayReassert")
                            ?? throw new InvalidOperationException("找不到 SkinService._needsFullOverlayReassert。");
        var oldReassert = (bool)reassertField.GetValue(null)!;
        try
        {
            var sceneBytes = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Visuals\" type=\"Node2D\"]\n");
            var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = sceneBytes,
                ["res://scenes/creature_visuals/gremlin_nob.tscn"] = sceneBytes
            };
            var multiPackFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"MultiJaw\" type=\"Node2D\"]\n"),
                ["res://scenes/creature_visuals/gremlin_nob.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"MultiNob\" type=\"Node2D\"]\n")
            };
            var soloPackFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://scenes/creature_visuals/jaw_worm.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"SoloJaw\" type=\"Node2D\"]\n")
            };

            var gamePath = Path.Combine(directory.FullName, "game.pck");
            var multiPath = Path.Combine(directory.FullName, "multi.pck");
            var soloPath = Path.Combine(directory.FullName, "solo.pck");
            PckArchive.Write(gamePath, baseline);
            PckArchive.Write(multiPath, multiPackFiles);
            PckArchive.Write(soloPath, soloPackFiles);

            var descriptors = new[]
            {
                new SkinModDescriptor("MultiFullPack", "MultiFullPack", multiPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: true),
                new SkinModDescriptor("SoloSkin", "SoloSkin", soloPath, AffectsGameplay: false, RootPath: directory.FullName, HasDll: false)
            };
            using var catalog = SkinCatalog.Build(gamePath, descriptors);
            Require(catalog.ProviderUsesFullRuntime("MultiFullPack") &&
                    catalog.GetFullRuntimeProviderGroups("MultiFullPack").Count >= 2,
                $"测试夹具中的 MultiFullPack 必须是多分组完整运行提供者，实际分组数={catalog.GetFullRuntimeProviderGroups("MultiFullPack").Count}。");

            var config = new SkinConfig();
            config.Selections["jaw_worm"] = "MultiFullPack";
            config.Selections["gremlin_nob"] = "MultiFullPack";

            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, catalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, config);
            reassertField.SetValue(null, false);

            // Register a live dynamic assembly whose static _hurtSkelCache backs MultiFullPack.
            var fakeAssembly = BuildSyntheticCacheAssembly(out var cacheField);
            var cacheDict = (Dictionary<string, object>)cacheField.GetValue(null)!;
            cacheDict["jaw_worm_hurt"] = new object();
            _stubActiveMultiAssembly = fakeAssembly;
            _stubConfigPath = Path.Combine(directory.FullName, "config.json");

            _suppressMountOverlay = true;
            harmony.Patch(
                AccessTools.Method(typeof(ManagedSkinModLoader), nameof(ManagedSkinModLoader.GetActiveProviderAssembly)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubGetActiveProviderAssembly)));
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(SkinService), "ConfigPath"),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(StubConfigPath)));
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "MountOverlay", [typeof(IReadOnlySet<string>), typeof(bool)]),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(SuppressMountOverlay)));

            var applied = SkinService.ApplySelection("jaw_worm", "SoloSkin");
            Require(applied, "切换到 SoloSkin 应成功：" + SkinService.LastError);

            Require((bool)reassertField.GetValue(null)!,
                "替换多分组完整运行提供者（MultiFullPack -> SoloSkin）时必须触发 RequestFullOverlayReassert。");
            Require(cacheDict.Count == 0,
                "切换皮肤时 ProviderRuntimeCacheReset 必须拿到旧选择（MultiFullPack）并清空其静态缓存。");

            // Unchanged switch must NOT re-trigger RequestFullOverlayReassert.
            reassertField.SetValue(null, false);
            var sameApplied = SkinService.ApplySelection("jaw_worm", "SoloSkin");
            Require(sameApplied && !(bool)reassertField.GetValue(null)!,
                "同选项重复应用不应错误触发 RequestFullOverlayReassert。");

            // Direct helper contract check for transition options containing both outgoing and incoming.
            var prev = new Dictionary<string, (bool HadValue, string? Value)>(StringComparer.OrdinalIgnoreCase)
            {
                ["jaw_worm"] = (true, "MultiFullPack"),
                ["gremlin_nob"] = (true, "MultiFullPack")
            };
            var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["jaw_worm"] = "SoloSkin",
                ["gremlin_nob"] = SkinCatalog.BaseOptionId
            };
            var transitionOptions = SkinService.CollectSelectionTransitionOptions(updates, prev);
            Require(transitionOptions.Contains("MultiFullPack", StringComparer.OrdinalIgnoreCase) &&
                    transitionOptions.Contains("SoloSkin", StringComparer.OrdinalIgnoreCase) &&
                    transitionOptions.Contains(SkinCatalog.BaseOptionId, StringComparer.OrdinalIgnoreCase),
                "CollectSelectionTransitionOptions 必须同时包含切出旧选择与切入新选择。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            _suppressMountOverlay = false;
            _stubActiveMultiAssembly = null;
            _stubConfigPath = null;
            reassertField.SetValue(null, oldReassert);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Catalog)).SetValue(null, oldCatalog);
            AccessTools.Property(typeof(SkinService), nameof(SkinService.Config)).SetValue(null, oldConfig);
            FakeActiveHurtCache.Clear();
            directory.Delete(true);
        }
    }

    private static void VerifyNewlyGuardedHooksIsolateExceptions()
    {
        var harmony = new Harmony("Gurio.SkinChanger.Tests.NewlyGuardedHooks");
        var uiLogs = new List<string>();
        var pendingField = AccessTools.Field(typeof(FakeMerchantBattleFigure), "_shopFigure")
                           ?? throw new InvalidOperationException("找不到 FakeMerchantBattleFigure._shopFigure。");
        var oldPending = pendingField.GetValue(null);
        try
        {
            UiInjectionGuard.ResetForTests();
            UiInjectionGuard.ErrorLoggerForTests = uiLogs.Add;

            _throwInLeave = true;
            _throwInContinueRun = true;
            _throwInCapture = true;
            _throwInApply = true;
            _throwInCurrentEncounter = true;

            harmony.Patch(AccessTools.Method(typeof(CharacterSelectUiScope), nameof(CharacterSelectUiScope.Leave)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(ThrowInLeave)));
            harmony.Patch(AccessTools.Method(typeof(ContextualSkinControls), nameof(ContextualSkinControls.RefreshContinueRunPortrait)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(ThrowInContinueRun)));
            harmony.Patch(AccessTools.Method(typeof(FakeMerchantBattleFigure), nameof(FakeMerchantBattleFigure.Capture)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(ThrowInCapture)));
            harmony.Patch(AccessTools.Method(typeof(FakeMerchantBattleFigure), nameof(FakeMerchantBattleFigure.Apply)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(ThrowInApply)));
            harmony.Patch(AccessTools.Method(typeof(BackgroundSkinService), nameof(BackgroundSkinService.ApplyToRoom)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(NoopApplyToRoom)));
            harmony.Patch(AccessTools.Method(typeof(BackgroundSkinService), nameof(BackgroundSkinService.CurrentEncounter)),
                prefix: new HarmonyMethod(typeof(PostReviewFixesTests), nameof(ThrowInCurrentEncounter)));

            // 1) CharacterSelectUiClosePatch.Prefix
            AccessTools.Method(typeof(CharacterSelectUiClosePatch), "Prefix")!.Invoke(null, [null]);
            Require(UiInjectionGuard.GetFailureCount("CharacterSelectUiClosePatch.Prefix") == 1,
                "CharacterSelectUiClosePatch.Prefix 必须经 UiInjectionGuard 隔离异常并计数。");

            // 2) ContinueRunPortraitPatch.Postfix
            AccessTools.Method(typeof(ContinueRunPortraitPatch), "Postfix")!.Invoke(null, [null, null]);
            Require(UiInjectionGuard.GetFailureCount("ContinueRunPortraitPatch.Postfix") == 1,
                "ContinueRunPortraitPatch.Postfix 必须经 UiInjectionGuard 隔离异常并计数。");

            // 3) FakeMerchantBattleFigureCapturePatch.Prefix
            AccessTools.Method(typeof(FakeMerchantBattleFigureCapturePatch), "Prefix")!.Invoke(null, [null]);

            // 4) FakeMerchantBattleFigureReadyPatch.Postfix (with pending figure active so hot path enters Apply)
            pendingField.SetValue(null, FakeGodot<Node2D>());
            AccessTools.Method(typeof(FakeMerchantBattleFigureReadyPatch), "Postfix")!.Invoke(null, [null]);

            // 5) BackgroundSkinRoomPatch.Postfix
            AccessTools.Method(typeof(BackgroundSkinRoomPatch), "Postfix")!.Invoke(null, [null, null]);
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            pendingField.SetValue(null, oldPending);
            _throwInLeave = false;
            _throwInContinueRun = false;
            _throwInCapture = false;
            _throwInApply = false;
            _throwInCurrentEncounter = false;
            UiInjectionGuard.ResetForTests();
        }
    }

    private static void VerifyFakeMerchantBackgroundNudgeRequiresDebugEnv()
    {
        var previous = System.Environment.GetEnvironmentVariable("STS2SC_DEBUG");
        try
        {
            System.Environment.SetEnvironmentVariable("STS2SC_DEBUG", null);
            Require(!FakeMerchantBackgroundNudge.IsDebugEnabled(),
                "未设置 STS2SC_DEBUG 时 IsDebugEnabled 必须为 false。");

            // Passing an uninitialized NCombatBackground would crash/throw if Attach tried to call
            // GetNodeOrNull or AddChild; returning early proves no node is inspected or attached.
            var dummyBackground = FakeGodot<NCombatBackground>();
            FakeMerchantBackgroundNudge.Attach(dummyBackground);

            foreach (var disabledValue in new[] { "", "0", "true", "yes" })
            {
                System.Environment.SetEnvironmentVariable("STS2SC_DEBUG", disabledValue);
                Require(!FakeMerchantBackgroundNudge.IsDebugEnabled(),
                    $"STS2SC_DEBUG={disabledValue} 时不得开启背景微调。");
                FakeMerchantBackgroundNudge.Attach(dummyBackground);
            }

            System.Environment.SetEnvironmentVariable("STS2SC_DEBUG", "1");
            Require(FakeMerchantBackgroundNudge.IsDebugEnabled(),
                "仅当 STS2SC_DEBUG=1 时 IsDebugEnabled 才应为 true。");

            var attachMethod = AccessTools.Method(typeof(FakeMerchantBackgroundNudge), nameof(FakeMerchantBackgroundNudge.Attach))
                               ?? throw new InvalidOperationException("找不到 FakeMerchantBackgroundNudge.Attach。");
            var calledMethods = PatchProcessor.GetOriginalInstructions(attachMethod)
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .Select(m => m.Name)
                .ToArray();
            var checkIdx = Array.IndexOf(calledMethods, nameof(FakeMerchantBackgroundNudge.IsDebugEnabled));
            var addChildIdx = Array.IndexOf(calledMethods, nameof(Node.AddChild));
            Require(checkIdx >= 0 && addChildIdx > checkIdx,
                "FakeMerchantBackgroundNudge.Attach 必须在 AddChild 之前检查 IsDebugEnabled()。");
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("STS2SC_DEBUG", previous);
        }
    }

    private static EventModel CreateSyntheticEventModel(string entryId)
    {
        var eventType = typeof(EventModel).Assembly.GetTypes()
                            .First(type => !type.IsAbstract &&
                                           typeof(EventModel).IsAssignableFrom(type) &&
                                           !typeof(AncientEventModel).IsAssignableFrom(type) &&
                                           type.GetProperty("InitialPortraitPath",
                                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) == null);
        var model = (EventModel)RuntimeHelpers.GetUninitializedObject(eventType);
        AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField")!
            .SetValue(model, new ModelId("EVENT", entryId));
        return model;
    }

    private static void AddSyntheticEventTexture(Dictionary<string, byte[]> files, string path, byte marker)
    {
        var imported = "res://.godot/imported/" + path["res://".Length..].Replace('/', '-') + $"_{marker}.ctex";
        files[path + ".import"] = Encoding.UTF8.GetBytes($"[remap]\npath=\"{imported}\"\n");
        var bytes = new byte[53];
        "GST2"u8.CopyTo(bytes);
        bytes[40] = 1;
        bytes[42] = 1;
        bytes[52] = marker;
        files[imported] = bytes;
    }

    private static Assembly BuildSyntheticCacheAssembly(out FieldInfo cacheField)
    {
        var assemblyName = new AssemblyName("SyntheticCacheResetProvider_" + Guid.NewGuid().ToString("N"));
        var ab = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var mb = ab.DefineDynamicModule("Main");
        var tb = mb.DefineType("FakeMultiPack.HalfHpPatch", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);
        var fb = tb.DefineField("_hurtSkelCache", typeof(Dictionary<string, object>), FieldAttributes.Public | FieldAttributes.Static);
        var created = tb.CreateType()!;
        cacheField = created.GetField("_hurtSkelCache", BindingFlags.Public | BindingFlags.Static)!;
        cacheField.SetValue(null, new Dictionary<string, object>(StringComparer.Ordinal));
        return created.Assembly;
    }

    private static bool StubGetOrLoadRuntimeScene(string groupId, string scenePath, ref PackedScene __result)
    {
        _runtimeSceneLoadCalls++;
        __result = _stubReplacementScene!;
        return false;
    }

    private static bool StubGetActiveProviderAssembly(string providerId, ref Assembly? __result)
    {
        if (StubActiveAssembliesByProvider.TryGetValue(providerId, out var mapped))
        {
            __result = mapped;
            return false;
        }

        if (_stubActiveMultiAssembly != null &&
            string.Equals(providerId, "MultiFullPack", StringComparison.OrdinalIgnoreCase))
        {
            __result = _stubActiveMultiAssembly;
            return false;
        }

        return true;
    }

    private static bool StubConfigPath(ref string __result)
    {
        if (_stubConfigPath != null)
        {
            __result = _stubConfigPath;
            return false;
        }

        return true;
    }

    private static bool SuppressMountOverlay() => !_suppressMountOverlay;

    private static bool CountAndSuppressMountOverlay()
    {
        _mountOverlayCalls++;
        return !_suppressMountOverlay;
    }

    private static bool FailIfPrepareRuntimeOverlayCalled()
    {
        _prepareOverlayCalls++;
        throw new InvalidOperationException("仅文本事件路径不得调用 GetOrPrepareRuntimeOverlay 生成私有包。");
    }

    private static bool CountRuntimeResourceLoad(ref Resource __result)
    {
        _runtimeResourceLoadCalls++;
        __result = _stubLoadedEventTexture!;
        return false;
    }

    private static bool StubEventLoadTexture(ref Texture2D __result)
    {
        _eventLoadTextureCalls++;
        __result = _stubLoadedEventTexture!;
        return false;
    }

    private static bool StubCurrentLanguage(ref string __result)
    {
        if (_stubLanguage != null)
        {
            __result = _stubLanguage;
            return false;
        }

        return true;
    }
    private static bool ThrowInLeave() => _throwInLeave ? throw new InvalidOperationException("leave-boom") : true;
    private static bool ThrowInContinueRun() => _throwInContinueRun ? throw new InvalidOperationException("continue-boom") : true;
    private static bool ThrowInCapture() => _throwInCapture ? throw new InvalidOperationException("capture-boom") : true;
    private static bool ThrowInApply() => _throwInApply ? throw new InvalidOperationException("apply-boom") : true;
    private static bool NoopApplyToRoom() => false;
    private static bool ThrowInCurrentEncounter() => _throwInCurrentEncounter ? throw new InvalidOperationException("encounter-boom") : true;

    private static T FakeGodot<T>() where T : class
    {
        var obj = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        GC.SuppressFinalize(obj);
        return obj;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
