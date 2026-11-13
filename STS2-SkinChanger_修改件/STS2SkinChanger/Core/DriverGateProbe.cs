using System;
using System.Collections.Generic;
using System.Reflection;

namespace STS2SkinChanger.Core;

/// <summary>
/// 临时诊断：血量换装那一族皮肤包自己带三道闸门，任何一道为 false 整个包就静默
/// （连战斗换装一起关，且它们一行日志都不打）。第一道会反射调用我们的
/// <see cref="SkinService.GetSelectedRuntimeProvider"/>，这里把它们的判定结果打出来，
/// 用来确认「选人界面点击切战损没反应」到底卡在哪一道。诊断结束后整文件删除。
/// </summary>
internal static class DriverGateProbe
{
    private static readonly HashSet<string> Reported = new(StringComparer.Ordinal);

    internal static void Report(string? groupId)
    {
        if (groupId == null || !Reported.Add(groupId))
        {
            return;
        }

        try
        {
            var found = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.GetName().Name ?? string.Empty;
                if (!assemblyName.Contains("HpWear", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var gate = assembly.GetType("BattleWear.SkinSelectionGate", throwOnError: false);
                if (gate == null)
                {
                    continue;
                }

                var menuAllowed = gate.GetMethod("MenuAllowed", Static);
                var activation = assembly.GetType("BattleWear.WearActivation", throwOnError: false);
                var peek = activation?.GetMethod("PeekActiveDriver", Static);
                found.Add(
                    $"{assemblyName}: MenuAllowed={menuAllowed?.Invoke(null, null) ?? "<无此方法>"}, " +
                    $"PeekActiveDriver={peek?.Invoke(null, null) ?? "<无此方法>"}");
            }

            ModLog.Info(found.Count == 0
                ? $"[选角闸门] {groupId} 没有加载任何 HpWear 皮肤包"
                : $"[选角闸门] {groupId} " + string.Join(" | ", found));
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[选角闸门] 探测失败：{exception.GetBaseException().Message}");
        }
    }
}
