using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using STS2SkinChanger.Core;

internal static class ProviderAssemblyOwnershipTests
{
    private static int _watchdogCalls;
    private static bool SkipEngine() => false;
    private static bool Watchdog() { _watchdogCalls++; return false; }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Fixture() => 1;
    private static void Callback(ref int __result) => __result++;

    internal static void Run()
    {
        const string identity = "Fixture, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Require(ProviderAssemblyReusePolicy.Matches("C:/mods/a.dll", identity, "", identity),
            "完整 identity 相同的内存程序集可复用。");
        Require(ProviderAssemblyReusePolicy.Matches("C:/mods/a.dll", null, "C:/mods/a.dll", null),
            "同 Location 可复用。");
        Require(!ProviderAssemblyReusePolicy.Matches("C:/mods/a.dll", identity,
            "C:/other/a.dll", identity.Replace("1.0.0.0", "2.0.0.0")), "同简单名不同版本/位置不可误认。");
        Require(!ProviderAssemblyReusePolicy.Matches("C:/mods/a.dll", null, "", null),
            "没有位置/完整 identity 依据时不可复用。");

        var harmony = new Harmony("SkinChanger.Tests.ProviderOwnership");
        var loader = typeof(ManagedSkinModLoader);
        var adopted = (HashSet<Assembly>)AccessTools.Field(loader, "AdoptedGameAssemblies").GetValue(null)!;
        var assembly = typeof(ProviderAssemblyOwnershipTests).Assembly;
        var target = AccessTools.Method(typeof(ProviderAssemblyOwnershipTests), nameof(Fixture));
        var callback = AccessTools.Method(typeof(ProviderAssemblyOwnershipTests), nameof(Callback));
        var skip = new HarmonyMethod(AccessTools.Method(typeof(ProviderAssemblyOwnershipTests), nameof(SkipEngine)));
        try
        {
            harmony.Patch(AccessTools.Method(typeof(Logger), "GetIsRunningFromGodotEditor"), prefix: skip);
            harmony.Patch(AccessTools.Method(typeof(ConsoleLogPrinter), "Print"), prefix: skip);
            harmony.Patch(target, postfix: new HarmonyMethod(callback));
            adopted.Add(assembly);
            var capture = AccessTools.Method(loader, "CaptureProviderPatches");
            var metadata = (IEnumerable)capture.Invoke(null, [assembly])!;
            var patch = metadata.Cast<object>().Single(item =>
                Equals(item.GetType().GetProperty("Callback")!.GetValue(item), callback));
            var patches = Array.CreateInstance(patch.GetType(), 1);
            patches.SetValue(patch, 0);
            Require(Fixture() == 2, "模拟作者回调应在位。");
            AccessTools.Method(loader, "UnpatchProviderCallbacks").Invoke(null, [patches]);
            Require(Fixture() == 2, "切走不能卸载被复用的作者回调。");
            AccessTools.Method(loader, "PatchProviderCallbacks").Invoke(null, [patches]);
            Require(Harmony.GetPatchInfo(target)!.Postfixes.Count(p => p.PatchMethod == callback) == 1,
                "切回不能重复安装未卸载的回调。");
            adopted.Remove(assembly);
            AccessTools.Method(loader, "UnpatchProviderCallbacks").Invoke(null, [patches]);
            Require(Fixture() == 1, "普通受管回调仍可卸载。");

            // Never start the real exit thread. Exercise the production postfix's two guards.
            var quit = typeof(QuitCacheCleanup);
            harmony.CreateClassProcessor(quit).Patch();
            Require(Harmony.GetPatchInfo(AccessTools.Method(typeof(MegaCrit.Sts2.Core.Nodes.NGame), "Quit"))!
                .Postfixes.Any(p => p.owner == harmony.Id && p.PatchMethod.DeclaringType == quit),
                "保存后观察点必须能绑定当前游戏的实际 Quit，包含 __runOriginal 参数。");
            harmony.Patch(AccessTools.Method(quit, "ArmForceExitWatchdog"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(ProviderAssemblyOwnershipTests), nameof(Watchdog))));
            var coordinator = AccessTools.Field(quit, "Coordinator").GetValue(null)!;
            var allow = AccessTools.Property(coordinator.GetType(), "AllowNativeQuit").GetSetMethod(true)!;
            var postfix = AccessTools.Method(quit, "Postfix");
            allow.Invoke(coordinator, [false]);
            postfix.Invoke(null, [true]);
            Require(_watchdogCalls == 0, "初始化/缓存清理尚未放行时不计时。");
            allow.Invoke(coordinator, [true]);
            postfix.Invoke(null, [false]);
            Require(_watchdogCalls == 0, "原版被 Prefix 抑制时不计时。");
            postfix.Invoke(null, [true]);
            Require(_watchdogCalls == 1, "只有原版 Quit 正常返回后才能启动计时。");
            allow.Invoke(coordinator, [false]);
            Console.WriteLine("Provider ownership and quit ordering tests passed.");
        }
        finally
        {
            adopted.Remove(assembly);
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
