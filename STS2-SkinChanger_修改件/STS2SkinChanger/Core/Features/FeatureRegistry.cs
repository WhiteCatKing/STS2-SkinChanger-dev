using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core.Features;

/// <summary>
/// 补丁装卸的最小接口。游戏里由 Harmony 实现；逻辑测试里由假实现记录调用顺序，
/// 这样「一个模块装失败只回滚它自己」这条规则可以脱离游戏验证。
/// </summary>
internal interface IPatchInstaller
{
    void Install(Type patchClass);

    void Uninstall(Type patchClass);
}

/// <summary>一个模块本次的安装结果。</summary>
internal sealed class FeatureInstallResult
{
    internal FeatureInstallResult(string featureId, int patchClassCount, bool installed, string? error)
    {
        FeatureId = featureId;
        PatchClassCount = patchClassCount;
        Installed = installed;
        Error = error;
    }

    internal string FeatureId { get; }

    internal int PatchClassCount { get; }

    internal bool Installed { get; }

    /// <summary>失败原因；成功时为 null。</summary>
    internal string? Error { get; }

    internal string Summary => Installed
        ? $"「{FeatureId}」{PatchClassCount} 个补丁类已装"
        : $"「{FeatureId}」{PatchClassCount} 个补丁类安装失败并整体回滚：{Error}";
}

/// <summary>
/// 按模块分组安装补丁。规则只有一条：**模块之间互不影响**——某个模块的任意一个补丁类抛异常，
/// 只把这个模块已经装上的那几个撤回去（保持「要么整套生效、要么整套不生效」），
/// 其余模块与常驻补丁照常安装。
/// </summary>
internal sealed class FeatureRegistry
{
    private readonly IPatchInstaller _installer;
    private readonly Dictionary<string, FeatureInstallResult> _results =
        new(StringComparer.Ordinal);

    internal FeatureRegistry(IPatchInstaller installer)
    {
        _installer = installer ?? throw new ArgumentNullException(nameof(installer));
    }

    internal IReadOnlyDictionary<string, FeatureInstallResult> Results => _results;

    internal bool IsInstalled(string featureId) =>
        _results.TryGetValue(featureId, out var result) && result.Installed;

    /// <summary>安装一个模块的全部补丁类；失败时回滚本模块已装的部分并返回失败结果。</summary>
    internal FeatureInstallResult InstallGroup(string featureId, IReadOnlyList<Type> patchClasses)
    {
        if (string.IsNullOrEmpty(featureId))
        {
            throw new ArgumentException("模块 id 不能为空。", nameof(featureId));
        }

        if (_results.ContainsKey(featureId))
        {
            throw new InvalidOperationException($"模块「{featureId}」已经安装过，不能重复安装。");
        }

        var installed = new List<Type>(patchClasses.Count);
        try
        {
            foreach (var patchClass in patchClasses)
            {
                _installer.Install(patchClass);
                installed.Add(patchClass);
            }

            var success = new FeatureInstallResult(featureId, patchClasses.Count, true, null);
            _results[featureId] = success;
            return success;
        }
        catch (Exception exception)
        {
            var baseException = exception.GetBaseException();
            foreach (var patchClass in installed)
            {
                try
                {
                    _installer.Uninstall(patchClass);
                }
                catch
                {
                    // 回滚单个类失败不影响其它模块，也不改变「本模块未启用」的结论。
                }
            }

            var failure = new FeatureInstallResult(
                featureId,
                patchClasses.Count,
                false,
                baseException.GetType().Name + " / " + baseException.Message);
            _results[featureId] = failure;
            return failure;
        }
    }
}
