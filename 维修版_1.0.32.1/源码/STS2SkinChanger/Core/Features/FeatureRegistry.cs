using System;
using System.Collections.Generic;
using System.Linq;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Core.Features;

/// <summary>装卸一组补丁类的抽象（实际实现走 Harmony；测试里用假的）。</summary>
internal interface IPatchInstaller
{
    void Install(Type patchClass);
    void Uninstall(Type patchClass);
}

/// <summary>
/// 功能模块注册表（阶段 C 骨架）。只管"模块的补丁现在该不该在位"：
/// <list type="bullet">
/// <item>启动时所有模块按"已激活"处理（和过去全装一致，不在目录就绪前冒险卸补丁）。</item>
/// <item><see cref="Reevaluate"/>：需要 → 装；不需要且不在用 → 卸；不需要但还在用 → 先不动，下次再看。</item>
/// <item>模块的熔断 key（<c>patch:&lt;id&gt;</c>）跳闸 → 本会话标记停用，不在用时卸掉；在用时等它结束。
/// 卸的时候照样调 <see cref="ISkinFeature.OnDeactivating"/> 做清理（清理不走已跳闸的 Safe）。</item>
/// </list>
/// 纯逻辑，不依赖 Godot / Harmony，便于离线测试。所有方法应在主线程调用。
/// </summary>
internal sealed class FeatureRegistry
{
    private sealed class Entry
    {
        public required ISkinFeature Feature;
        public required Type[] PatchClasses;
        public bool Installed;
        public bool DisabledBySafety;
        public long Generation;
    }

    private readonly IPatchInstaller _installer;
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.Ordinal);

    public FeatureRegistry(IPatchInstaller installer)
    {
        _installer = installer;
        CircuitBreaker.Tripped += OnBreakerTripped;
    }

    public static FeatureRegistry? Current { get; set; }

    public IReadOnlyCollection<string> FeatureIds => _entries.Keys;

    public static string BreakerKey(string featureId) => "patch:" + featureId;

    /// <summary>注册并立即装上补丁（启动期默认在位，等目录就绪后由 <see cref="Reevaluate"/> 决定去留）。</summary>
    public void Register(ISkinFeature feature, IEnumerable<Type> patchClasses)
    {
        if (_entries.ContainsKey(feature.Id))
        {
            throw new InvalidOperationException("重复注册功能模块：" + feature.Id);
        }

        var entry = new Entry { Feature = feature, PatchClasses = patchClasses.Distinct().ToArray() };
        _entries[feature.Id] = entry;
        Install(entry, notify: false);
    }

    public bool IsInstalled(string featureId) => _entries.TryGetValue(featureId, out var e) && e.Installed;

    public long Generation(string featureId) => _entries.TryGetValue(featureId, out var e) ? e.Generation : 0;

    /// <summary>延迟回调执行前调用：排队时记下的代次已经过期（模块停用过或重新激活过）就返回 false。</summary>
    public bool IsCurrent(string featureId, long generation) =>
        _entries.TryGetValue(featureId, out var e) && e.Installed && e.Generation == generation;

    /// <summary>按当前配置 / 目录 / 在用情况调整每个模块的补丁。返回这一轮装和卸的模块 id。</summary>
    public (IReadOnlyList<string> Activated, IReadOnlyList<string> Deactivated) Reevaluate()
    {
        var activated = new List<string>();
        var deactivated = new List<string>();
        foreach (var entry in _entries.Values)
        {
            var want = !entry.DisabledBySafety && SafeShould(entry) != false;
            if (want && !entry.Installed)
            {
                Install(entry, notify: true);
                if (entry.Installed) activated.Add(entry.Feature.Id);
            }
            else if (!want && entry.Installed && !SafeInUse(entry))
            {
                Uninstall(entry);
                deactivated.Add(entry.Feature.Id);
            }
        }

        if (activated.Count > 0 || deactivated.Count > 0)
        {
            SafetyLog.Always(
                $"[模块] 按需调整：启用 {(activated.Count == 0 ? "无" : string.Join("、", activated))}；" +
                $"停用 {(deactivated.Count == 0 ? "无" : string.Join("、", deactivated))}。");
        }

        return (activated, deactivated);
    }

    internal void DetachForTests() => CircuitBreaker.Tripped -= OnBreakerTripped;

    private void OnBreakerTripped(string key, string reason)
    {
        foreach (var entry in _entries.Values)
        {
            if (BreakerKey(entry.Feature.Id) == key)
            {
                entry.DisabledBySafety = true;
                SafetyLog.Always($"[模块] 「{entry.Feature.Id}」因熔断本会话停用；场上没有在用对象时摘掉它的补丁。");
                if (entry.Installed && !SafeInUse(entry))
                {
                    Uninstall(entry);
                }
            }
        }
    }

    private void Install(Entry entry, bool notify)
    {
        var done = new List<Type>();
        try
        {
            foreach (var type in entry.PatchClasses)
            {
                _installer.Install(type);
                done.Add(type);
            }
        }
        catch (Exception exception)
        {
            // 装一半失败：把已经装上的撤掉，模块整体保持"未激活"，不留半套补丁。
            foreach (var type in done)
            {
                try { _installer.Uninstall(type); } catch { }
            }

            SafetyLog.Always($"[模块] 「{entry.Feature.Id}」补丁安装失败，保持未启用：{exception.GetBaseException().Message}");
            return;
        }

        entry.Installed = true;
        entry.Generation++;
        if (notify)
        {
            try
            {
                entry.Feature.OnActivated();
            }
            catch (Exception exception)
            {
                Safe.Report(BreakerKey(entry.Feature.Id), exception);
            }
        }
    }

    private void Uninstall(Entry entry)
    {
        // 先推进代次，让排队中的旧回调在执行点自行退出；再清理；最后摘补丁。
        entry.Generation++;
        try
        {
            entry.Feature.OnDeactivating();
        }
        catch (Exception exception)
        {
            SafetyLog.Always($"[模块] 「{entry.Feature.Id}」停用清理出错（继续摘补丁）：{exception.GetBaseException().Message}");
        }

        foreach (var type in entry.PatchClasses)
        {
            try
            {
                _installer.Uninstall(type);
            }
            catch (Exception exception)
            {
                SafetyLog.Always($"[模块] 「{entry.Feature.Id}」摘补丁 {type.Name} 失败：{exception.GetBaseException().Message}");
            }
        }

        entry.Installed = false;
    }

    private static bool? SafeShould(Entry entry)
    {
        try
        {
            return entry.Feature.ShouldBeActive();
        }
        catch
        {
            return null; // 判断不出来就当"还不知道"，保持现状
        }
    }

    private static bool SafeInUse(Entry entry)
    {
        try
        {
            return entry.Feature.IsInUse();
        }
        catch
        {
            return true; // 判断不出来就当"在用"，宁可晚卸
        }
    }
}
