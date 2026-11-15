using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace STS2SkinChanger.Core.Features;

/// <summary>
/// 把带 <see cref="FeaturePatchAttribute"/> 的补丁类按模块分组安装。
///
/// 分工：本类只负责「哪些类属于哪个模块」和「成组装卸」，<see cref="Entry"/> 负责把带标记的类
/// 从旧的全量安装里排除。没有标记的类（入口、宿主、联机、兼容桥）行为完全不变。
/// </summary>
internal static class FeatureBootstrap
{
    /// <summary>读类上的模块标记；id 不在 <see cref="FeatureIds.All"/> 里按拼错处理，返回 false。</summary>
    internal static bool TryGetFeatureId(Type type, out string featureId)
    {
        featureId = string.Empty;
        var attribute = type.GetCustomAttributes(typeof(FeaturePatchAttribute), inherit: false)
            .OfType<FeaturePatchAttribute>()
            .FirstOrDefault();
        if (attribute is null || !FeatureIds.All.Contains(attribute.FeatureId))
        {
            return false;
        }

        featureId = attribute.FeatureId;
        return true;
    }

    /// <summary>这些类是否已交给模块注册表——是的话旧的全量安装必须跳过它们。</summary>
    internal static bool IsManagedByFeature(Type type) => TryGetFeatureId(type, out _);

    internal static IReadOnlyList<FeatureInstallResult> InstallGroups(
        Harmony harmony,
        IEnumerable<Type> loadableTypes)
    {
        var registry = new FeatureRegistry(new HarmonyPatchInstaller(harmony));
        var groups = loadableTypes
            .Where(type => type.IsClass)
            .Select(type => (Type: type, HasFeature: TryGetFeatureId(type, out var id), FeatureId: id))
            .Where(pair => pair.HasFeature)
            .GroupBy(pair => pair.FeatureId, pair => pair.Type, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal);

        var results = new List<FeatureInstallResult>();
        foreach (var group in groups)
        {
            var patchClasses = group.ToArray();
            // 装了皮肤包但还没有任何模块标记之前，这里不会产生任何结果，旧路径照旧安装全部补丁。
            var result = registry.InstallGroup(group.Key, patchClasses);
            results.Add(result);
            if (result.Installed)
            {
                ModLog.Info($"[模块] {result.Summary}。");
            }
            else
            {
                ModLog.Error($"[模块] {result.Summary}（该模块本次未启用，其余模块不受影响）。");
            }
        }

        return results;
    }

    private sealed class HarmonyPatchInstaller : IPatchInstaller
    {
        private readonly Harmony _harmony;

        internal HarmonyPatchInstaller(Harmony harmony)
        {
            _harmony = harmony;
        }

        public void Install(Type patchClass) => _harmony.CreateClassProcessor(patchClass).Patch();

        public void Uninstall(Type patchClass) => _harmony.CreateClassProcessor(patchClass).Unpatch();
    }
}
