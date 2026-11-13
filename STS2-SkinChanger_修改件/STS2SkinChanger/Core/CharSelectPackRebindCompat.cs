using System;
using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Core;

/// <summary>
/// 血量换装那一族皮肤包（<c>necrobinderHpWear</c> 这类）在选人界面给背景挂「点击切战损」时，
/// 会先量当前背景 SpineSprite 的图集页尺寸，只有等于自己预设尺寸才接管；
/// 它们的量测发生在包初始化那一刻，而我们的皮肤资源包要等用户选中后才挂载，
/// 于是它们量到的是原版图集、直接判定「没装对应皮肤」并永久放弃。
/// 这里在每次重建选角展示之后，把它们自己的重挂入口再叫一次，让它重新量一次。
/// 包内部仍会用自己的 <c>SkinSelectionGate.MenuAllowed()</c> 决定要不要接管，
/// 所以只对真正被选中的那一个包生效，不会串皮。
/// </summary>
internal static class CharSelectPackRebindCompat
{
    private const string RebindMethod = "TryReattachFromScreen";
    private const int ThrottleMilliseconds = 700;

    private static readonly BindingFlags StaticFlags =
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly Dictionary<string, long> LastCall = new(StringComparer.Ordinal);

    internal static void AfterRebuild(NCharacterSelectScreen? screen, string? groupId, string? providerId)
    {
        if (screen == null || string.IsNullOrEmpty(providerId))
        {
            return;
        }

        if (!IsWearDriverProvider(providerId))
        {
            return;
        }

        var throttleKey = providerId!;
        var now = Environment.TickCount64;
        if (LastCall.TryGetValue(throttleKey, out var previous) && now - previous < ThrottleMilliseconds)
        {
            return;
        }

        LastCall[throttleKey] = now;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var assemblyName = assembly.GetName().Name ?? string.Empty;
            if (!string.Equals(assemblyName, throttleKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var type in types)
            {
                var method = type.GetMethod(
                    RebindMethod,
                    StaticFlags,
                    null,
                    [typeof(NCharacterSelectScreen)],
                    null);
                if (method == null)
                {
                    continue;
                }

                try
                {
                    method.Invoke(null, [screen]);

                    if (groupId != null)
                    {
                        ModLog.Info($"[选角重挂] 已请求 {assemblyName} 重新量取选人背景（分组={groupId}）。");
                    }
                }
                catch (Exception exception)
                {
                    ModLog.Warn($"[选角重挂] {assemblyName} 重挂失败：{exception.GetBaseException().Message}");
                }
            }
        }
    }

    /// <summary>
    /// 只处理「皮肤包自己会做点击切换」的这一族：它们的程序集名以 <c>HpWear</c> 结尾，
    /// 且我们的选择里存的就是这个程序集名。
    /// </summary>
    private static bool IsWearDriverProvider(string providerId)
    {
        return providerId.EndsWith("HpWear", StringComparison.OrdinalIgnoreCase);
    }
}
