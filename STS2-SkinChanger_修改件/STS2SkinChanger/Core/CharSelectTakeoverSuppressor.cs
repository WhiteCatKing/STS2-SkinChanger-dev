using System.Reflection;
using HarmonyLib;

namespace STS2SkinChanger.Core;

/// <summary>
/// 一个"第三方在选人界面接管某个外观"的拦截目标。
///
/// 这类 Mod 的通用特征：它们给 <c>MegaCrit.Sts2.Core.Nodes.Animation.NSpineAutoPlayer._Ready</c>
/// 挂一个 Harmony Postfix，在角色选择界面的 SpineSprite 下面塞自己的显示层并每帧遮罩本体骨架，
/// 于是换掉的不只是它自己那个召唤物，连别的皮肤的背景图也会被它改写。
///
/// <paramref name="AssemblyName"/> 等字段全部是**可配置**的：换一个同类 Mod 时只加一条目标即可，
/// 不需要改代码。默认表里只有 OstyDefectImpostor，但机制是通用的。
/// </summary>
internal sealed record CharSelectTakeoverTarget(
    string AssemblyName,
    string PatchTypeName,
    string PatchMethodName,
    string? NotifyTypeName = null,
    string? NotifyMethodName = null,
    int NotifyArgumentCount = 0);

/// <summary>
/// 通用拦截器：禁用第三方 Mod 在**角色选择界面**的接管，并可选地把它的"切换外观"调用转告本 Mod。
///
/// 设计原则（换成别的同类 Mod 也成立）：
///   1. 只动"选人界面"的那一个 Postfix，战斗/休息/商店里的行为一概不碰；
///   2. 全部走反射 + 按名字查找，目标不存在时静默跳过，不影响本 Mod 启动；
///   3. 只禁用"界面接管"这一个入口，不修改对方的事件、配置与存档；
///   4. 通知回调默认带"回声抑制"，本 Mod 自己调用对方时不回灌。
/// </summary>
internal static class CharSelectTakeoverSuppressor
{
    /// <summary>默认目标：OstyDefectImpostor（把骨妹选人背景里的奥斯提换成自己的）。</summary>
    private static readonly CharSelectTakeoverTarget[] DefaultTargets =
    [
        new(
            AssemblyName: "OstyDefectImpostor",
            PatchTypeName: "OstyDefectImpostor.CharacterSelectVisualPatch",
            PatchMethodName: "Postfix",
            NotifyTypeName: "OstyDefectImpostor.OstySkinSelection",
            NotifyMethodName: "Apply",
            NotifyArgumentCount: 1)
    ];

    private static bool _installed;
    private static Action<string>? _onNotified;
    private static bool _skipLogged;

    /// <summary>本 Mod 自己触发对方切换时置位，避免把自己的调用当成"玩家在对方界面里改了"。</summary>
    internal static bool EchoSuppressed { get; set; }

    /// <summary>
    /// 安装拦截。<paramref name="onNotified"/> 收到对方切换后的标识（例如皮肤 id）。
    /// 可以重复调用；也可以传入自定义目标表来适配别的同类 Mod。
    /// </summary>
    internal static void Install(Action<string>? onNotified = null,
        IEnumerable<CharSelectTakeoverTarget>? targets = null)
    {
        _onNotified = onNotified;
        if (_installed)
        {
            return;
        }

        var plan = (targets ?? DefaultTargets).ToArray();
        if (plan.Length == 0)
        {
            return;
        }

        try
        {
            var harmony = new Harmony(Entry.ModId + ".char-select-takeover-guard");
            var patched = 0;
            foreach (var target in plan)
            {
                var assembly = FindAssembly(target.AssemblyName);
                if (assembly == null)
                {
                    continue;
                }

                var patchType = assembly.GetType(target.PatchTypeName, throwOnError: false);
                var postfix = patchType?.GetMethod(
                    target.PatchMethodName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (postfix != null)
                {
                    harmony.Patch(postfix, prefix: new HarmonyMethod(
                        typeof(CharSelectTakeoverSuppressor), nameof(SkipTakeoverPrefix)));
                    patched++;
                }

                if (target.NotifyTypeName == null || target.NotifyMethodName == null)
                {
                    continue;
                }

                var notifyType = assembly.GetType(target.NotifyTypeName, throwOnError: false);
                var notify = notifyType?.GetMethod(
                    target.NotifyMethodName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: Enumerable.Repeat(typeof(string), target.NotifyArgumentCount).ToArray(),
                    modifiers: null);
                if (notify != null)
                {
                    harmony.Patch(notify, postfix: new HarmonyMethod(
                        typeof(CharSelectTakeoverSuppressor), nameof(NotifyPostfix)));
                    patched++;
                }
            }

            _installed = patched > 0;
            ModLog.Info(patched > 0
                ? $"[兼容] 已拦截 {plan.Length} 个第三方选人界面接管入口，并绑定其切换回调（{patched} 处补丁）。"
                : "[兼容] 未找到可拦截的第三方选人界面接管入口。");
        }
        catch (Exception exception)
        {
            ModLog.Warn("[兼容] 安装选人界面拦截失败：" + exception.GetBaseException().Message);
        }
    }

    private static System.Reflection.Assembly? FindAssembly(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(candidate =>
            string.Equals(candidate.GetName()?.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>短路对方的选人界面 Postfix：不再往选人背景里塞显示层。</summary>
    private static bool SkipTakeoverPrefix()
    {
        if (!_skipLogged)
        {
            _skipLogged = true;
            ModLog.Info("[兼容] 已禁用第三方 Mod 在选人界面的接管；选人界面的外观由本 Mod 负责。");
        }

        return false;
    }

    /// <summary>对方切换外观后转告本 Mod（本 Mod 自己调用时会被回声抑制挡住）。</summary>
    private static void NotifyPostfix(string __0)
    {
        if (EchoSuppressed)
        {
            return;
        }

        try
        {
            _onNotified?.Invoke(__0);
        }
        catch (Exception exception)
        {
            ModLog.Warn("[兼容] 同步第三方切换结果失败：" + exception.GetBaseException().Message);
        }
    }
}
