using STS2SkinChanger.Core.Features;
using STS2SkinChanger.Core.Safety;

// 阶段 C 骨架：功能模块按需装卸的离线用例。
internal static class FeatureRegistryTests
{
    private sealed class FakeInstaller : IPatchInstaller
    {
        public readonly HashSet<Type> Installed = [];
        public Type? FailOn;
        public void Install(Type patchClass)
        {
            if (patchClass == FailOn) throw new InvalidOperationException("目标方法不存在");
            Installed.Add(patchClass);
        }

        public void Uninstall(Type patchClass) => Installed.Remove(patchClass);
    }

    private sealed class FakeFeature(string id) : ISkinFeature
    {
        public string Id { get; } = id;
        public bool? Should = null;
        public bool InUse;
        public int Activated;
        public int Deactivating;
        public bool? ShouldBeActive() => Should;
        public bool IsInUse() => InUse;
        public void OnActivated() => Activated++;
        public void OnDeactivating() => Deactivating++;
    }

    private sealed class PatchA;
    private sealed class PatchB;
    private sealed class PatchC;

    internal static void Run()
    {
        CircuitBreaker.ResetForTests();
        SafetyLog.Sink = null;
        var installer = new FakeInstaller();
        var registry = new FeatureRegistry(installer);
        var merchant = new FakeFeature("merchant");
        registry.Register(merchant, [typeof(PatchA), typeof(PatchB)]);

        Require(installer.Installed.SetEquals([typeof(PatchA), typeof(PatchB)]), "注册后补丁默认在位（和过去全装一致）。");
        Require(merchant.Activated == 0, "启动期注册不调用 OnActivated。");
        var gen0 = registry.Generation("merchant");

        // 目录还没就绪（null）：保持现状，不卸。
        registry.Reevaluate();
        Require(registry.IsInstalled("merchant"), "目录未就绪时不能卸补丁。");

        // 不需要但场上还在用：先不卸。
        merchant.Should = false;
        merchant.InUse = true;
        registry.Reevaluate();
        Require(registry.IsInstalled("merchant"), "还在用的模块不能卸补丁。");

        // 不需要且不在用：卸，先清理再摘，代次推进，旧回调失效。
        merchant.InUse = false;
        registry.Reevaluate();
        Require(!registry.IsInstalled("merchant") && installer.Installed.Count == 0, "不需要且不在用时必须摘掉全部补丁。");
        Require(merchant.Deactivating == 1, "停用前必须调用一次清理。");
        Require(!registry.IsCurrent("merchant", gen0), "停用后旧代次的延迟回调必须判定为过期。");

        // 又需要了：重新装，调用 OnActivated，新代次有效。
        merchant.Should = true;
        registry.Reevaluate();
        Require(registry.IsInstalled("merchant") && merchant.Activated == 1, "重新需要时必须装回并通知。");
        Require(registry.IsCurrent("merchant", registry.Generation("merchant")), "当前代次必须有效。");
        Require(!registry.IsCurrent("merchant", gen0), "更早的代次依然无效（A→B→A 不能复活旧回调）。");

        // 熔断：在用时先标记不卸，不在用后下一轮卸掉；之后即使"需要"也不再装回。
        merchant.InUse = true;
        CircuitBreaker.Trip(FeatureRegistry.BreakerKey("merchant"), "测试");
        Require(registry.IsInstalled("merchant"), "熔断时场上还在用，不能立刻摘补丁。");
        merchant.InUse = false;
        registry.Reevaluate();
        Require(!registry.IsInstalled("merchant"), "熔断后不在用时必须摘掉。");
        registry.Reevaluate();
        Require(!registry.IsInstalled("merchant"), "熔断的模块本会话不能再被装回。");

        // 装一半失败：已装的撤回，模块保持未启用，不留半套。
        var card = new FakeFeature("card") { Should = true };
        installer.FailOn = typeof(PatchC);
        registry.Register(card, [typeof(PatchA), typeof(PatchC)]);
        Require(!registry.IsInstalled("card") && !installer.Installed.Contains(typeof(PatchA)), "装一半失败必须把已装的撤掉。");

        // 判断函数自己抛异常：ShouldBeActive 当"不知道"保持现状；IsInUse 当"在用"不卸。
        installer.FailOn = null;
        var bg = new ThrowingFeature("background");
        registry.Register(bg, [typeof(PatchB)]);
        registry.Reevaluate();
        Require(registry.IsInstalled("background"), "判断出错时保持现状。");

        registry.DetachForTests();
        CircuitBreaker.ResetForTests();
        Console.WriteLine("Feature registry passed: default-on until the catalog is ready, in-use modules are never unpatched, generations retire stale callbacks, tripped modules stay off, partial installs roll back.");
    }

    private sealed class ThrowingFeature(string id) : ISkinFeature
    {
        public string Id { get; } = id;
        public bool? ShouldBeActive() => throw new InvalidOperationException();
        public bool IsInUse() => throw new InvalidOperationException();
        public void OnActivated() { }
        public void OnDeactivating() { }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
