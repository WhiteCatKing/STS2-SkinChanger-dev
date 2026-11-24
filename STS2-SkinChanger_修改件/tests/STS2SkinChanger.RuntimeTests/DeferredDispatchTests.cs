using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;
using STS2SkinChanger.Ui.EmperorCrabCompat;

internal static class DeferredDispatchTests
{
    internal static void Run()
    {
        try
        {
            VerifyInterleavedFifoOrderAndNestedCriticalHops();
            VerifyCrossFrameReadinessNotExhaustedInSingleFrame();
            VerifyExceptionPropagationUnchanged();
            VerifyBreadcrumbsHighRiskImmediateFlushRingAndFileCaps();
            VerifyHighRiskEnterOnlyFlushesEnterWithoutDone();
            VerifyPeriodicEmptyPollsOnlyCountWithoutBreadcrumbs();
            VerifyHalfHpCrabSkipsRetryWhenTargetModAbsent();
            VerifyPostReviewDeferredSitesUseWrapper();
            VerifyReentrancyWarningOnceWithoutTruncation();
            VerifyThreadSafeBackgroundEnqueue();
            VerifyCrashWatchEmbedsBreadcrumbSupport();
            Console.WriteLine("Deferred dispatch records passed: interleaved FIFO order, cross-frame progression, exception parity, high-risk enter-only & empty-poll counting, crab retry gate, site wiring, caps, reentrancy warning and thread-safe enqueue.");
        }
        finally
        {
            DeferredDispatch.ResetDefaultsForTests();
        }
    }

    private static void VerifyInterleavedFifoOrderAndNestedCriticalHops()
    {
        var queue = new Queue<Action>();
        ulong frame = 10;
        DeferredDispatch.ConfigureForTests(
            enqueue: queue.Enqueue,
            frameCounter: () => frame,
            breadcrumbDirectoryProvider: () => null);

        var executed = new List<string>();
        DeferredDispatch.CallDeferred("Test.OursA", "ownerA", () => executed.Add("OursA"));
        queue.Enqueue(() => executed.Add("ThirdParty"));
        DeferredDispatch.CallDeferred("Test.OursB", "ownerB", () => executed.Add("OursB"));

        DrainSinglePass(queue);
        Require(executed.SequenceEqual(["OursA", "ThirdParty", "OursB"]),
            "穿插顺序必须严格保持 我方甲 -> 第三方 -> 我方乙：" + string.Join(" -> ", executed));

        // Two-hop nested Critical (like ScrollListRebuild): outer hop enqueues inner hop,
        // which must execute AFTER an already-queued third-party callback, never collapsed into hop 1.
        executed.Clear();
        DeferredDispatch.CallDeferred("ScrollListRebuild.QueueSort", "scroll#1", () =>
        {
            executed.Add("OuterHop");
            DeferredDispatch.CallDeferred("ScrollListRebuild.RestoreScroll", "scroll#1", () =>
            {
                executed.Add("InnerHop");
            });
        });
        queue.Enqueue(() => executed.Add("ThirdPartyBetweenHops"));

        DrainSinglePass(queue);
        Require(executed.SequenceEqual(["OuterHop", "ThirdPartyBetweenHops"]),
            "嵌套两跳的首轮不得把第二跳提前吞进同一轮：" + string.Join(" -> ", executed));
        Require(queue.Count == 1, "嵌套第二跳必须留在下一轮队列中。");

        frame = 11;
        DrainSinglePass(queue);
        Require(executed.SequenceEqual(["OuterHop", "ThirdPartyBetweenHops", "InnerHop"]),
            "嵌套第二跳必须在下一轮按顺序执行：" + string.Join(" -> ", executed));
    }

