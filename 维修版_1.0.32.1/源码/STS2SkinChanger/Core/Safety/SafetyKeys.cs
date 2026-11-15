namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 熔断 key 与各自的阈值集中在这里，Entry 启动时调用 <see cref="ConfigureDefaults"/>。
/// 阈值按"正常使用远够不到"来定，目标是挡住每帧或每秒反复触发的失控情况，不是限制玩家操作。
/// </summary>
internal static class SafetyKeys
{
    /// <summary>外观覆盖重建（<c>SkinService.MountOverlay</c>）：60 秒内超过 120 次判定为循环触发。
    /// 正常换肤一次一轮，连点也到不了每秒 2 次；每帧重试的失控循环 2 秒内就会撞线。</summary>
    public const string MountOverlay = "mount-overlay";

    /// <summary>外观面板排队的选择（每帧驱动）。</summary>
    public const string AppearanceQueue = "appearance-queue";

    // 常驻每帧 tick（MultiplayerSkinSync.Tick）里的子任务，各自独立熔断，一个坏了不拖累其他。
    public const string TickVariantHotkeyBridge = "tick:variant-hotkey-bridge";
    public const string TickVariantHotkeyPoller = "tick:variant-hotkey-poller";
    public const string TickMerchantAfterLoad = "tick:merchant-after-load";
    public const string TickAppearanceQueue = "tick:appearance-queue";
    public const string TickIntentDepth = "tick:intent-depth";
    public const string TickMultiplayer = "tick:multiplayer";
    public const string TickFeatures = "tick:features";

    public static void ConfigureDefaults()
    {
        CircuitBreaker.Configure(MountOverlay, new BreakerPolicy(MaxFailures: int.MaxValue, WindowMilliseconds: 60_000, MaxCalls: 120));

        // 每帧任务：60 秒内失败 20 次才停（偶发一两帧节点未就绪不至于停掉功能）。
        var perFrame = new BreakerPolicy(MaxFailures: 20, WindowMilliseconds: 60_000);
        foreach (var key in new[]
                 {
                     AppearanceQueue, TickVariantHotkeyBridge, TickVariantHotkeyPoller, TickMerchantAfterLoad,
                     TickAppearanceQueue, TickIntentDepth, TickMultiplayer, TickFeatures,
                 })
        {
            CircuitBreaker.Configure(key, perFrame);
        }
    }
}
