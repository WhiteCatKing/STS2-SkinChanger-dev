using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 同一线程上的重入深度守卫（需求 1、2）：挡住 A → B → A 的同步互相触发
/// （例如重建身体触发 _Ready 补丁，补丁又请求重建）。超过深度就拒绝进入并节流记一行。
/// <code>
/// using var scope = LoopGuard.Enter("rebuild-creature");
/// if (!scope.Entered) return;
/// </code>
/// 跨帧的循环（每帧 tick 反复触发）用 <see cref="CircuitBreaker"/> 的 MaxCalls 挡，不归这里管。
/// </summary>
internal static class LoopGuard
{
    [ThreadStatic]
    private static Dictionary<string, int>? _depths;

    public const int DefaultMaxDepth = 3;

    public static Scope Enter(string key, int maxDepth = DefaultMaxDepth)
    {
        var depths = _depths ??= new Dictionary<string, int>(StringComparer.Ordinal);
        depths.TryGetValue(key, out var depth);
        if (depth >= maxDepth)
        {
            SafetyLog.Throttled(
                "loop:" + key,
                $"[防循环] 「{key}」重入已达 {depth} 层（上限 {maxDepth}），本次跳过。");
            return default;
        }

        depths[key] = depth + 1;
        return new Scope(key);
    }

    public static int Depth(string key) =>
        _depths != null && _depths.TryGetValue(key, out var depth) ? depth : 0;

    public readonly struct Scope : IDisposable
    {
        private readonly string? _key;

        internal Scope(string key) => _key = key;

        public bool Entered => _key != null;

        public void Dispose()
        {
            if (_key == null || _depths == null || !_depths.TryGetValue(_key, out var depth))
            {
                return;
            }

            if (depth <= 1)
            {
                _depths.Remove(_key);
            }
            else
            {
                _depths[_key] = depth - 1;
            }
        }
    }
}