    private static void VerifyCrossFrameReadinessNotExhaustedInSingleFrame()
    {
        var queue = new Queue<Action>();
        ulong frame = 1;
        DeferredDispatch.ConfigureForTests(
            enqueue: queue.Enqueue,
            frameCounter: () => frame,
            breadcrumbDirectoryProvider: () => null);

        var timeline = new List<string>();
        var attempts = 0;
        var succeededAtFrame = 0UL;

        void TryWhenReady()
        {
            attempts++;
            timeline.Add($"task@f{frame}");
            if (frame >= 3)
            {
                succeededAtFrame = frame;
                return;
            }

            DeferredDispatch.CallDeferred("CrossFrame.WaitReady", "model#1", TryWhenReady);
        }

        // Frame 1: enqueue our task, then a third-party callback.
        DeferredDispatch.CallDeferred("CrossFrame.WaitReady", "model#1", TryWhenReady);
        queue.Enqueue(() => timeline.Add($"thirdParty1@f{frame}"));
        DrainSinglePass(queue);

        Require(attempts == 1 && succeededAtFrame == 0,
            $"首帧不得提前耗光重试次数：attempts={attempts}, succeededAtFrame={succeededAtFrame}");
        Require(timeline.SequenceEqual(["task@f1", "thirdParty1@f1"]),
            "第 1 帧顺序必须先任务后第三方：" + string.Join(", ", timeline));

        // Frame 2: third-party callback interleaved after the re-enqueued task.
        frame = 2;
        queue.Enqueue(() => timeline.Add($"thirdParty2@f{frame}"));
        DrainSinglePass(queue);

        Require(attempts == 2 && succeededAtFrame == 0,
            $"第 2 帧模型未就绪时必须继续延后到第 3 帧：attempts={attempts}");
        Require(timeline.SequenceEqual(["task@f1", "thirdParty1@f1", "task@f2", "thirdParty2@f2"]),
            "第 2 帧穿插顺序不得反转：" + string.Join(", ", timeline));

        // Frame 3: model is ready now.
        frame = 3;
        DrainSinglePass(queue);
        Require(attempts == 3 && succeededAtFrame == 3 && queue.Count == 0,
            $"第 3 帧就绪后应精确成功且不再重入：attempts={attempts}, succeededAtFrame={succeededAtFrame}, remaining={queue.Count}");

        var records = DeferredDispatch.SnapshotRecords();
        Require(records.Length == 3 &&
                records[0].EnqueueFrame == 1 && records[0].ExecuteFrame == 1 && records[0].StreakCount == 1 &&
                records[1].EnqueueFrame == 1 && records[1].ExecuteFrame == 2 && records[1].StreakCount == 2 &&
                records[2].EnqueueFrame == 2 && records[2].ExecuteFrame == 3 && records[2].StreakCount == 3,
            "跨帧执行记录的入队帧、执行帧和连续重入次数必须准确。");
    }

