using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Core.Features;

/// <summary>用 Harmony 按补丁类精确装卸（只动这个类自己的补丁，不碰别人挂在同一方法上的）。</summary>
internal sealed class HarmonyPatchInstaller(Harmony harmony) : IPatchInstaller
{
    public void Install(Type patchClass) => harmony.CreateClassProcessor(patchClass).Patch();

    public void Uninstall(Type patchClass) => harmony.CreateClassProcessor(patchClass).Unpatch();
}

/// <summary>
/// 启动接线：建注册表、注册已迁移的模块、把它们的补丁类从 Entry 的"全装"里拿出来。
/// 新模块迁好后只需在 <see cref="CreateFeatures"/> 里加一行。
/// </summary>
internal static class FeatureBootstrap
{
    private const double ReevaluateIntervalSeconds = 2.0;
    private static double _sinceReevaluate;

    /// <summary>已迁移的功能模块。迁移一个加一个。</summary>
    private static IEnumerable<ISkinFeature> CreateFeatures()
    {
        yield return new Merchant.MerchantFeature();
        yield return new Event.EventFeature();
        yield return new Background.BackgroundFeature();
        yield return new Card.CardFeature();
        yield return new StatefulCard.StatefulCardFeature();
        yield return new Ancient.AncientFeature();
        yield return new Monster.MonsterFeature();
        yield return new Character.CharacterFeature();
        yield break;
    }

    /// <summary>Entry 统一安装时要跳过的类型：凡是标了 <see cref="FeaturePatchAttribute"/> 且对应模块已注册的。</summary>
    public static bool IsManagedByFeature(Type type) =>
        FeatureRegistry.Current is { } registry &&
        type.GetCustomAttribute<FeaturePatchAttribute>() is { } attribute &&
        registry.FeatureIds.Contains(attribute.FeatureId);

    public static void Initialize(Harmony harmony, IEnumerable<Type> loadableTypes)
    {
        var registry = new FeatureRegistry(new HarmonyPatchInstaller(harmony));
        FeatureRegistry.Current = registry;
        var byFeature = loadableTypes
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<FeaturePatchAttribute>()))
            .Where(pair => pair.Attribute != null && pair.Type.IsDefined(typeof(HarmonyPatch), inherit: false))
            .GroupBy(pair => pair.Attribute!.FeatureId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(pair => pair.Type).ToArray(), StringComparer.Ordinal);

        foreach (var feature in CreateFeatures())
        {
            var classes = byFeature.GetValueOrDefault(feature.Id) ?? [];
            registry.Register(feature, classes);
            SafetyLog.Always($"[模块] 已注册「{feature.Id}」：{classes.Length} 个补丁类，目录就绪后按需决定去留。");
        }

        foreach (var orphan in byFeature.Keys.Where(id => !registry.FeatureIds.Contains(id)))
        {
            SafetyLog.Always($"[模块] 补丁类标记了模块「{orphan}」但该模块没注册，按旧方式统一安装。");
        }
    }

    /// <summary>目录就绪后调用一次；之后由每帧 tick 每 2 秒调一次（只读快照，很便宜）。</summary>
    public static void Reevaluate() => FeatureRegistry.Current?.Reevaluate();

    public static void Tick(double delta)
    {
        _sinceReevaluate += delta;
        if (_sinceReevaluate < ReevaluateIntervalSeconds)
        {
            return;
        }

        _sinceReevaluate = 0;
        Reevaluate();
    }
}
