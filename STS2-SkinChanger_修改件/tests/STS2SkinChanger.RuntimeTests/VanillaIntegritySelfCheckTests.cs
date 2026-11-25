using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class VanillaIntegritySelfCheckTests
{
    private const string CardAtlasImportPath = "res://images/atlases/card_atlas_0.png.import";
    private const string CardAtlasCtexPath = "res://.godot/imported/card_atlas_0.png-6b947739e98d17beac01a23ccc9a95cd.bptc.ctex";
    private const string SelectPortraitImportPath = "res://images/packed/character_select/char_select_ironclad.png.import";
    private const string SelectPortraitCtexPath = "res://.godot/imported/char_select_ironclad.png-6a60464bc609de472b194c81a0841360.bptc.ctex";
    private const string BetaAncientBorderPath = "res://images/atlases/compressed_atlas.sprites/ancient_card_border.tres";
    private const string BetaCompressedAtlasImportPath = "res://images/atlases/compressed_atlas.png.import";
    private const string BetaCompressedAtlasCtexPath = "res://.godot/imported/compressed_atlas.png-af498678a5aa54d48a9f8f8b521b3b92.s3tc.ctex";
    private const string StableAncientBorderPath = "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres";
    private const string StableCompressedImportPath = "res://images/atlases/compressed.png.import";
    private const string StableCompressedCtexPath = "res://.godot/imported/compressed.png-1111222233334444.s3tc.ctex";

    internal static void Run()
    {
        VerifyCleanNoConflicts();
        VerifySuspectOnlyWithoutConfirmationAndWhenLiveIsStillVanilla();
        VerifyConfirmedOverwriteAndWinningModResolution();
        VerifyManagedBySkinChangerExcluded();
        VerifyNativeModSourceRulesExcludeInactiveHasPckFalseAndLegacyFallback();
        VerifySelfOverlayExclusionDowngradesToSuspect();
        VerifyBudgetedMultiFrameConfirmationAndOverBudgetFallback();
        VerifyRuntimeConfirmContinuationAdvancesAcrossProcessFrames();
        VerifyStableAndBetaAncientTemplateDiscovery();
        VerifyRealPckArchiveScanRoundtrip();
        VerifyMainMenuPatchAndSelfOverlayHooksBindCleanly();
        Console.WriteLine(
            "Vanilla integrity self-check passed: clean, suspect, confirmed, managed/inactive/self-overlay exclusion, budgeted multi-frame VFS, stable/beta ancient templates, PCK scan and hooks.");
    }

    private static void VerifyCleanNoConflicts()
    {
        var gameEntries = BuildBaseGameEntries();
        var identicalCopy = gameEntries[CardAtlasImportPath];
        var benignPack = new VanillaIntegrityModPack(
            ModId: "benign.ui",
            DisplayName: "Benign UI",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                ["res://benign_ui/panel.png"] = MakeEntry("res://benign_ui/panel.png", "custom-panel"),
                // Unmodified byte-identical copy of vanilla file must not trigger false alarm.
                [CardAtlasImportPath] = identicalCopy
            });

        var report = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [benignPack],
            elapsedMs: 12,
            mainThreadMs: 1,
            mainThreadFrames: 2);
        Require(report.CheckedCount == 7,
            $"基线抽查应覆盖 7 项（共享图集+选角头像+先古模板及其导入产物），实际：{report.CheckedCount}。");
        Require(report.Findings.Count == 0 && report.SuspectCount == 0 && report.ConfirmedCount == 0,
            "无冲突场景不得报嫌疑或确认。");

        var lines = VanillaIntegritySelfCheck.FormatLogLines(report);
        Require(lines.Count == 1 &&
                lines[0] == "[完整性自检] 检查 7 项，嫌疑 0，确认 0，耗时 12 ms，主线程 1 ms（2 帧）",
            $"全部正常时必须只输出汇总行（含主线程耗时与帧数），实际：{string.Join(" | ", lines)}");
    }

    private static void VerifySuspectOnlyWithoutConfirmationAndWhenLiveIsStillVanilla()
    {
        var gameEntries = BuildBaseGameEntries();
        var modifiedAtlasImport = MakeEntry(
            CardAtlasImportPath,
            "path.bptc=\"res://.godot/imported/mod_card_atlas_0.ctex\"");
        var modifiedPortrait = MakeEntry(SelectPortraitCtexPath, "MOD_PORTRAIT_BYTES_DIFFERENT_SIZE");
        var zeroMd5Modified = new VanillaIntegrityEntry(
            BetaCompressedAtlasCtexPath,
            gameEntries[BetaCompressedAtlasCtexPath].Size,
            RedirectTarget: null,
            Md5: new byte[16],
            ReadBytes: () => Encoding.UTF8.GetBytes("ZERO_MD5_SAME_LEN_DIFF!!"));
        var zeroMd5Identical = new VanillaIntegrityEntry(
            CardAtlasCtexPath,
            gameEntries[CardAtlasCtexPath].Size,
            RedirectTarget: null,
            Md5: new byte[16],
            ReadBytes: gameEntries[CardAtlasCtexPath].ReadBytes);

        var suspectPack = new VanillaIntegrityModPack(
            ModId: "suspect.pack",
            DisplayName: "Suspect Pack",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasImportPath] = modifiedAtlasImport,
                [SelectPortraitCtexPath] = modifiedPortrait,
                [BetaCompressedAtlasCtexPath] = zeroMd5Modified,
                [CardAtlasCtexPath] = zeroMd5Identical
            });

        // Case 1: no live VFS reader -> all real conflicts stay Suspect, zero-MD5 byte-identical skipped.
        var noLiveReport = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [suspectPack],
            readLiveBytes: null,
            elapsedMs: 9,
            mainThreadMs: 1,
            mainThreadFrames: 2);
        Require(noLiveReport.SuspectCount == 3 && noLiveReport.ConfirmedCount == 0,
            $"无运行态核对时应报 3 嫌疑 / 0 确认，实际：嫌疑={noLiveReport.SuspectCount}, 确认={noLiveReport.ConfirmedCount}。");
        var lines = VanillaIntegritySelfCheck.FormatLogLines(noLiveReport);
        Require(lines.Count == 4 &&
                lines.Contains($"[完整性自检] 嫌疑 {CardAtlasImportPath} ← Suspect Pack(suspect.pack)") &&
                lines.Contains($"[完整性自检] 嫌疑 {SelectPortraitCtexPath} ← Suspect Pack(suspect.pack)") &&
                lines.Contains($"[完整性自检] 嫌疑 {BetaCompressedAtlasCtexPath} ← Suspect Pack(suspect.pack)") &&
                lines[^1] == "[完整性自检] 检查 7 项，嫌疑 3，确认 0，耗时 9 ms，主线程 1 ms（2 帧）",
            $"嫌疑日志格式不符：{string.Join(" | ", lines)}");

        // Case 2: live VFS still serves vanilla bytes -> remains Suspect (not falsely upgraded to Confirmed).
        var vanillaLiveReport = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [suspectPack],
            readLiveBytes: path => gameEntries.TryGetValue(path, out var entry) ? entry.ReadBytes?.Invoke() : null,
            elapsedMs: 11,
            mainThreadMs: 2,
            mainThreadFrames: 2);
        Require(vanillaLiveReport.SuspectCount == 3 && vanillaLiveReport.ConfirmedCount == 0,
            "运行态仍为原版字节时不得误标为确认。");
    }

    private static void VerifyConfirmedOverwriteAndWinningModResolution()
    {
        var gameEntries = BuildBaseGameEntries();
        var modABytes = Encoding.UTF8.GetBytes("MOD_A_OVERWRITE_ATLAS_PAYLOAD");
        var modBBytes = Encoding.UTF8.GetBytes("MOD_B_OVERWRITE_ATLAS_PAYLOAD_WINNER");

        var packA = new VanillaIntegrityModPack(
            ModId: "mod.a",
            DisplayName: "Mod A",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, modABytes)
            });
        var packB = new VanillaIntegrityModPack(
            ModId: "mod.b",
            DisplayName: "Mod B",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, modBBytes)
            });

        var report = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [packA, packB],
            readLiveBytes: path => path.Equals(CardAtlasCtexPath, StringComparison.OrdinalIgnoreCase)
                ? modBBytes
                : gameEntries.TryGetValue(path, out var entry) ? entry.ReadBytes?.Invoke() : null,
            elapsedMs: 15,
            mainThreadMs: 2,
            mainThreadFrames: 3);

        Require(report.ConfirmedCount == 1 && report.SuspectCount == 1,
            $"两个原生包撞同一路径且 B 生效时应为 1 确认 + 1 嫌疑，实际：确认={report.ConfirmedCount}, 嫌疑={report.SuspectCount}。");
        var lines = VanillaIntegritySelfCheck.FormatLogLines(report);
        Require(lines.Contains($"[完整性自检] 确认 {CardAtlasCtexPath} ← Mod B(mod.b)") &&
                lines.Contains($"[完整性自检] 嫌疑 {CardAtlasCtexPath} ← Mod A(mod.a)") &&
                lines[^1] == "[完整性自检] 检查 7 项，嫌疑 1，确认 1，耗时 15 ms，主线程 2 ms（3 帧）",
            $"确认/嫌疑归因日志不符：{string.Join(" | ", lines)}");
    }

    private static void VerifyManagedBySkinChangerExcluded()
    {
        var gameEntries = BuildBaseGameEntries();
        var managedBytes = Encoding.UTF8.GetBytes("MANAGED_SKIN_ATLAS_SHOULD_BE_IGNORED");
        var preloadedBytes = Encoding.UTF8.GetBytes("PRELOADED_NATIVE_CONFLICT_BYTES");

        var managedPack = new VanillaIntegrityModPack(
            ModId: "managed.skin",
            DisplayName: "Managed Skin",
            IsManagedBySkinChanger: true,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, managedBytes),
                [SelectPortraitImportPath] = MakeEntry(SelectPortraitImportPath, "path=\"res://managed/other.ctex\""),
                [BetaAncientBorderPath] = MakeEntry(BetaAncientBorderPath, "managed-border")
            });

        var onlyManagedReport = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [managedPack],
            readLiveBytes: _ => managedBytes,
            elapsedMs: 4);
        Require(onlyManagedReport.Findings.Count == 0,
            "被切换器隔离托管的皮肤包不得进入完整性自检报警。");

        var nativePack = new VanillaIntegrityModPack(
            ModId: "native.preloaded",
            DisplayName: "Preloaded Pack",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [SelectPortraitCtexPath] = MakeEntry(SelectPortraitCtexPath, preloadedBytes)
            });

        var mixedReport = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [managedPack, nativePack],
            readLiveBytes: path => path.Equals(SelectPortraitCtexPath, StringComparison.OrdinalIgnoreCase)
                ? preloadedBytes
                : null,
            elapsedMs: 6);
        Require(mixedReport.Findings.Count == 1 &&
                mixedReport.ConfirmedCount == 1 &&
                mixedReport.Findings[0].ModId == "native.preloaded",
            "混合场景下应只报原生加载包、跳过托管包。");
    }

    private static void VerifyNativeModSourceRulesExcludeInactiveHasPckFalseAndLegacyFallback()
    {
        var tempDir = Directory.CreateTempSubdirectory("sts2-integrity-native-rules-");
        try
        {
            var gamePck = Path.Combine(tempDir.FullName, "SlayTheSpire2.pck");
            var modADir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "ModA")).FullName;
            var modBDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "ModB")).FullName;
            var modCDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "ModC")).FullName;
            var modDDir = Directory.CreateDirectory(Path.Combine(tempDir.FullName, "ModD")).FullName;

            var vanillaAtlasBytes = Encoding.UTF8.GetBytes("VANILLA_CARD_ATLAS_CTEX_CONTENT");
            var conflictBytes = Encoding.UTF8.GetBytes("SHARED_CONFLICTING_ATLAS_BYTES_FROM_A");

            PckArchive.Write(gamePck, new Dictionary<string, byte[]>
            {
                [CardAtlasImportPath] = Encoding.UTF8.GetBytes($"[remap]\npath.bptc=\"{CardAtlasCtexPath}\"\n"),
                [CardAtlasCtexPath] = vanillaAtlasBytes
            });

            // Mod A: genuinely loaded native mod (has_pck=true + exact mod.a.pck).
            PckArchive.Write(Path.Combine(modADir, "mod.a.pck"), new Dictionary<string, byte[]>
            {
                [CardAtlasCtexPath] = conflictBytes
            });

            // Mod B (loaded AFTER A): has_pck=false, but leftover mod.b.pck with identical bytes sits in folder.
            // Must NEVER enter candidate list or steal "确认" from Mod A!
            PckArchive.Write(Path.Combine(modBDir, "mod.b.pck"), new Dictionary<string, byte[]>
            {
                [CardAtlasCtexPath] = conflictBytes
            });

            // Mod C (loaded AFTER A): has_pck=true, but only a legacy alternate filename exists (no mod.c.pck).
            // Native loader only loads <id>.pck, so legacy fallback must NOT be picked up!
            File.WriteAllText(
                Path.Combine(modCDir, "legacy_stem.json"),
                "{\"id\":\"mod.c\",\"affects_gameplay\":false,\"has_pck\":true,\"has_dll\":false}");
            PckArchive.Write(Path.Combine(modCDir, "legacy_stem.pck"), new Dictionary<string, byte[]>
            {
                [CardAtlasCtexPath] = conflictBytes
            });

            // Mod D: has_pck=true + exact mod.d.pck, but NOT in Loaded state -> must be ignored.
            PckArchive.Write(Path.Combine(modDDir, "mod.d.pck"), new Dictionary<string, byte[]>
            {
                [CardAtlasCtexPath] = conflictBytes
            });

            var rawMods = new VanillaIntegrityRawMod[]
            {
                new("mod.a", "Mod A", modADir, IsLoaded: true, HasPck: true, IsManagedBySkinChanger: false),
                new("mod.b", "Mod B (NoPck)", modBDir, IsLoaded: true, HasPck: false, IsManagedBySkinChanger: false),
                new("mod.c", "Mod C (LegacyName)", modCDir, IsLoaded: true, HasPck: true, IsManagedBySkinChanger: false),
                new("mod.d", "Mod D (Unloaded)", modDDir, IsLoaded: false, HasPck: true, IsManagedBySkinChanger: false)
            };

            var resolved = VanillaIntegritySelfCheck.ResolveNativeModSources(rawMods);
            Require(resolved.Count == 1 && resolved[0].ModId == "mod.a",
                $"原生候选必须严格遵循 Loaded + has_pck=true + <id>.pck，实际命中：{string.Join(",", resolved.Select(s => s.ModId))}");

            var report = VanillaIntegritySelfCheck.ScanArchives(
                gamePck,
                resolved,
                readLiveBytes: path => path.Equals(CardAtlasCtexPath, StringComparison.OrdinalIgnoreCase)
                    ? conflictBytes
                    : null);

            Require(report.Findings.Count == 1 &&
                    report.ConfirmedCount == 1 &&
                    report.SuspectCount == 0 &&
                    report.Findings[0].ModId == "mod.a",
                $"has_pck=false 残留包与非 <id>.pck 包不得抢占确认归因，实际：{string.Join(" | ", VanillaIntegritySelfCheck.FormatLogLines(report))}");
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    private static void VerifySelfOverlayExclusionDowngradesToSuspect()
    {
        var gameEntries = BuildBaseGameEntries();
        var conflictBytes = Encoding.UTF8.GetBytes("CONFLICTING_ATLAS_BYTES_ALSO_MOUNTED_BY_SELF");

        var nativePack = new VanillaIntegrityModPack(
            ModId: "native.a",
            DisplayName: "Native A",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, conflictBytes),
                [SelectPortraitCtexPath] = MakeEntry(SelectPortraitCtexPath, conflictBytes)
            });

        // Case 1: CardAtlasCtexPath is owned by SkinChanger's own mounted overlay -> MUST downgrade to Suspect;
        // SelectPortraitCtexPath is NOT owned by SkinChanger -> still Confirmed.
        var report = VanillaIntegritySelfCheck.Evaluate(
            gameEntries,
            [nativePack],
            readLiveBytes: _ => conflictBytes,
            elapsedMs: 10,
            selfOverlayPaths: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { CardAtlasCtexPath });

        var atlasFinding = report.Findings.Single(f => f.Path.Equals(CardAtlasCtexPath, StringComparison.OrdinalIgnoreCase));
        var portraitFinding = report.Findings.Single(f => f.Path.Equals(SelectPortraitCtexPath, StringComparison.OrdinalIgnoreCase));
        Require(atlasFinding.Severity == VanillaIntegritySeverity.Suspect,
            "切换器自身已挂载覆盖的路径即使字节与原生候选一致，也必须降级为嫌疑而不得报确认。");
        Require(portraitFinding.Severity == VanillaIntegritySeverity.Confirmed,
            "未被切换器自挂载覆盖的另一条路径仍应正常报确认。");

        // Case 2: Incomplete self-overlay snapshot (e.g. lock contention / missing reflection hook) ->
        // conservatively keep ALL suspects as Suspect, zero VFS reads.
        var countingReader = new CountingMemoryVfsReader(new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasCtexPath] = conflictBytes,
            [SelectPortraitCtexPath] = conflictBytes
        });
        using var incompleteSession = VanillaIntegritySelfCheck.CreateConfirmSession(
            gameEntries,
            [nativePack],
            countingReader,
            selfOverlayIncomplete: true);
        Require(incompleteSession.StepFrame(),
            "自挂载记录不完整时应单帧安全降级结束。");
        Require(countingReader.OpenCount == 0 &&
                incompleteSession.Findings.Count == 2 &&
                incompleteSession.Findings.All(f => f.Severity == VanillaIntegritySeverity.Suspect),
            "自挂载记录不确定时不得读取 VFS 或升级为确认。");
    }

    private static void VerifyBudgetedMultiFrameConfirmationAndOverBudgetFallback()
    {
        var gameEntries = BuildBaseGameEntries();
        // 512 KiB modified atlas (with 200 KiB/frame budget and 64 KiB chunks -> needs 3 frames).
        var multiFrameBytes = new byte[512 * 1024];
        for (var i = 0; i < multiFrameBytes.Length; i++)
        {
            multiFrameBytes[i] = (byte)((i * 31 + 7) & 0xFF);
        }

        // 700 KiB oversized portrait (> 600 KiB MaxConfirmFileBytes -> must skip without reading and stay Suspect).
        var oversizedBytes = new byte[700 * 1024];
        Array.Fill(oversizedBytes, (byte)0x5A);

        // 120 KiB ancient ctex (fits individual limit 600 KiB, but 512 + 120 = 632 > 600 MaxTotalConfirmBytes -> stays Suspect).
        var overTotalBytes = new byte[120 * 1024];
        Array.Fill(overTotalBytes, (byte)0x3C);

        // 40 KiB atlas import (512 + 40 = 552 <= 600 -> still confirms within remaining total budget!).
        var smallImportBytes = Encoding.UTF8.GetBytes("path.bptc=\"res://.godot/imported/mod_atlas_small.ctex\"" + new string(' ', 40 * 1024));

        var nativePack = new VanillaIntegrityModPack(
            ModId: "heavy.native",
            DisplayName: "Heavy Native",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, multiFrameBytes),
                [SelectPortraitCtexPath] = MakeEntry(SelectPortraitCtexPath, oversizedBytes),
                [BetaCompressedAtlasCtexPath] = MakeEntry(BetaCompressedAtlasCtexPath, overTotalBytes),
                [CardAtlasImportPath] = MakeEntry(CardAtlasImportPath, smallImportBytes)
            });

        var vfsReader = new CountingMemoryVfsReader(new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasCtexPath] = multiFrameBytes,
            [SelectPortraitCtexPath] = oversizedBytes,
            [BetaCompressedAtlasCtexPath] = overTotalBytes,
            [CardAtlasImportPath] = smallImportBytes
        });

        var tightBudget = new VanillaIntegrityBudget(
            MaxConfirmFileBytes: 600 * 1024,
            MaxTotalConfirmBytes: 600 * 1024,
            PerFrameByteBudget: 200 * 1024,
            ChunkBytes: 64 * 1024,
            PerFrameTimeBudgetMs: 100.0);

        using var session = VanillaIntegritySelfCheck.CreateConfirmSession(
            gameEntries,
            [nativePack],
            vfsReader,
            tightBudget);

        var maxBytesInSingleFrame = 0L;
        var guard = 0;
        while (guard++ < 20)
        {
            var beforeBytes = vfsReader.TotalBytesRead;
            var done = session.StepFrame(elapsedMsProvider: () => 0.1);
            var deltaBytes = vfsReader.TotalBytesRead - beforeBytes;
            if (deltaBytes > maxBytesInSingleFrame)
            {
                maxBytesInSingleFrame = deltaBytes;
            }

            if (done)
            {
                break;
            }
        }

        Require(session.FramesUsed >= 3 && session.FramesUsed <= 5,
            $"分帧预算核对必须拆成多帧执行，实际帧数：{session.FramesUsed}。");
        Require(maxBytesInSingleFrame <= tightBudget.PerFrameByteBudget,
            $"单帧 VFS 读取字节不得超过帧预算 {tightBudget.PerFrameByteBudget}，实际峰值：{maxBytesInSingleFrame}。");
        Require(vfsReader.BytesReadFor(SelectPortraitCtexPath) == 0,
            "超过单文件大小上限的 VFS 文件不得读取字节。");
        Require(vfsReader.BytesReadFor(BetaCompressedAtlasCtexPath) == 0,
            "超过累计总量上限的 VFS 文件不得读取字节。");

        var byPath = session.Findings.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);
        Require(byPath[CardAtlasCtexPath].Severity == VanillaIntegritySeverity.Confirmed,
            "分帧读取完的冲突图集应最终标为确认。");
        Require(byPath[CardAtlasImportPath].Severity == VanillaIntegritySeverity.Confirmed,
            "仍在剩余累计预算内的小文件应照常标为确认。");
        Require(byPath[SelectPortraitCtexPath].Severity == VanillaIntegritySeverity.Suspect,
            "超单文件预算的文件必须保留为嫌疑。");
        Require(byPath[BetaCompressedAtlasCtexPath].Severity == VanillaIntegritySeverity.Suspect,
            "超总预算的文件必须保留为嫌疑。");

        // Also verify mid-scan self-overlay takeover aborts confirmation cleanly to Suspect.
        var midScanReader = new CountingMemoryVfsReader(new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasCtexPath] = multiFrameBytes
        });
        var singlePack = new VanillaIntegrityModPack(
            ModId: "mid.native",
            DisplayName: "Mid Native",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, multiFrameBytes)
            });
        using var midSession = VanillaIntegritySelfCheck.CreateConfirmSession(
            gameEntries,
            [singlePack],
            midScanReader,
            tightBudget);
        Require(!midSession.StepFrame(elapsedMsProvider: () => 0.1),
            "512 KiB 文件在 200 KiB 帧预算下第一帧不应直接完成。");
        midSession.MergeSelfOverlayPaths([CardAtlasCtexPath], snapshotComplete: true);
        Require(midSession.StepFrame(elapsedMsProvider: () => 0.1),
            "中途被切换器自挂载接管后应立即停止剩余分块并结束。");
        Require(midSession.Findings.Single().Severity == VanillaIntegritySeverity.Suspect &&
                midScanReader.TotalBytesRead <= tightBudget.PerFrameByteBudget,
            "中途检测到自挂载覆盖必须降级为嫌疑且停止继续读取 VFS。");

        // Also verify per-frame time budget cutoff stops after 1 chunk when simulated elapsed time hits budget.
        var timeReader = new CountingMemoryVfsReader(new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasCtexPath] = multiFrameBytes
        });
        var timeBudget = new VanillaIntegrityBudget(
            MaxConfirmFileBytes: 1024 * 1024,
            MaxTotalConfirmBytes: 1024 * 1024,
            PerFrameByteBudget: 1024 * 1024,
            ChunkBytes: 128 * 1024,
            PerFrameTimeBudgetMs: 2.5);
        using var timeSession = VanillaIntegritySelfCheck.CreateConfirmSession(
            gameEntries,
            [singlePack],
            timeReader,
            timeBudget);
        var timeFrames = 0;
        while (timeFrames < 10)
        {
            var beforeBytes = timeReader.TotalBytesRead;
            var done = timeSession.StepFrame(elapsedMsProvider: () => 3.0);
            var delta = timeReader.TotalBytesRead - beforeBytes;
            Require(delta == timeBudget.ChunkBytes,
                $"命中每帧时间预算时应每帧最多读 1 个分块（{timeBudget.ChunkBytes} B），实际：{delta} B。");
            timeFrames++;
            if (done)
            {
                break;
            }
        }

        Require(timeFrames == 4 &&
                timeSession.FramesUsed == 4 &&
                timeSession.Findings.Single().Severity == VanillaIntegritySeverity.Confirmed,
            $"512 KiB 按 128 KiB 时间截断应恰好 4 帧完成并确认，实际帧数：{timeFrames}。");
    }

    private static void VerifyRuntimeConfirmContinuationAdvancesAcrossProcessFrames()
    {
        var gameEntries = BuildBaseGameEntries();
        var atlasBytes = new byte[448 * 1024];
        var portraitBytes = new byte[256 * 1024];
        for (var i = 0; i < atlasBytes.Length; i++)
        {
            atlasBytes[i] = (byte)((i * 17 + 3) & 0xFF);
        }

        for (var i = 0; i < portraitBytes.Length; i++)
        {
            portraitBytes[i] = (byte)((i * 29 + 11) & 0xFF);
        }

        var nativePack = new VanillaIntegrityModPack(
            ModId: "multi.batch.native",
            DisplayName: "Multi Batch Native",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, atlasBytes),
                [SelectPortraitCtexPath] = MakeEntry(SelectPortraitCtexPath, portraitBytes)
            });

        var vfsReader = new CountingMemoryVfsReader(new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasCtexPath] = atlasBytes,
            [SelectPortraitCtexPath] = portraitBytes
        });

        var tightBudget = new VanillaIntegrityBudget(
            MaxConfirmFileBytes: 1024 * 1024,
            MaxTotalConfirmBytes: 2 * 1024 * 1024,
            PerFrameByteBudget: 200 * 1024,
            ChunkBytes: 64 * 1024,
            PerFrameTimeBudgetMs: 100.0);

        var deferredQueue = new Queue<Action>();
        var processFrameQueue = new Queue<Action>();
        var warnings = new List<string>();
        ulong frame = 10;

        try
        {
            DeferredDispatch.ConfigureForTests(
                enqueue: deferredQueue.Enqueue,
                frameCounter: () => frame,
                breadcrumbDirectoryProvider: () => null,
                warnLogger: warnings.Add);

            VanillaIntegrityReport? completedReport = null;
            using (VanillaIntegritySelfCheck.UseNextProcessFrameSchedulerForTests(callback =>
                   {
                       processFrameQueue.Enqueue(callback);
                       return true;
                   }))
            {
                var fifoEvents = new List<string>();
                VanillaIntegritySelfCheck.StartRuntimeConfirmForTests(
                    gameEntries,
                    [nativePack],
                    vfsReader,
                    tightBudget,
                    report => completedReport = report,
                    elapsedMsProvider: () => 0.1);
                DeferredDispatch.CallDeferred("Test.FifoSibling", "ui#1", () => fifoEvents.Add($"fifo@f{frame}"));

                // Simulate Godot draining CallDeferred to completion inside a SINGLE idle cycle
                // without advancing the frame counter.
                while (deferredQueue.Count > 0)
                {
                    deferredQueue.Dequeue()();
                }

                Require(fifoEvents.SequenceEqual(["fifo@f10"]),
                    $"其它依赖 FIFO 的 CallDeferred 回调仍应在同一 idle 按顺序执行：{string.Join(",", fifoEvents)}");
                Require(completedReport == null,
                    "预算用尽后的自检续约不得在同一 idle 周期内被 CallDeferred 连续跑完。");
                Require(vfsReader.TotalBytesRead > 0 && vfsReader.TotalBytesRead <= tightBudget.PerFrameByteBudget,
                    $"首帧仅应消耗单帧预算（<= {tightBudget.PerFrameByteBudget} B），实际：{vfsReader.TotalBytesRead} B。");
                Require(processFrameQueue.Count == 1,
                    $"首帧预算耗尽后应挂出 1 个一次性 ProcessFrame 续约，实际：{processFrameQueue.Count}。");

                var guard = 0;
                while (processFrameQueue.Count > 0 && guard++ < 20)
                {
                    frame++;
                    var beforeBytes = vfsReader.TotalBytesRead;
                    var batchCount = processFrameQueue.Count;
                    Require(batchCount == 1,
                        $"单链分帧续约每帧只应挂出 1 个回调，实际：{batchCount}。");
                    processFrameQueue.Dequeue()();

                    // Continuation must never fall back to CallDeferred while ProcessFrame is available.
                    Require(deferredQueue.Count == 0,
                        "分帧续约不得再次入队 CallDeferred 导致同 idle 连续执行。");
                    var deltaBytes = vfsReader.TotalBytesRead - beforeBytes;
                    Require(deltaBytes <= tightBudget.PerFrameByteBudget,
                        $"第 {frame} 帧读取字节超过单帧预算：delta={deltaBytes}, budget={tightBudget.PerFrameByteBudget}。");
                }
            }

            Require(completedReport != null, "多批候选应在后续 ProcessFrame 中全部完成。");
            Require(completedReport!.ConfirmedCount == 2 && completedReport.SuspectCount == 0,
                $"两份候选资源应全部确认：confirmed={completedReport.ConfirmedCount}, suspect={completedReport.SuspectCount}。");

            var stepRecords = DeferredDispatch.SnapshotRecords()
                .Where(r => r.TaskName == "VanillaIntegritySelfCheck.Step" && r.Phase == "DONE")
                .ToArray();
            Require(stepRecords.Length >= 4,
                $"704 KiB 候选在 200 KiB 帧预算下至少应拆成 4 步执行，实际：{stepRecords.Length}。");
            Require(completedReport.MainThreadFrames == stepRecords.Length + 1,
                $"报告主线程帧数应与初始快照(1)+分帧步数({stepRecords.Length})一致，实际：{completedReport.MainThreadFrames}。");
            Require(stepRecords[0].ExecuteFrame == 10,
                $"第 1 步应在初始帧 10 执行，实际：{stepRecords[0].ExecuteFrame}。");
            for (var i = 1; i < stepRecords.Length; i++)
            {
                Require(stepRecords[i].ExecuteFrame == stepRecords[i - 1].ExecuteFrame + 1,
                    $"第 {i + 1} 步执行帧号必须严格推进到下一帧：prev={stepRecords[i - 1].ExecuteFrame}, curr={stepRecords[i].ExecuteFrame}。");
                Require(stepRecords[i].EnqueueFrame == stepRecords[i - 1].ExecuteFrame,
                    $"第 {i + 1} 步入队帧应等于上一步执行帧：expected={stepRecords[i - 1].ExecuteFrame}, actual={stepRecords[i].EnqueueFrame}。");
            }

            // If ProcessFrame scheduler is unavailable (e.g. main loop shutting down), stop cleanly.
            var abortedReport = false;
            using (VanillaIntegritySelfCheck.UseNextProcessFrameSchedulerForTests(_ => false))
            {
                VanillaIntegritySelfCheck.StartRuntimeConfirmForTests(
                    gameEntries,
                    [nativePack],
                    vfsReader,
                    tightBudget,
                    _ => abortedReport = true,
                    elapsedMsProvider: () => 0.1);
                while (deferredQueue.Count > 0)
                {
                    deferredQueue.Dequeue()();
                }
            }

            Require(!abortedReport && deferredQueue.Count == 0,
                "主循环不可用导致 ProcessFrame 无法挂接时应安全释放并停止，不得回退同 idle 自排。");

            // Structural guard: StepOnMainThread uses WrapCallback (not CallDeferred), and default scheduler uses OneShot ProcessFrame.
            var runnerType = typeof(VanillaIntegritySelfCheck).GetNestedType("RuntimeConfirmRunner", BindingFlags.NonPublic)
                             ?? throw new InvalidOperationException("找不到 RuntimeConfirmRunner。");
            var stepMethod = runnerType.GetMethod("StepOnMainThread", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                             ?? throw new InvalidOperationException("找不到 RuntimeConfirmRunner.StepOnMainThread。");
            var stepCalls = PatchProcessor.GetOriginalInstructions(stepMethod)
                .Select(i => i.operand)
                .OfType<MethodInfo>()
                .ToArray();
            Require(stepCalls.Any(m => m.DeclaringType == typeof(DeferredDispatch) && m.Name == nameof(DeferredDispatch.WrapCallback)),
                "RuntimeConfirmRunner.StepOnMainThread 必须经 DeferredDispatch.WrapCallback 保留任务记录。");
            Require(!stepCalls.Any(m => m.DeclaringType == typeof(DeferredDispatch) && m.Name == nameof(DeferredDispatch.CallDeferred)),
                "RuntimeConfirmRunner.StepOnMainThread 不得再通过 CallDeferred 自续。");

            var schedulerMethod = typeof(VanillaIntegritySelfCheck).GetMethod(
                                      "TryScheduleNextProcessFrameOnSceneTree",
                                      BindingFlags.Static | BindingFlags.NonPublic)
                                  ?? throw new InvalidOperationException("找不到 TryScheduleNextProcessFrameOnSceneTree。");
            var schedulerInstructions = PatchProcessor.GetOriginalInstructions(schedulerMethod);
            Require(schedulerInstructions.Any(i =>
                        (i.operand is FieldInfo f &&
                         f.DeclaringType == typeof(SceneTree.SignalName) &&
                         f.Name == nameof(SceneTree.SignalName.ProcessFrame)) ||
                        (i.operand is MethodInfo m &&
                         m.DeclaringType == typeof(SceneTree.SignalName) &&
                         m.Name == "get_ProcessFrame")),
                "默认续约调度器必须挂接 SceneTree.SignalName.ProcessFrame。");
            Require(schedulerInstructions.Any(i => i.operand is MethodInfo m && m.Name == nameof(GodotObject.Connect)),
                "默认续约调度器必须通过一次性 Connect(OneShot) 挂接下一 ProcessFrame。");
        }
        finally
        {
            DeferredDispatch.ResetDefaultsForTests();
        }
    }

    private static void VerifyStableAndBetaAncientTemplateDiscovery()
    {
        var stableGameEntries = new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
        {
            [StableAncientBorderPath] = MakeEntry(
                StableAncientBorderPath,
                "[ext_resource type=\"Texture2D\" path=\"res://images/atlases/compressed.png\" id=\"1\"]"),
            [StableCompressedImportPath] = MakeEntry(
                StableCompressedImportPath,
                $"path.s3tc=\"{StableCompressedCtexPath}\""),
            [StableCompressedCtexPath] = MakeEntry(StableCompressedCtexPath, "STABLE_COMPRESSED_VANILLA")
        };

        var stableModBytes = Encoding.UTF8.GetBytes("STABLE_COMPRESSED_OVERWRITTEN");
        var stableMod = new VanillaIntegrityModPack(
            ModId: "stable.override",
            DisplayName: "Stable Override",
            IsManagedBySkinChanger: false,
            Entries: new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [StableCompressedCtexPath] = MakeEntry(StableCompressedCtexPath, stableModBytes)
            });

        var report = VanillaIntegritySelfCheck.Evaluate(
            stableGameEntries,
            [stableMod],
            readLiveBytes: path => path.Equals(StableCompressedCtexPath, StringComparison.OrdinalIgnoreCase)
                ? stableModBytes
                : null,
            elapsedMs: 3);
        Require(report.CheckedCount == 3 && report.ConfirmedCount == 1 &&
                report.Findings[0].Path.Equals(StableCompressedCtexPath, StringComparison.OrdinalIgnoreCase),
            "正式版旧路径先古模板（compressed.sprites/card_template）及两跳导入产物必须被抽查并确认。");
    }

    private static void VerifyRealPckArchiveScanRoundtrip()
    {
        var tempDir = Directory.CreateTempSubdirectory("sts2-integrity-pck-");
        try
        {
            var gamePck = Path.Combine(tempDir.FullName, "SlayTheSpire2.pck");
            var badModPck = Path.Combine(tempDir.FullName, "bad_native.pck");
            var managedModPck = Path.Combine(tempDir.FullName, "managed_skin.pck");

            var vanillaAtlasBytes = Encoding.UTF8.GetBytes("VANILLA_CARD_ATLAS_CTEX_CONTENT");
            var badAtlasBytes = Encoding.UTF8.GetBytes("BAD_NATIVE_MOD_ATLAS_OVERWRITE_CONTENT");
            var managedPortraitBytes = Encoding.UTF8.GetBytes("MANAGED_PORTRAIT_IGNORED_CONTENT");

            PckArchive.Write(gamePck, new Dictionary<string, byte[]>
            {
                [CardAtlasImportPath] = Encoding.UTF8.GetBytes($"[remap]\npath.bptc=\"{CardAtlasCtexPath}\"\n"),
                [CardAtlasCtexPath] = vanillaAtlasBytes,
                [SelectPortraitImportPath] = Encoding.UTF8.GetBytes($"[remap]\npath.bptc=\"{SelectPortraitCtexPath}\"\n"),
                [SelectPortraitCtexPath] = Encoding.UTF8.GetBytes("VANILLA_SELECT_PORTRAIT_CTEX"),
                ["res://scenes/unrelated_room.tscn"] = Encoding.UTF8.GetBytes("UNRELATED_SHOULD_NOT_BE_SAMPLED")
            });

            PckArchive.Write(badModPck, new Dictionary<string, byte[]>
            {
                [CardAtlasCtexPath] = badAtlasBytes,
                ["res://scenes/unrelated_room.tscn"] = Encoding.UTF8.GetBytes("UNRELATED_MODIFIED")
            });

            PckArchive.Write(managedModPck, new Dictionary<string, byte[]>
            {
                [SelectPortraitCtexPath] = managedPortraitBytes
            });

            var report = VanillaIntegritySelfCheck.ScanArchives(
                gamePck,
                [
                    new VanillaIntegrityModSource("bad.native", "Bad Native", badModPck, IsManagedBySkinChanger: false),
                    new VanillaIntegrityModSource("managed.skin", "Managed Skin", managedModPck, IsManagedBySkinChanger: true)
                ],
                readLiveBytes: path => path.Equals(CardAtlasCtexPath, StringComparison.OrdinalIgnoreCase)
                    ? badAtlasBytes
                    : null);

            Require(report.CheckedCount == 4,
                $"真实 PCK 抽查应只查关键类别（4 项），不应全量扫无关场景，实际：{report.CheckedCount}。");
            Require(report.Findings.Count == 1 &&
                    report.ConfirmedCount == 1 &&
                    report.MainThreadFrames >= 1 &&
                    report.Findings[0].Path.Equals(CardAtlasCtexPath, StringComparison.OrdinalIgnoreCase) &&
                    report.Findings[0].ModId == "bad.native",
                $"真实 PCK 端到端扫描结果异常：{string.Join(" | ", VanillaIntegritySelfCheck.FormatLogLines(report))}");
        }
        finally
        {
            try { tempDir.Delete(recursive: true); } catch { }
        }
    }

    private static void VerifyMainMenuPatchAndSelfOverlayHooksBindCleanly()
    {
        Require(VanillaIntegritySelfCheck.AreSelfOverlayHooksBound(),
            "必须能在双分支成功绑定 SkinService.Sync / MountedOverlayOwners / RuntimeCanonicalDependencyPaths 反射字段。");

        var harmony = new Harmony("Gurio.SkinChanger.Tests.VanillaIntegrityHook");
        try
        {
            var patched = harmony.CreateClassProcessor(typeof(VanillaIntegrityMainMenuReadyPatch)).Patch();
            var target = AccessTools.Method(typeof(NMainMenu), nameof(NMainMenu._Ready)) ??
                         throw new InvalidOperationException("找不到 NMainMenu._Ready。");
            Require(patched.Count == 1 &&
                    Harmony.GetPatchInfo(target)?.Postfixes.Any(p => p.owner == harmony.Id) == true,
                "VanillaIntegrityMainMenuReadyPatch 必须能在正式版与测试版正常挂到 NMainMenu._Ready。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static Dictionary<string, VanillaIntegrityEntry> BuildBaseGameEntries() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [CardAtlasImportPath] = MakeEntry(
                CardAtlasImportPath,
                $"[remap]\npath.bptc=\"{CardAtlasCtexPath}\"\n"),
            [CardAtlasCtexPath] = MakeEntry(CardAtlasCtexPath, "VANILLA_CARD_ATLAS_BYTES"),
            [SelectPortraitImportPath] = MakeEntry(
                SelectPortraitImportPath,
                $"[remap]\npath.bptc=\"{SelectPortraitCtexPath}\"\n"),
            [SelectPortraitCtexPath] = MakeEntry(SelectPortraitCtexPath, "VANILLA_SELECT_PORTRAIT"),
            [BetaAncientBorderPath] = MakeEntry(
                BetaAncientBorderPath,
                "[ext_resource type=\"Texture2D\" path=\"res://images/atlases/compressed_atlas.png\" id=\"1\"]"),
            [BetaCompressedAtlasImportPath] = MakeEntry(
                BetaCompressedAtlasImportPath,
                $"[remap]\npath.s3tc=\"{BetaCompressedAtlasCtexPath}\"\n"),
            [BetaCompressedAtlasCtexPath] = MakeEntry(BetaCompressedAtlasCtexPath, "VANILLA_COMPRESSED_ATLS!"),
            // Unrelated game asset must NOT be included in the sampled set.
            ["res://images/misc/random_prop.png.import"] = MakeEntry(
                "res://images/misc/random_prop.png.import",
                "[remap]\npath=\"res://.godot/imported/random_prop.ctex\"\n")
        };

    private static VanillaIntegrityEntry MakeEntry(string path, string text) =>
        MakeEntry(path, Encoding.UTF8.GetBytes(text));

    private static VanillaIntegrityEntry MakeEntry(string path, byte[] bytes) =>
        new(
            path,
            (ulong)bytes.LongLength,
            RedirectTarget: null,
            Md5: MD5.HashData(bytes),
            ReadBytes: () => bytes);

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CountingMemoryVfsReader : IVanillaIntegrityVfsReader
    {
        private readonly Dictionary<string, byte[]> _files;
        private readonly Dictionary<string, long> _bytesByPath = new(StringComparer.OrdinalIgnoreCase);

        internal CountingMemoryVfsReader(Dictionary<string, byte[]> files)
        {
            _files = files;
        }

        internal int OpenCount { get; private set; }

        internal long TotalBytesRead { get; private set; }

        internal long BytesReadFor(string path) => _bytesByPath.GetValueOrDefault(path);

        public IVanillaIntegrityVfsHandle? OpenRead(string resourcePath, long offset)
        {
            OpenCount++;
            if (!_files.TryGetValue(resourcePath, out var bytes) || offset < 0 || offset > bytes.LongLength)
            {
                return null;
            }

            return new Handle(this, resourcePath, bytes, offset);
        }

        private sealed class Handle : IVanillaIntegrityVfsHandle
        {
            private readonly CountingMemoryVfsReader _owner;
            private readonly string _path;
            private readonly byte[] _bytes;
            private long _offset;

            internal Handle(CountingMemoryVfsReader owner, string path, byte[] bytes, long offset)
            {
                _owner = owner;
                _path = path;
                _bytes = bytes;
                _offset = offset;
            }

            public long Length => _bytes.LongLength;

            public byte[]? ReadChunk(int maxBytes)
            {
                if (maxBytes <= 0 || _offset >= _bytes.LongLength)
                {
                    return [];
                }

                var count = (int)Math.Min(maxBytes, _bytes.LongLength - _offset);
                var buffer = new byte[count];
                Buffer.BlockCopy(_bytes, (int)_offset, buffer, 0, count);
                _offset += count;
                _owner.TotalBytesRead += count;
                _owner._bytesByPath[_path] = _owner._bytesByPath.GetValueOrDefault(_path) + count;
                return buffer;
            }

            public void Dispose()
            {
            }
        }
    }
}