    private static void VerifyExceptionPropagationUnchanged()
    {
        var queue = new Queue<Action>();
        var warnings = new List<string>();
        ulong frame = 5;
        DeferredDispatch.ConfigureForTests(
            enqueue: queue.Enqueue,
            frameCounter: () => frame,
            breadcrumbDirectoryProvider: () => null,
            warnLogger: warnings.Add);

        var expected = new InvalidOperationException("deferred-boom-42");
        DeferredDispatch.CallDeferred("Test.Throwing", "panel#9", () => throw expected);

        Exception? caught = null;
        try
        {
            queue.Dequeue()();
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        Require(ReferenceEquals(caught, expected),
            "包装器不得吞掉、改写或重复包装回调异常，必须原样抛出同一异常对象。");
        Require(warnings.Count == 0,
            "单次普通异常不得触发额外的重试告警日志。");

        var records = DeferredDispatch.SnapshotRecords();
        Require(records.Length == 1 &&
                records[0].Phase == "FAIL" &&
                records[0].Result.Contains("InvalidOperationException", StringComparison.Ordinal) &&
                records[0].Result.Contains("deferred-boom-42", StringComparison.Ordinal),
            "异常执行后必须在内存记录中标记 FAIL 和异常信息。");
    }

    private static void VerifyBreadcrumbsHighRiskImmediateFlushRingAndFileCaps()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sts2_dispatch_bc_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var queue = new Queue<Action>();
            ulong frame = 100;
            DeferredDispatch.ConfigureForTests(
                enqueue: queue.Enqueue,
                frameCounter: () => frame,
                breadcrumbDirectoryProvider: () => tempDir,
                ringCapacity: 8,
                flushEveryRecords: 10,
                maxBreadcrumbFileBytes: 1200,
                keepTailLinesOnTrim: 6);

            var breadcrumbFile = Path.Combine(tempDir, CrashDiagnostics.BreadcrumbFileName);

            // 1) Normal tasks stay in memory until flush threshold or explicit FlushBreadcrumbs.
            DeferredDispatch.CallDeferred("Normal.Step1", "obj1", () => { });
            DeferredDispatch.CallDeferred("Normal.Step2", "obj2", () => { });
            DrainSinglePass(queue);
            Require(!File.Exists(breadcrumbFile),
                "普通任务未达批量刷盘阈值前不得逐条写盘。");

            // 2) High-risk task MUST flush its ENTER record to disk BEFORE executing the callback body.
            var sawEnterOnDiskMidCallback = false;
            var sawPriorNormalRecordsBeforeEnter = false;
            DeferredDispatch.CallDeferred("Spine.HighRiskFit", "osty#7", () =>
            {
                if (File.Exists(breadcrumbFile))
                {
                    var textDuringRun = File.ReadAllText(breadcrumbFile);
                    sawEnterOnDiskMidCallback = textDuringRun.Contains("[ENTER][HIGH] task=Spine.HighRiskFit owner=osty#7", StringComparison.Ordinal);
                    sawPriorNormalRecordsBeforeEnter = textDuringRun.Contains("task=Normal.Step1", StringComparison.Ordinal);
                }

                // Simulate a fatal abort/exception mid-callback.
                throw new AccessViolationException("simulated-native-crash");
            }, DeferredDispatchRisk.HighRisk);

            try
            {
                DrainSinglePass(queue);
            }
            catch (AccessViolationException)
            {
            }

            Require(sawEnterOnDiskMidCallback,
                "高风险任务必须在回调体执行之前就把 ENTER 面包屑刷盘，保证原生闪退时看门进程能读到最后一个在途回调。");
            Require(sawPriorNormalRecordsBeforeEnter,
                "高风险任务刷盘前应按序号顺带落盘前置内存记录。");

            var afterText = File.ReadAllText(breadcrumbFile);
            Require(afterText.Contains("[FAIL][HIGH] task=Spine.HighRiskFit", StringComparison.Ordinal),
                "高风险任务抛错后应补写 FAIL 记录。");

            // 3) Verify CrashDiagnostics attaches recent breadcrumbs (capped at 20 lines).
            var section = CrashDiagnostics.FormatRecentBreadcrumbsSection(tempDir, 20);
            Require(section.Contains("延迟任务面包屑", StringComparison.Ordinal) &&
                    section.Contains("task=Spine.HighRiskFit", StringComparison.Ordinal),
                "CrashDiagnostics 必须能将面包屑文件附入诊断报告。");

            // 4) Ring buffer cap & file byte cap.
            for (var i = 0; i < 40; i++)
            {
                frame++;
                var index = i;
                DeferredDispatch.CallDeferred($"Burst.Task{index}", "burst", () => { });
            }
            DrainSinglePass(queue);
            DeferredDispatch.FlushBreadcrumbs();

            var snapshot = DeferredDispatch.SnapshotRecords();
            Require(snapshot.Length == 8,
                $"内存环形缓冲必须严格受上限约束：expected=8, actual={snapshot.Length}");
            Require(snapshot[^1].TaskName == "Burst.Task39",
                $"环形缓冲尾部应保留最新记录：actual={snapshot[^1].TaskName}");

            var fileInfo = new FileInfo(breadcrumbFile);
            Require(fileInfo.Exists && fileInfo.Length <= 1200,
                $"面包屑文件必须受字节上限裁切约束：limit=1200, actual={fileInfo.Length}");
            var finalText = File.ReadAllText(breadcrumbFile);
            Require(finalText.Contains("Burst.Task39", StringComparison.Ordinal),
                "裁切后的面包屑文件必须保留最新的尾部记录。");

            // 5) Verify 20-line tail limit in FormatRecentBreadcrumbsSection when > 20 lines exist.
            var manyLines = Enumerable.Range(1, 35).Select(n => $"line-{n}");
            File.WriteAllLines(breadcrumbFile, manyLines);
            var tailSection = CrashDiagnostics.FormatRecentBreadcrumbsSection(tempDir, 20);
            Require(!tailSection.Contains("line-15\n", StringComparison.Ordinal) &&
                    tailSection.Contains("line-16", StringComparison.Ordinal) &&
                    tailSection.Contains("line-35", StringComparison.Ordinal),
                "CrashDiagnostics 附带的面包屑必须只保留最后 20 条。");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static void VerifyHighRiskEnterOnlyFlushesEnterWithoutDone()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sts2_dispatch_enteronly_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var queue = new Queue<Action>();
            ulong frame = 200;
            DeferredDispatch.ConfigureForTests(
                enqueue: queue.Enqueue,
                frameCounter: () => frame,
                breadcrumbDirectoryProvider: () => tempDir,
                flushEveryRecords: 2);

            var breadcrumbFile = Path.Combine(tempDir, CrashDiagnostics.BreadcrumbFileName);
            var sawEnterOnDisk = false;
            DeferredDispatch.CallDeferred("OstyModelPreview.FitModel", "osty#1", () =>
            {
                sawEnterOnDisk = File.Exists(breadcrumbFile) &&
                                 File.ReadAllText(breadcrumbFile).Contains("[ENTER][HIGH] task=OstyModelPreview.FitModel", StringComparison.Ordinal);
            }, DeferredDispatchRisk.HighRiskEnterOnly);

            DrainSinglePass(queue);
            DeferredDispatch.FlushBreadcrumbs();

            Require(sawEnterOnDisk,
                "HighRiskEnterOnly 必须在执行前立即把 ENTER 落盘。");
            var diskLines = File.ReadAllLines(breadcrumbFile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            Require(diskLines.Length == 1 &&
                    diskLines[0].Contains("[ENTER][HIGH] task=OstyModelPreview.FitModel", StringComparison.Ordinal) &&
                    !diskLines.Any(line => line.Contains("[DONE]", StringComparison.Ordinal)),
                $"OstyModelPreview.FitModel 正常完成时磁盘只能落盘 ENTER，不得写 DONE：{string.Join(" | ", diskLines)}");

            var records = DeferredDispatch.SnapshotRecords();
            Require(records.Length == 2 && records[0].Phase == "ENTER" && records[1].Phase == "DONE",
                "HighRiskEnterOnly 内存环仍应保留 ENTER/DONE 状态便于测试与排查。");

            // Managed exceptions on HighRiskEnterOnly must still flush FAIL to disk.
            DeferredDispatch.CallDeferred("OstyModelPreview.FitModel", "osty#2", () =>
                throw new InvalidOperationException("fit-boom"), DeferredDispatchRisk.HighRiskEnterOnly);
            try
            {
                DrainSinglePass(queue);
            }
            catch (InvalidOperationException)
            {
            }

            var afterFail = File.ReadAllText(breadcrumbFile);
            Require(afterFail.Contains("[FAIL][HIGH] task=OstyModelPreview.FitModel owner=osty#2", StringComparison.Ordinal),
                "HighRiskEnterOnly 抛出托管异常时仍应把 FAIL 落盘。");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static void VerifyPeriodicEmptyPollsOnlyCountWithoutBreadcrumbs()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"sts2_dispatch_poll_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            ulong frame = 10;
            DeferredDispatch.ConfigureForTests(
                enqueue: action => action(),
                frameCounter: () => frame,
                breadcrumbDirectoryProvider: () => tempDir,
                flushEveryRecords: 4);

            var breadcrumbFile = Path.Combine(tempDir, CrashDiagnostics.BreadcrumbFileName);
            var poll = DeferredDispatch.WrapPollCallback("SkinPreloader.Tick", "global", () => false);
            for (var i = 0; i < 40; i++)
            {
                frame += 10;
                poll();
            }

            DeferredDispatch.FlushBreadcrumbs();
            Require(DeferredDispatch.GetIdlePollCount("SkinPreloader.Tick", "global") == 40,
                $"空轮询应准确累计次数，实际：{DeferredDispatch.GetIdlePollCount("SkinPreloader.Tick", "global")}");
            Require(DeferredDispatch.SnapshotRecords().Length == 0,
                "周期性空轮询不得向环形缓冲逐条追加面包屑记录。");
            Require(!File.Exists(breadcrumbFile),
                "周期性空轮询不得触发磁盘面包屑写入。");

            frame += 10;
            var didWork = DeferredDispatch.RunTrackedPoll("SkinPreloader.Tick", "global", () => true);
            Require(didWork, "真正干活的轮询步应返回 true。");
            var records = DeferredDispatch.SnapshotRecords();
            Require(records.Length == 1 &&
                    records[0].Phase == "DONE" &&
                    records[0].Result == "ok(idle=40)",
                $"首次真正干活的轮询记录应汇总前置空轮询次数，实际：{records.FirstOrDefault().Result}");
            Require(DeferredDispatch.GetIdlePollCount("SkinPreloader.Tick", "global") == 40,
                "总空轮询计数应保持累计。");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static void VerifyHalfHpCrabSkipsRetryWhenTargetModAbsent()
    {
        Require(!HalfHpCrabSkinPatch.IsTargetModLoaded(
                loadedAssemblyNames: ["Gurio.SkinChanger", "sts2"],
                loadedModTokens: ["STS2SkinChanger", "SomeOtherMod"],
                hasPatchType: false),
            "未加载 erowatcher 目标 Mod 时，IsTargetModLoaded 必须返回 false。");
        Require(HalfHpCrabSkinPatch.IsTargetModLoaded(
                loadedAssemblyNames: ["Gurio.SkinChanger", "erowatcher"],
                loadedModTokens: [],
                hasPatchType: false),
            "已加载目标程序集 erowatcher 时，IsTargetModLoaded 必须返回 true。");
        Require(HalfHpCrabSkinPatch.IsTargetModLoaded(
                loadedAssemblyNames: ["Gurio.SkinChanger"],
                loadedModTokens: ["EroWatcher_Pack"],
                hasPatchType: false),
            "目标 Mod 清单或包名包含 erowatcher 时，IsTargetModLoaded 必须返回 true。");

        var installMethod = AccessTools.Method(typeof(HalfHpCrabSkinPatch), nameof(HalfHpCrabSkinPatch.Install))
                            ?? throw new InvalidOperationException("找不到 HalfHpCrabSkinPatch.Install。");
        var calls = PatchProcessor.GetOriginalInstructions(installMethod)
            .Select(i => i.operand)
            .OfType<MethodInfo>()
            .Select(m => m.Name)
            .ToArray();
        Require(calls.Contains(nameof(HalfHpCrabSkinPatch.IsTargetModLoaded)),
            "HalfHpCrabSkinPatch.Install 必须在启动 5 分钟重试前调用 IsTargetModLoaded 检查目标 Mod。");
    }

    private static void VerifyPostReviewDeferredSitesUseWrapper()
    {
        AssertCallsDeferredDispatch(typeof(FakeMerchantEventFrame), nameof(FakeMerchantEventFrame.Schedule), nameof(DeferredDispatch.CallDeferred));
        AssertCallsDeferredDispatch(typeof(CharacterSelectUiScope), nameof(CharacterSelectUiScope.Reopen), nameof(DeferredDispatch.CallDeferred));
        AssertCallsDeferredDispatch(typeof(MerchantRuntimeAppearance), nameof(MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady), nameof(DeferredDispatch.CallDeferred));
        AssertCallsDeferredDispatch(typeof(ContextualSkinControls), nameof(ContextualSkinControls.ApplyProviderVariantLabels), nameof(DeferredDispatch.CallDeferred));
        AssertCallsDeferredDispatch(typeof(SkinPreloader), "EnsureTimer", nameof(DeferredDispatch.WrapPollCallback));
        AssertCallsDeferredDispatch(typeof(DeferredProviderActivator), "EnsureTimer", nameof(DeferredDispatch.WrapPollCallback));

        var ostyType = typeof(OstyModelPreviewControls).Assembly.GetType("STS2SkinChanger.Ui.OstyModelPreviewPanel")
                       ?? throw new InvalidOperationException("找不到 OstyModelPreviewPanel。");
        var scheduleFit = AccessTools.Method(ostyType, "ScheduleFit")
                          ?? throw new InvalidOperationException("找不到 OstyModelPreviewPanel.ScheduleFit。");
        var fitInstructions = PatchProcessor.GetOriginalInstructions(scheduleFit);
        Require(fitInstructions.Any(i => i.operand is MethodInfo m &&
                                         m.DeclaringType == typeof(DeferredDispatch) &&
                                         m.Name == nameof(DeferredDispatch.CallDeferred)),
            "OstyModelPreviewPanel.ScheduleFit 必须走 DeferredDispatch.CallDeferred。");
        Require(fitInstructions.Any(i => i.opcode.Name == "ldc.i4.2"),
            "OstyModelPreviewPanel.ScheduleFit 必须传 DeferredDispatchRisk.HighRiskEnterOnly (2)。");

        // Also verify inner compiler-generated state-machine / closure callbacks (FitStep, ProbeStep,
        // and the two Spine-ready inner callbacks) are wired into DeferredDispatch with exact risk tags.
        AssertInnerCallbackCallsDeferredDispatch(
            typeof(FakeMerchantEventFrame),
            "FakeMerchantEventFrame.FitStep",
            nameof(DeferredDispatch.RunTrackedPoll));
        AssertInnerCallbackCallsDeferredDispatch(
            typeof(FakeMerchantBattleFigure),
            "FakeMerchantBattleFigure.ProbeStep",
            nameof(DeferredDispatch.RunTrackedPoll));
        AssertInnerCallbackCallsDeferredDispatch(
            typeof(FakeMerchantBattleFigure),
            "FakeMerchantBattleFigure.AttachSpineReady",
            nameof(DeferredDispatch.RunTracked),
            expectedRiskOpcode: "ldc.i4.2");
        AssertInnerCallbackCallsDeferredDispatch(
            typeof(MerchantRuntimeAppearance),
            "MerchantAppearance.SpineNodeReady",
            nameof(DeferredDispatch.RunTracked),
            expectedRiskOpcode: "ldc.i4.1");
    }

    private static void AssertInnerCallbackCallsDeferredDispatch(
        Type owner,
        string expectedTag,
        string expectedDispatchMethod,
        string? expectedRiskOpcode = null)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var candidateMethods = new List<MethodBase>();
        candidateMethods.AddRange(owner.GetMethods(flags).Where(m => m.GetMethodBody() != null && !m.ContainsGenericParameters));
        foreach (var nested in owner.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (nested.ContainsGenericParameters)
            {
                continue;
            }

            candidateMethods.AddRange(nested.GetMethods(flags).Where(m => m.GetMethodBody() != null && !m.ContainsGenericParameters));
        }

        var matched = false;
        foreach (var method in candidateMethods)
        {
            var instructions = PatchProcessor.GetOriginalInstructions(method);
            var hasTag = instructions.Any(i => i.operand is string s && s == expectedTag);
            var hasCall = instructions.Any(i => i.operand is MethodInfo m &&
                                                m.DeclaringType == typeof(DeferredDispatch) &&
                                                m.Name == expectedDispatchMethod);
            if (!hasTag || !hasCall)
            {
                continue;
            }

            if (expectedRiskOpcode != null && !instructions.Any(i => i.opcode.Name == expectedRiskOpcode))
            {
                continue;
            }

            matched = true;
            break;
        }

        Require(matched,
            $"{owner.Name} 内层回调必须携带标签 \"{expectedTag}\" 并调用 DeferredDispatch.{expectedDispatchMethod}" +
            (expectedRiskOpcode != null ? $"（风险等级操作码 {expectedRiskOpcode}）" : string.Empty) + "。");
    }

    private static void AssertCallsDeferredDispatch(Type owner, string methodName, string expectedDispatchMethod)
    {
        var method = AccessTools.Method(owner, methodName)
                     ?? throw new InvalidOperationException($"找不到 {owner.Name}.{methodName}。");
        var instructions = PatchProcessor.GetOriginalInstructions(method);
        var called = instructions
            .Select(i => i.operand)
            .OfType<MethodInfo>()
            .ToArray();
        Require(called.Any(m => m.DeclaringType == typeof(DeferredDispatch) && m.Name == expectedDispatchMethod),
            $"{owner.Name}.{methodName} 必须调用 DeferredDispatch.{expectedDispatchMethod}。");
        Require(!called.Any(m => m.Name == "CallDeferred" && m.DeclaringType != typeof(DeferredDispatch)),
            $"{owner.Name}.{methodName} 不得再直接调用裸 CallDeferred。");
    }

    private static void VerifyReentrancyWarningOnceWithoutTruncation()
    {
        Require(DeferredDispatch.DefaultReentrancyWarnThreshold == 50,
            "默认连续重入告警阈值应为 50。");

        var queue = new Queue<Action>();
        var warnings = new List<string>();
        ulong frame = 1;
        DeferredDispatch.ConfigureForTests(
            enqueue: queue.Enqueue,
            frameCounter: () => frame,
            breadcrumbDirectoryProvider: () => null,
            warnLogger: warnings.Add,
            reentrancyWarnThreshold: 5);

        var count = 0;
        void Loop()
        {
            count++;
            if (count < 12)
            {
                DeferredDispatch.CallDeferred("Loop.InfiniteStyle", "widget#1", Loop);
            }
        }

        DeferredDispatch.CallDeferred("Loop.InfiniteStyle", "widget#1", Loop);
        while (queue.Count > 0)
        {
            DrainSinglePass(queue);
            frame++;
        }

        Require(count == 12,
            $"只观察不干预：超过阈值后任务必须照常继续执行完毕，expected=12, actual={count}");
        Require(warnings.Count == 1 &&
                warnings[0].Contains("Loop.InfiniteStyle", StringComparison.Ordinal) &&
                warnings[0].Contains("widget#1", StringComparison.Ordinal) &&
                warnings[0].Contains("次数=6", StringComparison.Ordinal),
            $"连续重入超阈值必须且仅告警一次并带任务名/归属/次数：count={warnings.Count}, msg={warnings.FirstOrDefault()}");
    }

    private static void VerifyThreadSafeBackgroundEnqueue()
    {
        var sync = new object();
        var queue = new Queue<Action>();
        DeferredDispatch.ConfigureForTests(
            enqueue: action => { lock (sync) queue.Enqueue(action); },
            frameCounter: () => 1,
            breadcrumbDirectoryProvider: () => null);

        var executed = 0;
        var tasks = Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (var i = 0; i < 16; i++)
            {
                DeferredDispatch.CallDeferred("Bg.Enqueue", $"worker-{worker}", () => Interlocked.Increment(ref executed));
            }
        })).ToArray();
        Task.WaitAll(tasks);

        while (true)
        {
            Action? next;
            lock (sync)
            {
                next = queue.Count > 0 ? queue.Dequeue() : null;
            }
            if (next == null) break;
            next();
        }

        Require(executed == 128,
            $"跨线程并发入队必须线程安全且不丢任务：expected=128, actual={executed}");
    }

    private static void VerifyCrashWatchEmbedsBreadcrumbSupport()
    {
        using var stream = typeof(CrashDiagnostics).Assembly.GetManifestResourceStream("STS2SkinChanger.Diagnostics.CrashWatch.ps1")
                           ?? throw new InvalidOperationException("找不到内嵌 CrashWatch.ps1 资源。");
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd();
        Require(script.Contains(CrashDiagnostics.BreadcrumbFileName, StringComparison.Ordinal) &&
                script.Contains("Select-Object -Last 20", StringComparison.Ordinal),
            "CrashWatch.ps1 必须读取 deferred-breadcrumbs.log 并将最后 20 条附进 diagnostic.txt。");
    }

    private static void DrainSinglePass(Queue<Action> queue)
    {
        var count = queue.Count;
        for (var i = 0; i < count; i++)
        {
            queue.Dequeue()();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
