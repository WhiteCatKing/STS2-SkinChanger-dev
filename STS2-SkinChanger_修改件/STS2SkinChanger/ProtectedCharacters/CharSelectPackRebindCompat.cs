// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
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

    internal static void AfterRebuild(Control? screen, string? groupId, string? providerId)
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
        var now = System.Environment.TickCount64;
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
                if (method == null || !method.GetParameters()[0].ParameterType.IsInstanceOfType(screen))
                {
                    continue;
                }

                try
                {
                    method.Invoke(null, [screen]);

                    if (groupId != null)
                    {
                        ModLog.Info($"[选角重挂] 已请求 {assemblyName} 重新量取选人背景（分组={groupId}）。");
                        DescribeClickCatchers(screen, groupId);
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

    /// <summary>
    /// 诊断用：把它们挂的点击接收器（<c>DBWCharSelectClickCatcher</c> 一类）的尺寸、可见性、
    /// 以及所在背景节点的可见性打出来，用来判断「点击没反应」到底是没收到还是收到了被拒。
    /// </summary>
    private static void DescribeClickCatchers(Node screen, string groupId)
    {
        try
        {
            var found = new List<string>();
            CollectClickCatchers(screen, found, 0, "screen");
            if (found.Count == 0)
            {
                ModLog.Info($"[选角点击] {groupId}: 没找到任何点击接收器节点。");
                return;
            }

            foreach (var line in found)
            {
                ModLog.Info($"[选角点击] {groupId}: {line}");
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[选角点击] {groupId}: 采集失败：{exception.GetBaseException().Message}");
        }
    }

    private static void CollectClickCatchers(Node node, List<string> found, int depth, string path)
    {
        if (depth > 32)
        {
            return;
        }

        if (node.Name.ToString().Contains("ClickCatcher", StringComparison.Ordinal))
        {
            if (node is Control control)
            {
                found.Add(
                    $"节点={path}/{node.Name} 尺寸={control.Size} 位置={control.Position} " +
                    $"可见={control.Visible} 在树可见={control.IsVisibleInTree()} " +
                    $"过滤={control.MouseFilter} 父可见={(control.GetParent() as CanvasItem)?.Visible}");
            }
        }

        foreach (var child in node.GetChildren())
        {
            CollectClickCatchers(child, found, depth + 1, $"{path}/{node.Name}");
        }
    }
}
