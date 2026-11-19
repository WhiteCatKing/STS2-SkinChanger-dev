using System.Text;
using Godot;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

// #195/#197：覆盖 AncientWaifus 三类回归：
// 1. 遗物：waifu_assets/<ancient>/... 下各先古按自身分组隔离，独立 PNG 切片（无共享 relic_atlas.png）
//    能产出 RelicSpriteTextures 与隔离别名，且大图标 BigIcon 路径正确派生与归属。
// 2. 时间线：orobas_epoch / silent7_epoch (tezcatara) / defect7_epoch (darv) 按各自先古选择生效，
//    且不进入全局 visual 覆盖包、独立隔离加载（缩略图与大图一致跟随选择）。
// 3. 交互触摸：InteractiveTouchBoxBridge 仅接管隔离运行包场景，轴向映射、范围裁剪与优先级判定准确。
internal static class AncientWaifusSupportTests
{
    private const string BaselineRelicAtlas = "res://images/atlases/relic_atlas.png";
    private const string BaselineRelicOutlineAtlas = "res://images/atlases/relic_outline_atlas.png";
    private const string RunicSprite = "res://images/atlases/relic_atlas.sprites/runic_pyramid.tres";
    private const string RunicOutlineSprite = "res://images/atlases/relic_outline_atlas.sprites/runic_pyramid.tres";
    private const string RunicBigIcon = "res://images/relics/runic_pyramid.png";
    private const string PaelsLegionSprite = "res://images/atlases/relic_atlas.sprites/paels_legion.tres";
    private const string PaelsLegionBigIcon = "res://images/relics/paels_legion.png";
    private const string OrobasEpochPortrait = "res://images/timeline/epoch_portraits/orobas_epoch.png";
    private const string Silent7EpochPortrait = "res://images/timeline/epoch_portraits/silent7_epoch.png";
    private const string Defect7EpochPortrait = "res://images/timeline/epoch_portraits/placeholder/defect7_epoch.png";

    private const string BaselineRelicSpriteText =
        "[gd_resource type=\"AtlasTexture\" load_steps=2 format=3]\n\n" +
        "[ext_resource type=\"Texture2D\" path=\"res://images/atlases/relic_atlas.png\" id=\"1\"]\n\n" +
        "[resource]\natlas = ExtResource(\"1\")\nregion = Rect2(10, 20, 64, 64)\n";

    private const string BaselineRelicOutlineSpriteText =
        "[gd_resource type=\"AtlasTexture\" load_steps=2 format=3]\n\n" +
        "[ext_resource type=\"Texture2D\" path=\"res://images/atlases/relic_outline_atlas.png\" id=\"1\"]\n\n" +
        "[resource]\natlas = ExtResource(\"1\")\nregion = Rect2(10, 20, 64, 64)\n";

    internal static void Run()
    {
        VerifyRelicStandaloneAndPerAncientOwnership();
        VerifyTimelineEpochOwnershipAndIsolation();
        VerifyInteractiveTouchBoxPolicies();
        Console.WriteLine("AncientWaifus support policy tests passed.");
    }

    private static void VerifyRelicStandaloneAndPerAncientOwnership()
    {
        using var fixture = CreateAncientWaifusFixture();
        var catalog = fixture.Catalog;

        Require(
            RelicSkinResources.TryDeriveBigIconPath(RunicSprite) == RunicBigIcon,
            "PackedIconPath 必须正确派生出对应的 res://images/relics/<name>.png 大图标路径。");
        Require(
            RelicSkinResources.TryDeriveBigIconPath("res://images/other/foo.tres") == null,
            "非 relic_atlas.sprites 路径不能误派生遗物大图标路径。");

        // 只选 darv=AncientWaifus，pael 保持原版：darv 的遗物必须命中，pael 的不能串台。
        var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["darv"] = "AncientWaifus",
            ["pael"] = SkinCatalog.BaseOptionId,
            ["orobas"] = SkinCatalog.BaseOptionId,
            ["tezcatara"] = SkinCatalog.BaseOptionId,
        };

