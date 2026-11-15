using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Safety;

// 阶段 A 安全件（熔断 / 防崩溃）的离线用例：模拟写满、模拟连续抛异常、模拟循环触发与重入。
internal static class SafetyTests
{
    internal static void Run()
    {
        var logs = new List<string>();
        long now = 0;
        void Reset()
        {
            CircuitBreaker.ResetForTests();
            DiskGuard.ResetForTests();
            SafetyLog.ResetForTests();
            logs.Clear();
            now = 0;
            SafetyLog.Sink = logs.Add;
            SafetyLog.Clock = () => now;
            CircuitBreaker.Clock = () => now;
        }

        // ---- CircuitBreaker：失败过多跳闸 ----
        Reset();
        var trippedKeys = new List<string>();
        CircuitBreaker.Tripped += (key, _) => trippedKeys.Add(key);
        for (var i = 0; i < 4; i++)
        {
            CircuitBreaker.RecordFailure("merchant", new InvalidOperationException("boom"));
        }

        Require(!CircuitBreaker.IsOpen("merchant"), "默认策略 60 秒内失败 4 次还不应跳闸。");
        CircuitBreaker.RecordFailure("merchant", new InvalidOperationException("boom"));
        Require(CircuitBreaker.IsOpen("merchant"), "60 秒内第 5 次失败必须跳闸。");
        Require(!CircuitBreaker.TryEnter("merchant"), "跳闸后 TryEnter 必须返回 false。");
        Require(trippedKeys.SequenceEqual(["merchant"]), "跳闸事件必须恰好触发一次。");
        Require(logs.Count(line => line.Contains("[熔断]")) == 1, "跳闸只打一行 [熔断]。");
        Require(!CircuitBreaker.IsOpen("card"), "一个功能跳闸不能连带别的功能。");

        // ---- CircuitBreaker：一个订阅者出错不影响其他订阅者 ----
        Reset();
        var secondCalled = false;
        CircuitBreaker.Tripped += (_, _) => throw new InvalidOperationException("first");
        CircuitBreaker.Tripped += (_, _) => secondCalled = true;
        CircuitBreaker.Trip("background", "测试");
        Require(secondCalled, "第一个停用回调抛异常时，后面的回调仍必须收到通知。");

        // ---- CircuitBreaker：窗口外的旧失败不累计 ----
        Reset();
        for (var i = 0; i < 4; i++)
        {
            CircuitBreaker.RecordFailure("osty");
        }

        now += 61_000;
        CircuitBreaker.RecordFailure("osty");
        Require(!CircuitBreaker.IsOpen("osty"), "超出 60 秒窗口的旧失败不能算进来。");

        // ---- CircuitBreaker：调用过频（循环触发）跳闸 ----
        Reset();
        CircuitBreaker.Configure("rebuild", new BreakerPolicy(5, 1_000, MaxCalls: 10));
        var entered = 0;
        for (var i = 0; i < 50; i++)
        {
            if (CircuitBreaker.TryEnter("rebuild")) entered++;
            now += 10;
        }

        Require(entered == 10, $"1 秒内最多进入 10 次，实际 {entered}。");
        Require(CircuitBreaker.IsOpen("rebuild"), "调用过频必须跳闸。");

        Reset();
        CircuitBreaker.Configure("tick", new BreakerPolicy(5, 1_000, MaxCalls: 10));
        for (var i = 0; i < 100; i++)
        {
            Require(CircuitBreaker.TryEnter("tick"), "频率在上限以内（每 200 ms 一次）不应跳闸。");
            now += 200;
        }

        // ---- SafetyKeys：实际接入的阈值 ----
        Reset();
        SafetyKeys.ConfigureDefaults();
        for (var i = 0; i < 120; i++)
        {
            Require(CircuitBreaker.TryEnter(SafetyKeys.MountOverlay), "正常频率（60 秒内 120 次以内）的覆盖重建不能被挡。");
            now += 400;
        }

        Reset();
        SafetyKeys.ConfigureDefaults();
        var mounts = 0;
        for (var i = 0; i < 600; i++)
        {
            if (CircuitBreaker.TryEnter(SafetyKeys.MountOverlay)) mounts++;
            now += 16; // 每帧请求一次重建的失控循环
        }

        Require(mounts == 120 && CircuitBreaker.IsOpen(SafetyKeys.MountOverlay), $"每帧重建必须在第 121 次熔断，实际放行 {mounts}。");

        Reset();
        SafetyKeys.ConfigureDefaults();
        for (var i = 0; i < 19; i++)
        {
            CircuitBreaker.RecordFailure(SafetyKeys.TickMultiplayer);
        }

        Require(!CircuitBreaker.IsOpen(SafetyKeys.TickMultiplayer), "每帧子任务 60 秒内偶发 19 次失败不应停用。");
        CircuitBreaker.RecordFailure(SafetyKeys.TickMultiplayer);
        Require(CircuitBreaker.IsOpen(SafetyKeys.TickMultiplayer), "每帧子任务 60 秒内第 20 次失败必须停用。");
        Require(!CircuitBreaker.IsOpen(SafetyKeys.TickIntentDepth), "一个子任务停用不能连带其他子任务。");

        // ---- Safe：异常止步、计数、跳闸后跳过 ----
        Reset();
        var runs = 0;
        for (var i = 0; i < 8; i++)
        {
            Require(!Safe.Run("card", () => { runs++; throw new NullReferenceException("空节点"); }),
                "抛异常的动作 Safe.Run 必须返回 false 而不是往外抛。");
        }

        Require(runs == 5, $"第 5 次失败跳闸后不能再执行，实际执行 {runs} 次。");
        Require(logs.Count(line => line.Contains("[防崩溃]")) == 1, "同类异常 30 秒内只打一行 [防崩溃]。");
        Require(Safe.Get("card", () => 42, -1) == -1, "跳闸后 Safe.Get 必须直接返回 fallback。");
        Require(Safe.Get("event", () => 42, -1) == 42, "正常功能 Safe.Get 返回实际值。");
        Require(Safe.Get<string?>("event", () => throw new InvalidOperationException(), "fb") == "fb",
            "抛异常时 Safe.Get 返回 fallback。");

        // ---- SafetyLog：节流并带上省略次数 ----
        Reset();
        Require(SafetyLog.Throttled("k", "a"), "首条必须输出。");
        Require(!SafetyLog.Throttled("k", "a"), "30 秒内同 key 不再输出。");
        Require(!SafetyLog.Throttled("k", "a"), "30 秒内同 key 不再输出。");
        now += 31_000;
        Require(SafetyLog.Throttled("k", "a"), "过了节流期必须再输出。");
        Require(logs[^1].Contains("2 次"), "再次输出时要带上此前省略的次数。");

        // ---- LoopGuard：同步重入深度 ----
        Reset();
        var depthReached = 0;
        void Recurse(int level)
        {
            using var scope = LoopGuard.Enter("rebuild-creature", maxDepth: 3);
            if (!scope.Entered) return;
            depthReached = Math.Max(depthReached, level);
            Recurse(level + 1);
        }

        Recurse(1);
        Require(depthReached == 3, $"重入深度上限 3，实际到达 {depthReached}。");
        Require(LoopGuard.Depth("rebuild-creature") == 0, "离开后深度必须归零，否则下一次正常调用会被误挡。");
        using (var outer = LoopGuard.Enter("x", 1))
        {
            Require(outer.Entered, "第一层必须能进。");
            using var inner = LoopGuard.Enter("x", 1);
            Require(!inner.Entered, "深度 1 时第二层必须被挡。");
        }

        Require(LoopGuard.Enter("x", 1).Entered, "被挡的那层 Dispose 不能把计数减坏。");
        LoopGuard.Enter("x", 1).Dispose();

        // ---- DiskGuard：会话写盘总量 ----
        Reset();
        const long Mb = 1024 * 1024;
        DiskGuard.FreeSpaceProbe = _ => 100_000 * Mb;
        DiskGuard.SessionBudgetBytes = 1_000 * Mb;
        DiskGuard.EnsureCanWrite("C:/x.pck", 600 * Mb, "测试");
        DiskGuard.Account(600 * Mb);
        var refused = false;
        try
        {
            DiskGuard.EnsureCanWrite("C:/y.pck", 500 * Mb, "测试");
        }
        catch (DiskGuardException)
        {
            refused = true;
        }

        Require(refused, "累计将超过会话预算时必须拒绝写入。");
        Require(DiskGuard.IsTripped, "超预算必须让写盘熔断跳闸。");
        Require(!DiskGuard.CanWrite("C:/z.pck", "测试", out _), "跳闸后哪怕 0 字节也拒绝。");

        // ---- DiskGuard：剩余空间下限（不跳闸） ----
        Reset();
        long free = 3_000 * Mb;
        DiskGuard.FreeSpaceProbe = _ => free;
        DiskGuard.MinFreeBytes = 2_048 * Mb;
        DiskGuard.EnsureCanWrite("C:/a.pck", 500 * Mb, "测试");
        refused = false;
        try
        {
            DiskGuard.EnsureCanWrite("C:/b.pck", 1_000 * Mb, "测试");
        }
        catch (DiskGuardException exception)
        {
            refused = exception is IOException;
        }

        Require(refused, "写完会低于保底空间时必须拒绝，且异常必须是 IOException（走现有失败路径）。");
        Require(!DiskGuard.IsTripped, "空间不足只拒绝这一次，不让整个会话熔断。");
        free = 50_000 * Mb;
        DiskGuard.EnsureCanWrite("C:/b.pck", 1_000 * Mb, "测试");

        // ---- DiskGuard：拿不到剩余空间时不拦 ----
        Reset();
        DiskGuard.FreeSpaceProbe = _ => throw new IOException("no drive");
        DiskGuard.EnsureCanWrite("C:/c.pck", 10 * Mb, "测试");

        // ---- SkinChangerStorage.WriteCache：接入熔断 + 不重复记账 ----
        Reset();
        DiskGuard.FreeSpaceProbe = _ => 100_000 * Mb;
        var root = Path.Combine(Path.GetTempPath(), "sts2sc-safety-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var storage = new SkinChangerStorage(Path.Combine(root, "user"), Path.Combine(root, "temp"), "s1");
            storage.StartCacheSession();
            var file = Path.Combine(storage.CacheDirectory, "a.bin");
            Require(storage.WriteCache(file, () => File.WriteAllBytes(file, new byte[4096])), "正常缓存写入必须成功。");
            Require(DiskGuard.SessionWrittenBytes == 4096, $"直写的字节要记账，实际 {DiskGuard.SessionWrittenBytes}。");

            var nested = Path.Combine(storage.CacheDirectory, "b.bin");
            Require(storage.WriteCache(nested, () =>
            {
                File.WriteAllBytes(nested, new byte[1000]);
                DiskGuard.Account(1000); // 模拟内层（PckArchive）已经记过账
            }), "嵌套记账写入必须成功。");
            Require(DiskGuard.SessionWrittenBytes == 5096, $"内层已记的部分不能重复记，实际 {DiskGuard.SessionWrittenBytes}。");

            // 同名覆盖重写也要算写入量（循环重写同一个缓存文件是要挡的典型情况）。
            Require(storage.WriteCache(file, () => File.WriteAllBytes(file, new byte[4096])), "覆盖写入必须成功。");
            Require(DiskGuard.SessionWrittenBytes == 9192, $"同名覆盖要按写入量记账，实际 {DiskGuard.SessionWrittenBytes}。");

            // 真实 PckArchive 嵌在 WriteCache 里：内层记一次，外层不重复记。
            var pck = Path.Combine(storage.CacheDirectory, "real.pck");
            var beforePck = DiskGuard.SessionWrittenBytes;
            Require(storage.WriteCache(pck, () => STS2SkinChanger.Pck.PckArchive.Write(pck,
                new Dictionary<string, byte[]> { ["res://a.txt"] = new byte[300] })), "PCK 写入必须成功。");
            var pckDelta = DiskGuard.SessionWrittenBytes - beforePck;
            // 内层按对齐后的预估大小记账，可能比实际文件多出末尾对齐的不到 16 字节。
            Require(pckDelta >= new FileInfo(pck).Length && pckDelta < new FileInfo(pck).Length + 16,
                $"PCK 只记一次（≈文件大小 {new FileInfo(pck).Length}），实际记了 {pckDelta}。");

            // 打开失败（同名包被占用）时不能删掉路径上原有的完整包。
            var oldLength = new FileInfo(pck).Length;
            using (new FileStream(pck, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            {
                var openFailed = false;
                try
                {
                    STS2SkinChanger.Pck.PckArchive.Write(pck, new Dictionary<string, byte[]> { ["res://b.txt"] = new byte[10] });
                }
                catch (IOException)
                {
                    openFailed = true;
                }

                Require(openFailed, "被占用时写 PCK 应当失败。");
                Require(File.Exists(pck) && new FileInfo(pck).Length == oldLength, "打开失败时必须保留原有的完整包。");
            }

            CircuitBreaker.Trip(DiskGuard.BreakerKey, "测试");
            var called = false;
            Require(!storage.WriteCache(Path.Combine(storage.CacheDirectory, "c.bin"), () => called = true),
                "写盘熔断后 WriteCache 必须返回 false。");
            Require(!called, "写盘熔断后不能再执行写入动作。");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); } catch { }
        }

        CircuitBreaker.ResetForTests();
        DiskGuard.ResetForTests();
        SafetyLog.ResetForTests();
        SafetyLog.Sink = null;
        SafetyLog.Clock = static () => Environment.TickCount64;
        CircuitBreaker.Clock = static () => Environment.TickCount64;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
