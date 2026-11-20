using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

// 正式版 v0.111.1 只有 res://scenes/rooms/merchant_room.tscn；测试版 v0.113.x 将其拆分为
// standard_merchant_room.tscn 与 blitz_merchant_room.tscn 并删除了旧路径。
// 验证商人场景路径（预热、解析、其它图鉴兜底）始终按实际加载的游戏 PCK 解析，不按分支写死。
internal static class MerchantRoomPathTests
{
    private const string OfficialGamePckEnv = "STS2_OFFICIAL_PCK";
    private const string BetaGamePckEnv = "STS2_BETA_PCK";

    private static readonly string[] GameFolderNames = { "Slay the Spire 2", "Slay Spire 2" };

    private sealed class NBlitzMerchantRoom
    {
    }

    internal static void Run()
    {
        VerifyRealGamePcks();
        VerifyEndToEndCatalogResolution();
        VerifyCallerWiring();
        Console.WriteLine("Merchant room path resolution passed.");
    }

    private static void VerifyRealGamePcks()
    {
        var blitzType = typeof(NMerchantRoom).Assembly.GetType(
                            "MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom") ??
                        typeof(NBlitzMerchantRoom);

        var officialPck = ResolveGamePck(OfficialGamePckEnv, beta: false);
        var betaPck = ResolveGamePck(BetaGamePckEnv, beta: true);

        var verifiedCount = 0;

        if (officialPck is not null)
        {
            using var archive = PckArchive.Open(officialPck);
            bool HasScene(string path) => archive.Contains(path) || archive.Contains(path + ".remap");

            Require(!HasScene(MerchantRuntimeAppearance.StandardMerchantRoomScenePath),
                "正式版 PCK 不应包含测试版拆分后的 standard_merchant_room.tscn。");
            Require(HasScene(MerchantRuntimeAppearance.LegacyMerchantRoomScenePath),
                "正式版 PCK 应包含 merchant_room.tscn。");

            foreach (var hasBlitzBranch in new[] { false, true })
            {
                var normal = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.GroupId,
                    roomType: null,
                    HasScene,
                    hasBlitzBranch);
                Require(normal == MerchantRuntimeAppearance.LegacyMerchantRoomScenePath,
                    $"正式版 PCK 下普通商人应解析到 merchant_room.tscn（hasBlitzBranch={hasBlitzBranch}），实际：{normal}");
                Require(HasScene(normal), $"正式版 PCK 中不存在解析出的普通商人场景：{normal}");

                var blitzFallback = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.GroupId,
                    blitzType,
                    HasScene,
                    hasBlitzBranch);
                Require(blitzFallback == MerchantRuntimeAppearance.LegacyMerchantRoomScenePath,
                    $"正式版 PCK 缺少 blitz 场景时应安全回落到 merchant_room.tscn，实际：{blitzFallback}");
                Require(HasScene(blitzFallback), $"正式版 PCK 中不存在回落商人场景：{blitzFallback}");

                var fake = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.FakeMerchantGroupId,
                    roomType: null,
                    HasScene,
                    hasBlitzBranch);
                Require(HasScene(fake), $"正式版 PCK 中不存在假商人场景：{fake}");
            }

            verifiedCount++;
        }

        if (betaPck is not null)
        {
            using var archive = PckArchive.Open(betaPck);
            bool HasScene(string path) => archive.Contains(path) || archive.Contains(path + ".remap");

            Require(!HasScene(MerchantRuntimeAppearance.LegacyMerchantRoomScenePath),
                "测试版 PCK 不应再包含已删除的 merchant_room.tscn。");
            Require(HasScene(MerchantRuntimeAppearance.StandardMerchantRoomScenePath),
                "测试版 PCK 应包含 standard_merchant_room.tscn。");
            Require(HasScene(MerchantRuntimeAppearance.BlitzMerchantRoomScenePath),
                "测试版 PCK 应包含 blitz_merchant_room.tscn。");

            foreach (var hasBlitzBranch in new[] { false, true })
            {
                var normal = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.GroupId,
                    roomType: null,
                    HasScene,
                    hasBlitzBranch);
                Require(normal == MerchantRuntimeAppearance.StandardMerchantRoomScenePath,
                    $"测试版 PCK 下普通商人应解析到 standard_merchant_room.tscn（hasBlitzBranch={hasBlitzBranch}），实际：{normal}");
                Require(HasScene(normal), $"测试版 PCK 中不存在解析出的普通商人场景：{normal}");

                var blitz = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.GroupId,
                    blitzType,
                    HasScene,
                    hasBlitzBranch);
                Require(blitz == MerchantRuntimeAppearance.BlitzMerchantRoomScenePath,
                    $"测试版 PCK 下极速商人应解析到 blitz_merchant_room.tscn，实际：{blitz}");
                Require(HasScene(blitz), $"测试版 PCK 中不存在极速商人场景：{blitz}");

                var fake = MerchantRuntimeAppearance.ResolveMerchantScenePath(
                    MerchantRuntimeAppearance.FakeMerchantGroupId,
                    roomType: null,
                    HasScene,
                    hasBlitzBranch);
                Require(HasScene(fake), $"测试版 PCK 中不存在假商人场景：{fake}");
            }

            verifiedCount++;
        }

        Require(verifiedCount > 0,
            "未在本机找到正式版或测试版 SlayTheSpire2.pck，无法执行真实游戏包商人路径断言。" +
            $"已尝试的位置：{string.Join("；", KnownPckCandidates().Distinct(StringComparer.OrdinalIgnoreCase))}。" +
            $"也可用环境变量 {OfficialGamePckEnv} / {BetaGamePckEnv} 指定 PCK 路径。");
    }

    // 真实 PCK 是本机夹具，不写死某一台机器的绝对路径：先认环境变量，再按常见 Steam 布局探测，
    // 并用包内场景判定版本（测试版 0.113.x 拆出了 standard_merchant_room.tscn，正式版没有）。
    internal static string? ResolveGamePck(string environmentVariable, bool beta)
    {
        var explicitPath = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            Require(File.Exists(explicitPath), $"{environmentVariable} 指向的 PCK 不存在：{explicitPath}");
            Require(HasBetaSplitScenes(explicitPath) == beta,
                $"{environmentVariable} 指向的 PCK 版本与预期不符（期望{(beta ? "测试版" : "正式版")}）：{explicitPath}");
            return explicitPath;
        }

        foreach (var candidate in KnownPckCandidates().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            if (HasBetaSplitScenes(candidate) == beta)
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> KnownPckCandidates()
    {
        yield return @"D:\mod\游戏版本备份\正式版_v0.111.1_20261117_2249_build26655341\SlayTheSpire2.pck";

        foreach (var root in SteamCommonRoots())
        {
            foreach (var folder in GameFolderNames)
            {
                yield return Path.Combine(root, folder, "SlayTheSpire2.pck");
            }
        }
    }

    // Steam 库不一定是默认的 Program Files：本机就是 D:\steam\steamapps\common，
    // 也有 SteamLibrary 盘符目录和写在 libraryfolders.vdf 里的额外库，都要看。
    private static List<string> SteamCommonRoots()
    {
        var commonRoots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var steamRoots = new List<string>();

        foreach (var drive in new[] { "C", "D", "E", "F", "G" })
        {
            steamRoots.Add($@"{drive}:\steam");
            steamRoots.Add($@"{drive}:\Steam");
            steamRoots.Add($@"{drive}:\SteamLibrary");
            steamRoots.Add($@"{drive}:\steamapps");
            steamRoots.Add($@"{drive}:\Program Files (x86)\Steam");
            steamRoots.Add($@"{drive}:\Program Files\Steam");
        }

        steamRoots.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
        steamRoots.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"));
        steamRoots.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Steam"));

        var libraryRoots = new List<string>();
        foreach (var steamRoot in steamRoots)
        {
            var common = Path.Combine(steamRoot, "steamapps", "common");
            if (seen.Add(common))
            {
                commonRoots.Add(common);
            }

            var libraryFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile))
            {
                continue;
            }

            try
            {
                foreach (var line in File.ReadAllLines(libraryFile))
                {
                    var match = Regex.Match(line, "\"path\"\\s+\"([^\"]+)\"");
                    if (match.Success)
                    {
                        libraryRoots.Add(match.Groups[1].Value.Replace(@"\\", @"\"));
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        foreach (var libraryRoot in libraryRoots)
        {
            var common = Path.Combine(libraryRoot, "steamapps", "common");
            if (seen.Add(common))
            {
                commonRoots.Add(common);
            }
        }

        return commonRoots;
    }

    private static bool HasBetaSplitScenes(string pckPath)
    {
        using var archive = PckArchive.Open(pckPath);
        return archive.Contains(MerchantRuntimeAppearance.StandardMerchantRoomScenePath) ||
               archive.Contains(MerchantRuntimeAppearance.StandardMerchantRoomScenePath + ".remap");
    }

    private static void VerifyEndToEndCatalogResolution()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "sts2-merchant-room-paths-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var catalogProperty = AccessTools.Property(typeof(SkinService), "Catalog")
            ?? throw new InvalidOperationException("未找到 SkinService.Catalog 属性。");
        var previousCatalog = catalogProperty.GetValue(null);
        var blitzType = typeof(NMerchantRoom).Assembly.GetType(
                            "MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom") ??
                        typeof(NBlitzMerchantRoom);

        try
        {
            var stubScene = Encoding.UTF8.GetBytes("[gd_scene format=3]\n[node name=\"Root\" type=\"Control\"]\n");
            var stubRemap = Encoding.UTF8.GetBytes("[remap]\npath=\"res://.godot/exported/merchant.scn\"\n");

            // 1. 正式版布局 PCK（仅有 merchant_room.tscn）
            var officialPck = Path.Combine(tempDir, "official_game.pck");
            PckArchive.Write(officialPck, new Dictionary<string, byte[]>
            {
                ["res://scenes/rooms/merchant_room.tscn"] = stubScene,
                ["res://scenes/events/custom/fake_merchant.tscn"] = stubScene
            });
            using (var officialCatalog = SkinCatalog.Build(officialPck, []))
            {
                catalogProperty.SetValue(null, officialCatalog);
                Require(
                    MerchantRuntimeAppearance.MerchantRoomScenePath ==
                    MerchantRuntimeAppearance.LegacyMerchantRoomScenePath,
                    "加载正式版目录时 MerchantRoomScenePath 必须返回 merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath() ==
                    MerchantRuntimeAppearance.LegacyMerchantRoomScenePath,
                    "加载正式版目录时 GetMerchantScenePath 必须返回 merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.ResolveMerchantRoomScenePath(null) ==
                    MerchantRuntimeAppearance.LegacyMerchantRoomScenePath,
                    "加载正式版目录时 ResolveMerchantRoomScenePath 必须返回 merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath(MerchantRuntimeAppearance.FakeMerchantGroupId) ==
                    "res://scenes/events/custom/fake_merchant.tscn",
                    "假商人路径在正式版目录下应保持 fake_merchant.tscn。");
            }

            // 2. 测试版布局 PCK（standard_merchant_room.tscn + blitz_merchant_room.tscn）
            var betaPck = Path.Combine(tempDir, "beta_game.pck");
            PckArchive.Write(betaPck, new Dictionary<string, byte[]>
            {
                ["res://scenes/rooms/standard_merchant_room.tscn"] = stubScene,
                ["res://scenes/rooms/blitz_merchant_room.tscn"] = stubScene,
                ["res://scenes/events/custom/fake_merchant.tscn"] = stubScene
            });
            using (var betaCatalog = SkinCatalog.Build(betaPck, []))
            {
                catalogProperty.SetValue(null, betaCatalog);
                Require(
                    MerchantRuntimeAppearance.MerchantRoomScenePath ==
                    MerchantRuntimeAppearance.StandardMerchantRoomScenePath,
                    "加载测试版目录时 MerchantRoomScenePath 必须返回 standard_merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath() ==
                    MerchantRuntimeAppearance.StandardMerchantRoomScenePath,
                    "加载测试版目录时 GetMerchantScenePath 必须返回 standard_merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.ResolveMerchantRoomScenePath(null) ==
                    MerchantRuntimeAppearance.StandardMerchantRoomScenePath,
                    "加载测试版目录时 ResolveMerchantRoomScenePath 必须返回 standard_merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath(roomType: blitzType) ==
                    MerchantRuntimeAppearance.BlitzMerchantRoomScenePath,
                    "加载测试版目录且为极速商店类型时必须返回 blitz_merchant_room.tscn。");
            }

            // 3. 导出为 .remap 的测试版布局 PCK 同样应命中 standard_merchant_room.tscn
            var remapPck = Path.Combine(tempDir, "beta_remap_game.pck");
            PckArchive.Write(remapPck, new Dictionary<string, byte[]>
            {
                ["res://scenes/rooms/standard_merchant_room.tscn.remap"] = stubRemap,
                ["res://scenes/rooms/blitz_merchant_room.tscn.remap"] = stubRemap
            });
            using (var remapCatalog = SkinCatalog.Build(remapPck, []))
            {
                catalogProperty.SetValue(null, remapCatalog);
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath() ==
                    MerchantRuntimeAppearance.StandardMerchantRoomScenePath,
                    "仅有 .tscn.remap 导出条目时也必须正确识别 standard_merchant_room.tscn。");
                Require(
                    MerchantRuntimeAppearance.GetMerchantScenePath(roomType: blitzType) ==
                    MerchantRuntimeAppearance.BlitzMerchantRoomScenePath,
                    "仅有 .tscn.remap 导出条目时也必须正确识别 blitz_merchant_room.tscn。");
            }
        }
        finally
        {
            catalogProperty.SetValue(null, previousCatalog);
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    private static void VerifyCallerWiring()
    {
        var prewarm = AccessTools.Method(typeof(MerchantRoomPrewarmPatch), "Postfix")
            ?? throw new InvalidOperationException("未找到 MerchantRoomPrewarmPatch.Postfix。");
        var prewarmInstructions = PatchProcessor.GetOriginalInstructions(prewarm);
        Require(prewarmInstructions.Any(i =>
                i.operand is MethodInfo m &&
                m.DeclaringType == typeof(MerchantRuntimeAppearance) &&
                m.Name == nameof(MerchantRuntimeAppearance.GetMerchantScenePath)),
            "MerchantRoomPrewarmPatch.Postfix 必须通过 GetMerchantScenePath 动态解析商店场景。");
        Require(!prewarmInstructions.Any(i =>
                i.opcode == OpCodes.Ldstr &&
                i.operand is string s &&
                s.Equals(MerchantRuntimeAppearance.StandardMerchantRoomScenePath, StringComparison.OrdinalIgnoreCase)),
            "MerchantRoomPrewarmPatch.Postfix 不得写死测试版 standard_merchant_room.tscn。");

        var resolveRoom = AccessTools.Method(
                typeof(MerchantRuntimeAppearance),
                nameof(MerchantRuntimeAppearance.ResolveMerchantRoomScenePath))
            ?? throw new InvalidOperationException("未找到 ResolveMerchantRoomScenePath。");
        var resolveRoomInstructions = PatchProcessor.GetOriginalInstructions(resolveRoom);
        Require(resolveRoomInstructions.Any(i =>
                i.operand is MethodInfo m &&
                m.DeclaringType == typeof(MerchantRuntimeAppearance) &&
                m.Name == nameof(MerchantRuntimeAppearance.GetMerchantScenePath)),
            "ResolveMerchantRoomScenePath 必须委托给 GetMerchantScenePath。");

        var getOtherEntries = AccessTools.Method(typeof(AncientCompendiumScreen), "GetOtherEntries")
            ?? throw new InvalidOperationException("未找到 AncientCompendiumScreen.GetOtherEntries。");
        var otherEntryInstructions = PatchProcessor.GetOriginalInstructions(getOtherEntries);
        Require(otherEntryInstructions.Any(i =>
                i.operand is MethodInfo m &&
                m.DeclaringType == typeof(MerchantRuntimeAppearance) &&
                m.Name == nameof(MerchantRuntimeAppearance.GetMerchantScenePath)),
            "AncientCompendiumScreen.GetOtherEntries 的普通商人兜底必须通过 GetMerchantScenePath 解析。");
        Require(!otherEntryInstructions.Any(i =>
                i.opcode == OpCodes.Ldstr &&
                i.operand is string s &&
                s.Equals(MerchantRuntimeAppearance.LegacyMerchantRoomScenePath, StringComparison.OrdinalIgnoreCase)),
            "AncientCompendiumScreen.GetOtherEntries 不得再写死测试版已删除的 merchant_room.tscn。");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