        Require(
            catalog.FindSelectedRelicIconGroup(RunicSprite, selections, []) == "darv",
            "选中 darv=AncientWaifus 时，runic_pyramid 小图标必须归属 darv。");
        Require(
            catalog.FindSelectedRelicIconGroup(RunicOutlineSprite, selections, []) == "darv",
            "选中 darv=AncientWaifus 时，runic_pyramid 描边必须归属 darv。");
        Require(
            catalog.FindSelectedRelicIconGroup(RunicBigIcon, selections, []) == "darv",
            "选中 darv=AncientWaifus 时，runic_pyramid 大图标必须归属 darv。");
        Require(
            catalog.FindSelectedRelicIconGroup(PaelsLegionSprite, selections, []) == null,
            "pael 未选 AncientWaifus 时，绝不能因为 darv 选了 AncientWaifus 就串出 paels_legion 小图标。");
        Require(
            catalog.FindSelectedRelicIconGroup(PaelsLegionBigIcon, selections, []) == null,
            "pael 未选 AncientWaifus 时，绝不能串出 paels_legion 大图标。");
        Require(
            catalog.FindRelicIconOwnerGroup(PaelsLegionSprite) == "pael",
            "paels_legion 的归属先古组必须是 pael。");

        var darvGroup = catalog.Groups.Single(g => g.Id == "darv");
        var darvOption = darvGroup.Options.Single(o => o.Id == "AncientWaifus");
        var darvRelicPaths = catalog.GetProviderRelicResourcePaths("darv", darvOption);
        Require(
            darvRelicPaths.Contains(RunicSprite, StringComparer.OrdinalIgnoreCase) &&
            darvRelicPaths.Contains(RunicOutlineSprite, StringComparer.OrdinalIgnoreCase) &&
            darvRelicPaths.Contains(RunicBigIcon, StringComparer.OrdinalIgnoreCase),
            "darv 的遗物资源列表必须包含 runic_pyramid 切片、描边切片与大图标。");
        Require(
            !darvRelicPaths.Contains(PaelsLegionSprite, StringComparer.OrdinalIgnoreCase),
            "darv 的遗物资源列表不得混入 pael 的遗物。");

