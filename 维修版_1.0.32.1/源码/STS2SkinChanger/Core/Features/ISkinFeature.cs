using System;

namespace STS2SkinChanger.Core.Features;

/// <summary>
/// 一个可以按需开关的功能模块（需求 3、4）。主干只认识这个接口；
/// 商人、背景、卡牌等功能各自实现，长在自己的"枝条"目录里。
///
/// 生命周期规矩（来自商人勘察的三条合同）：
/// <list type="number">
/// <item>是否需要：<see cref="ShouldBeActive"/> 只读目录 / 配置快照，必须便宜（不扫盘、不读 DLL），会被定时调用。</item>
/// <item>是否在用：<see cref="IsInUse"/> 报告"现在场上有没有依赖本模块补丁的活对象"（活房间、预览、切换中）。
/// 在用时绝不卸补丁，等它结束再卸。</item>
/// <item>代次：每次激活 / 停用都会让 <see cref="FeatureRegistry.Generation"/> 加一。延迟回调要在排队时记下代次，
/// 执行时用 <see cref="FeatureRegistry.IsCurrent"/> 核对，过期就直接退出，不再改节点。</item>
/// </list>
/// 本模块的 Harmony 补丁类用 <see cref="FeaturePatchAttribute"/> 标记，Entry 启动时不再统一安装它们，
/// 由注册表在激活时装、停用时精确摘掉。
/// </summary>
internal interface ISkinFeature
{
    /// <summary>模块 id，同时是它的熔断 key 前缀（<c>patch:&lt;id&gt;</c>）。</summary>
    string Id { get; }

    /// <summary>按当前配置和目录，这个模块是否需要（便宜、可反复调用）。目录还没就绪时返回 null（表示"还不知道"）。</summary>
    bool? ShouldBeActive();

    /// <summary>场上是否还有依赖本模块补丁的活对象。</summary>
    bool IsInUse();

    /// <summary>补丁装好之后调用（主线程）。可以在这里对已存在的场景做一次受控刷新。</summary>
    void OnActivated();

    /// <summary>补丁摘掉之前调用（主线程）。清理本模块自己加的节点、状态；不得抛异常到外面。</summary>
    void OnDeactivating();
}

/// <summary>标记某个 Harmony 补丁类归属哪个功能模块。被标记的类由 <see cref="FeatureRegistry"/> 管理装卸。</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class FeaturePatchAttribute(string featureId) : Attribute
{
    public string FeatureId { get; } = featureId;
}
