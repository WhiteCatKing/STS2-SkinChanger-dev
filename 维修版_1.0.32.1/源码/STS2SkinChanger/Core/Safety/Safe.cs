using System;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 补丁体和回调的统一包裹（需求 2）。所有异常在这里止步：计入该功能的熔断器、节流打日志，
/// 绝不顺着 Harmony 补丁冒进游戏本体或 Godot 原生层（报告"经验教训 6"）。
/// 熔断器跳闸后直接跳过，不再执行。
/// <code>
/// static void Postfix(NMerchantRoom __instance) =>
///     Safe.Run("merchant", () => MerchantRuntimeAppearance.Refresh(__instance));
/// </code>
/// </summary>
internal static class Safe
{
    /// <returns>true 表示动作完整执行；false 表示已熔断跳过或执行中抛了异常。</returns>
    public static bool Run(string feature, Action action)
    {
        if (!CircuitBreaker.TryEnter(feature))
        {
            return false;
        }

        try
        {
            action();
            return true;
        }
        catch (Exception exception)
        {
            Report(feature, exception);
            return false;
        }
    }

    public static T Get<T>(string feature, Func<T> func, T fallback)
    {
        if (!CircuitBreaker.TryEnter(feature))
        {
            return fallback;
        }

        try
        {
            return func();
        }
        catch (Exception exception)
        {
            Report(feature, exception);
            return fallback;
        }
    }

    /// <summary>调用方自己 catch 到异常时也走同一套计数和日志。</summary>
    public static void Report(string feature, Exception exception)
    {
        var root = exception.GetBaseException();
        SafetyLog.Throttled(
            "safe:" + feature + ":" + root.GetType().FullName,
            $"[防崩溃] 功能「{feature}」出错已拦下：{root.GetType().Name}: {root.Message}");
        CircuitBreaker.RecordFailure(feature, exception);
    }
}