        var overlay = catalog.BuildIsolatedRelicResourceOverlay(
            "darv",
            "AncientWaifus",
            darvRelicPaths,
            "tok-darv");
        Require(
            overlay.CanonicalDependencyPaths.Count == 0,
            "隔离遗物包不得写入任何规范路径重定向，避免污染全局图集缓存。");
        Require(
            overlay.RelicSpriteTextures.TryGetValue(RunicSprite, out var normalTexDep) &&
            normalTexDep.Equals("res://waifu_assets/darv/images/relics/runic_pyramid.png", StringComparison.OrdinalIgnoreCase),
            $"runic_pyramid 切片必须映射到其独立 PNG 依赖，实际为 {normalTexDep ?? "null"}。");
        Require(
            overlay.RelicSpriteTextures.TryGetValue(RunicOutlineSprite, out var outlineTexDep) &&
            outlineTexDep.Equals("res://waifu_assets/darv/images/relics/runic_pyramid_outline.png", StringComparison.OrdinalIgnoreCase),
            $"runic_pyramid 描边切片必须映射到其独立描边 PNG 依赖，实际为 {outlineTexDep ?? "null"}。");
        Require(
            overlay.ResourcePaths.ContainsKey(normalTexDep!) &&
            overlay.ResourcePaths.ContainsKey(outlineTexDep!) &&
            overlay.ResourcePaths.ContainsKey(RunicBigIcon),
            "隔离遗物包必须为独立遗物 PNG、描边 PNG 与大图标生成独立别名路径。");
    }

    private static void VerifyTimelineEpochOwnershipAndIsolation()
    {
        using var fixture = CreateAncientWaifusFixture();
        var catalog = fixture.Catalog;

        var onlyOrobas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["orobas"] = "AncientWaifus",
            ["tezcatara"] = SkinCatalog.BaseOptionId,
            ["darv"] = SkinCatalog.BaseOptionId,
            ["pael"] = SkinCatalog.BaseOptionId,
        };

        var orobasMatch = catalog.FindSelectedEpochPortrait("OROBAS_EPOCH", onlyOrobas, []);
        Require(
            orobasMatch != null &&
            orobasMatch.GroupId == "orobas" &&
            orobasMatch.CanonicalPortraitPath.Equals(OrobasEpochPortrait, StringComparison.OrdinalIgnoreCase),
            "选中 orobas=AncientWaifus 时，OROBAS_EPOCH 必须命中 orobas 分组。");
        Require(
            catalog.FindSelectedEpochPortrait("SILENT7_EPOCH", onlyOrobas, []) == null,
            "tezcatara 未选 AncientWaifus 时，SILENT7_EPOCH（特兹卡塔拉的汤）不得替换。");
        Require(
            catalog.FindSelectedEpochPortrait("DEFECT7_EPOCH", onlyOrobas, []) == null,
            "darv 未选 AncientWaifus 时，DEFECT7_EPOCH（达弗）不得替换。");

        var allThree = new Dictionary<string, string>(onlyOrobas, StringComparer.OrdinalIgnoreCase)
        {
            ["tezcatara"] = "AncientWaifus",
            ["darv"] = "AncientWaifus",
        };
        var silent7Match = catalog.FindSelectedEpochPortrait("SILENT7_EPOCH", allThree, []);
        var defect7Match = catalog.FindSelectedEpochPortrait("DEFECT7_EPOCH", allThree, []);
        Require(
            silent7Match != null &&
            silent7Match.GroupId == "tezcatara" &&
            silent7Match.CanonicalPortraitPath.Equals(Silent7EpochPortrait, StringComparison.OrdinalIgnoreCase),
            "选中 tezcatara=AncientWaifus 时，SILENT7_EPOCH 必须命中 tezcatara 分组。");
        Require(
            defect7Match != null &&
            defect7Match.GroupId == "darv" &&
            defect7Match.CanonicalPortraitPath.Equals(Defect7EpochPortrait, StringComparison.OrdinalIgnoreCase),
            "选中 darv=AncientWaifus 时，DEFECT7_EPOCH 必须命中 darv 分组并保留 placeholder 路径。");

        // 全局覆盖包不得挂载提供者的时间线大图或 epoch_atlas（允许保留/还原游戏基线文件），
        // 避免切回原版后被 AssetCache 粘住或 Beta/正式版图集错位。
        var globalOverlay = catalog.BuildOverlay(allThree);
        Require(
            !globalOverlay.Any(pair =>
                (SkinCatalog.IsEpochPortraitPath(pair.Key.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ? pair.Key[..^7] : pair.Key) ||
                 SkinCatalog.IsEpochAtlasPath(pair.Key.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ? pair.Key[..^7] : pair.Key)) &&
                pair.Value.ArchivePath.EndsWith("AncientWaifus.pck", StringComparison.OrdinalIgnoreCase)),
            "全局覆盖包不得挂载提供者的 epoch_portraits 或 epoch_atlas，时间线必须走独立隔离路径。");

        var epochOverlay = catalog.BuildIsolatedEpochPortraitOverlay(
            "orobas",
            "AncientWaifus",
            "OROBAS_EPOCH",
            "tok-epoch");
        Require(
            epochOverlay.CanonicalDependencyPaths.Count == 0 &&
            epochOverlay.ResourcePaths.TryGetValue(OrobasEpochPortrait, out var alias) &&
            alias.StartsWith("res://sts2_skin_runtime/tok-epoch/", StringComparison.OrdinalIgnoreCase) &&
            epochOverlay.Files.ContainsKey(alias + ".import"),
            "时间线独立隔离包必须为选中的 epoch 大图生成私有别名 .import 且不污染规范路径。");
    }

    private static void VerifyInteractiveTouchBoxPolicies()
    {
        Require(
            InteractiveTouchBoxBridge.IsManagedTouchScenePath(
                "res://sts2_skin_runtime/session/042/scenes/events/background_scenes/orobas.tscn"),
            "隔离运行包路径必须被 InteractiveTouchBoxBridge 识别为受管触摸场景。");
        Require(
            !InteractiveTouchBoxBridge.IsManagedTouchScenePath("res://scenes/events/background_scenes/orobas.tscn") &&
            !InteractiveTouchBoxBridge.IsManagedTouchScenePath("res://waifu_assets/orobas/scenes/events/background_scenes/orobas.tscn") &&
            !InteractiveTouchBoxBridge.IsManagedTouchScenePath(null),
            "原版或第三方非受管路径不得被 InteractiveTouchBoxBridge 接管。");

        Require(InteractiveTouchBoxBridge.IsTouchBoxName("Touch_Box_Head"), "Touch_Box_ 前缀必须匹配。");
        Require(!InteractiveTouchBoxBridge.IsTouchBoxName("SpineSprite"), "非 Touch_Box_ 节点不得匹配。");

        Require(
            InteractiveTouchBoxBridge.MapDragOffsetToBoneAxes("Direct", 12f, -5f) == (12f, -5f) &&
            InteractiveTouchBoxBridge.MapDragOffsetToBoneAxes("DirectInverted", 12f, -5f) == (-12f, 5f) &&
            InteractiveTouchBoxBridge.MapDragOffsetToBoneAxes("Rotated90Inverted", 12f, -5f) == (-5f, 12f) &&
            InteractiveTouchBoxBridge.MapDragOffsetToBoneAxes("Rotated90", 12f, -5f) == (5f, -12f),
            "四种 DragAxisMode 的骨骼轴向映射必须与原包一致。");

        var clampedBox = InteractiveTouchBoxBridge.ClampDragOffset(
            new Vector2(500f, -500f),
            hasDragLimitBox: true,
            new Vector2(-40f, -30f),
            new Vector2(60f, 80f),
            maxDragRadius: 200f);
        Require(clampedBox == new Vector2(60f, -30f), "存在 Drag_Limit 时必须按矩形范围裁剪拖拽偏移。");

        var clampedRadius = InteractiveTouchBoxBridge.ClampDragOffset(
            new Vector2(300f, 400f),
            hasDragLimitBox: false,
            Vector2.Zero,
            Vector2.Zero,
            maxDragRadius: 100f);
        Require(
            MathF.Abs(clampedRadius.X - 60f) < 0.01f && MathF.Abs(clampedRadius.Y - 80f) < 0.01f,
            "无 Drag_Limit 时必须按 MaxDragRadius 圆形半径裁剪。");

        Require(
            InteractiveTouchBoxBridge.ShouldPreferCandidate(2, 0, 1, 99) &&
            InteractiveTouchBoxBridge.ShouldPreferCandidate(1, 5, 1, 4) &&
            !InteractiveTouchBoxBridge.ShouldPreferCandidate(0, 99, 1, 0),
            "重叠触摸区域必须按 Priority 优先、同 Priority 再按 ZIndex 判定。");

        // #202 item 1: no global SceneTree.NodeAdded installation entry point.
        Require(
            typeof(InteractiveTouchBoxBridge).GetMethod(
                "Install",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic) == null,
            "InteractiveTouchBoxBridge 不应再保留全局 SceneTree.NodeAdded 监听入口 Install。");

        // #202 item 2: bone callback fast-skip and release snap-to-zero settling.
        Require(
            !InteractiveTouchBoxBridge.NeedsBoneUpdate(hasTargetBone: false, isHolding: true, new Vector2(50f, 50f)) &&
            !InteractiveTouchBoxBridge.NeedsBoneUpdate(hasTargetBone: true, isHolding: false, Vector2.Zero) &&
            !InteractiveTouchBoxBridge.NeedsBoneUpdate(hasTargetBone: true, isHolding: false, new Vector2(0.05f, 0.05f)) &&
            InteractiveTouchBoxBridge.NeedsBoneUpdate(hasTargetBone: true, isHolding: true, Vector2.Zero) &&
            InteractiveTouchBoxBridge.NeedsBoneUpdate(hasTargetBone: true, isHolding: false, new Vector2(10f, 0f)),
            "骨骼拖拽空闲时必须快速跳过 GetSkeleton，仅在按住或回弹未归零时进入更新。");

        var (holdNext, holdApply) = InteractiveTouchBoxBridge.ComputeNextDragOffset(
            Vector2.Zero,
            new Vector2(100f, 0f),
            dragSpeed: 0.25f,
            isHolding: true);
        Require(
            holdApply && MathF.Abs(holdNext.X - 25f) < 0.01f && MathF.Abs(holdNext.Y) < 0.01f,
            "拖拽按住期间应平滑插值并标记需要施加骨骼偏移。");

        var (settleNext, settleApply) = InteractiveTouchBoxBridge.ComputeNextDragOffset(
            new Vector2(0.05f, 0f),
            Vector2.Zero,
            dragSpeed: 0.5f,
            isHolding: false);
        Require(
            !settleApply && settleNext == Vector2.Zero,
            "松手回弹衰减到阈值以下时应立即归零，避免后续空帧继续唤醒 GetSkeleton。");

        Require(
            MathF.Abs(InteractiveTouchBoxBridge.ResolveIntroDurationSeconds("1.75") - 1.75f) < 0.001f &&
            MathF.Abs(InteractiveTouchBoxBridge.ResolveIntroDurationSeconds("0") - 0f) < 0.001f &&
            MathF.Abs(InteractiveTouchBoxBridge.ResolveIntroDurationSeconds("-1") - InteractiveTouchBoxBridge.DefaultIntroDurationSeconds) < 0.001f &&
            MathF.Abs(InteractiveTouchBoxBridge.ResolveIntroDurationSeconds(null) - InteractiveTouchBoxBridge.DefaultIntroDurationSeconds) < 0.001f,
            "IntroDuration 解析必须兼容有效浮点、0 秒及非法回退默认值。");

        // #203 item 1: CznStyleUI RelicCatalogProvider bridge target resolution.
        var (catalogMethod, detailMethod) = CznRelicCollectionCompat.FindTargetMethods(typeof(MockCznRelicCatalogProvider));
        Require(
            catalogMethod != null && detailMethod != null,
            "CznRelicCollectionCompat 必须能反射定位 ResolveCatalogIcon 与 ResolveDetailIcon。");

        var (missingCatalog, missingDetail) = CznRelicCollectionCompat.FindTargetMethods(typeof(object));
        Require(
            missingCatalog == null && missingDetail == null,
            "未安装 CznStyleUI 或目标签名不存在时，反射定位必须安全返回 null。");
    }

    private static class MockCznRelicCatalogProvider
    {
        private static Texture2D? ResolveCatalogIcon(MegaCrit.Sts2.Core.Models.RelicModel model) => null;
        private static Texture2D? ResolveDetailIcon(MegaCrit.Sts2.Core.Models.RelicModel model) => null;
    }

    private static byte[] MakeFakeCtex(ushort width, ushort height, byte tag)
    {
        var bytes = new byte[56];
        Encoding.ASCII.GetBytes("GST2").CopyTo(bytes, 0);
        BitConverter.GetBytes(1u).CopyTo(bytes, 4);
        BitConverter.GetBytes((uint)width).CopyTo(bytes, 8);
        BitConverter.GetBytes((uint)height).CopyTo(bytes, 12);
        BitConverter.GetBytes(width).CopyTo(bytes, 40);
        BitConverter.GetBytes(height).CopyTo(bytes, 42);
        bytes[55] = tag;
        return bytes;
    }

    private static FixtureScope CreateAncientWaifusFixture()
    {
        var root = Directory.CreateTempSubdirectory("sc-ancient-waifus-");
        var game = Path.Combine(root.FullName, "game.pck");
        PckArchive.Write(game, new Dictionary<string, byte[]>
        {
            ["res://scenes/events/background_scenes/darv.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://scenes/events/background_scenes/pael.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://scenes/events/background_scenes/orobas.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://scenes/events/background_scenes/tezcatara.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            [BaselineRelicAtlas] = [1, 2, 3],
            [BaselineRelicOutlineAtlas] = [4, 5, 6],
            [RunicSprite] = Encoding.UTF8.GetBytes(BaselineRelicSpriteText),
            [RunicOutlineSprite] = Encoding.UTF8.GetBytes(BaselineRelicOutlineSpriteText),
            [PaelsLegionSprite] = Encoding.UTF8.GetBytes(BaselineRelicSpriteText),
            [OrobasEpochPortrait + ".import"] = Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/base_orobas_epoch.ctex\"\n"),
            ["res://.godot/imported/base_orobas_epoch.ctex"] = MakeFakeCtex(1620, 1000, 1),
            [Silent7EpochPortrait + ".import"] = Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/base_silent7_epoch.ctex\"\n"),
            ["res://.godot/imported/base_silent7_epoch.ctex"] = MakeFakeCtex(1620, 1000, 2),
            [Defect7EpochPortrait + ".import"] = Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/base_defect7_epoch.ctex\"\n"),
            ["res://.godot/imported/base_defect7_epoch.ctex"] = MakeFakeCtex(1620, 1000, 3),
        });

        // 模拟真实 AncientWaifus.pck：二进制 .res 切片引用私有 waifu_assets/<ancient>/images/relics/*.png，无共享 relic_atlas.png。
        static byte[] FakeBinaryRes(string texturePath, string spritePath) =>
            Encoding.UTF8.GetBytes($"RSRC\0AtlasTexture\0{texturePath}\0{spritePath}\0RSRC");

        var pack = Path.Combine(root.FullName, "AncientWaifus.pck");
        PckArchive.Write(pack, new Dictionary<string, byte[]>
        {
            ["res://waifu_assets/darv/scenes/events/background_scenes/darv.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://waifu_assets/pael/scenes/events/background_scenes/pael.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://waifu_assets/orobas/scenes/events/background_scenes/orobas.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),
            ["res://waifu_assets/tezcatara/scenes/events/background_scenes/tezcatara.tscn"] = Encoding.UTF8.GetBytes("[gd_scene format=3]\n"),

            ["res://waifu_assets/darv/images/atlases/relic_atlas.sprites/runic_pyramid.tres.remap"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/exported/133200997/export-darv-runic.res\"\n"),
            ["res://.godot/exported/133200997/export-darv-runic.res"] = FakeBinaryRes(
                "res://waifu_assets/darv/images/relics/runic_pyramid.png",
                "res://waifu_assets/darv/images/atlases/relic_atlas.sprites/runic_pyramid.tres"),
            ["res://waifu_assets/darv/images/atlases/relic_outline_atlas.sprites/runic_pyramid.tres.remap"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/exported/133200997/export-darv-runic-outline.res\"\n"),
            ["res://.godot/exported/133200997/export-darv-runic-outline.res"] = FakeBinaryRes(
                "res://waifu_assets/darv/images/relics/runic_pyramid_outline.png",
                "res://waifu_assets/darv/images/atlases/relic_outline_atlas.sprites/runic_pyramid.tres"),
            ["res://waifu_assets/darv/images/relics/runic_pyramid.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/runic_pyramid.png-aw.ctex\"\n"),
            ["res://.godot/imported/runic_pyramid.png-aw.ctex"] = MakeFakeCtex(256, 256, 11),
            ["res://waifu_assets/darv/images/relics/runic_pyramid_outline.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/runic_pyramid_outline.png-aw.ctex\"\n"),
            ["res://.godot/imported/runic_pyramid_outline.png-aw.ctex"] = MakeFakeCtex(256, 256, 12),

            ["res://waifu_assets/pael/images/atlases/relic_atlas.sprites/paels_legion.tres.remap"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/exported/133200997/export-pael-legion.res\"\n"),
            ["res://.godot/exported/133200997/export-pael-legion.res"] = FakeBinaryRes(
                "res://waifu_assets/pael/images/relics/paels_legion.png",
                "res://waifu_assets/pael/images/atlases/relic_atlas.sprites/paels_legion.tres"),
            ["res://waifu_assets/pael/images/relics/paels_legion.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/paels_legion.png-aw.ctex\"\n"),
            ["res://.godot/imported/paels_legion.png-aw.ctex"] = MakeFakeCtex(256, 256, 13),

            ["res://waifu_assets/timeline/images/timeline/epoch_portraits/orobas_epoch.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/orobas_epoch.png-aw.ctex\"\n"),
            ["res://.godot/imported/orobas_epoch.png-aw.ctex"] = MakeFakeCtex(1620, 1000, 21),
            ["res://waifu_assets/timeline/images/timeline/epoch_portraits/silent7_epoch.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/silent7_epoch.png-aw.ctex\"\n"),
            ["res://.godot/imported/silent7_epoch.png-aw.ctex"] = MakeFakeCtex(1620, 1000, 22),
            ["res://waifu_assets/timeline/images/timeline/epoch_portraits/placeholder/defect7_epoch.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/defect7_epoch.png-aw.ctex\"\n"),
            ["res://.godot/imported/defect7_epoch.png-aw.ctex"] = MakeFakeCtex(1620, 1000, 23),
            ["res://waifu_assets/timeline/images/atlases/epoch_atlas.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/epoch_atlas.png-aw.ctex\"\n"),
            ["res://.godot/imported/epoch_atlas.png-aw.ctex"] = MakeFakeCtex(822, 3236, 24),
            ["res://waifu_assets/timeline/images/atlases/epoch_atlas_beta.png.import"] =
                Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/imported/epoch_atlas_beta.png-aw.ctex\"\n"),
            ["res://.godot/imported/epoch_atlas_beta.png-aw.ctex"] = MakeFakeCtex(828, 3788, 25),
        });

        var catalog = SkinCatalog.Build(game, new[]
        {
            new SkinModDescriptor("AncientWaifus", "AncientWaifus", pack, false, root.FullName, HasDll: true)
        });
        return new FixtureScope(root, catalog);
    }

    private sealed class FixtureScope(DirectoryInfo root, SkinCatalog catalog) : IDisposable
    {
        public SkinCatalog Catalog { get; } = catalog;

        public void Dispose()
        {
            Catalog.Dispose();
            try { root.Delete(recursive: true); } catch { }
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value)
        {
            throw new InvalidOperationException(message);
        }
    }
}
