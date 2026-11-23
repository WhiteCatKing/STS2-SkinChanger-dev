using STS2SkinChanger.Core;

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
            VerifyReentrancyWarningOnceWithoutTruncation();
            VerifyThreadSafeBackgroundEnqueue();
            VerifyCrashWatchEmbedsBreadcrumbSupport();
            Console.WriteLine("Deferred dispatch records passed: interleaved FIFO order, cross-frame progression, exception parity, high-risk immediate breadcrumbs, caps, reentrancy warning and thread-safe enqueue.");
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
