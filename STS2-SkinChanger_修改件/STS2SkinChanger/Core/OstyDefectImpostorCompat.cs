using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

/// <summary>
/// Compatibility bridge for the "让鸡煲撑在地上假装自己是奥斯提" Workshop mod (id
/// <c>OstyDefectImpostor</c>, workshop 3820801911).
///
/// That mod installs all of its hooks through one <c>Harmony.PatchAll()</c> and, as soon as any
/// single patch target fails to resolve, its catch block calls <c>UnpatchAll()</c>. One renamed
/// game method therefore silences the whole mod ("Patch unavailable; original visuals
/// retained"). v0.113.0 renamed <c>NCreature.OstyScaleToSize(float)</c> to
/// <c>ScaleAndPositionOsty(float ostyHealth, double duration)</c>; the mod still asks Harmony for
/// the old name through <c>[HarmonyPatch(typeof(NCreature), "OstyScaleToSize")]</c>.
///
/// Harmony resolves attribute patches through <c>AccessTools.Method(type, name, parameters,
/// generics)</c>, so a postfix on that one overload can hand back the renamed method when a mod
/// asks for a member that the game moved. The redirect only fires for the exact type/name pair
/// below, and only while the lookup would otherwise return null, so it cannot change how any
/// other mod or the game itself resolves a method.
/// </summary>
internal static class OstyDefectImpostorCompat
{
    private const string LegacyMethodName = "OstyScaleToSize";
    private const string RenamedMethodName = "ScaleAndPositionOsty";

    private static MethodInfo? _renamedMethod;
    private static bool _resolving;
    private static bool _reported;

    /// <summary>
    /// Install the lookup redirect. Called while the mod's own patch classes are being installed,
    /// which happens before any other mod's initializer runs, so a mod that patches during
    /// initialization already sees the renamed method.
    /// </summary>
    internal static void Install(Harmony harmony)
    {
        _renamedMethod = AccessTools.Method(typeof(NCreature), RenamedMethodName);
        if (_renamedMethod == null)
        {
            ModLog.Warn($"当前游戏版本找不到 NCreature.{RenamedMethodName}，奥斯提皮肤兼容层未生效。");
            return;
        }

        // Resolve the overload by exact signature: AccessTools has two methods called "Method", so
        // looking it up through AccessTools itself hits an ambiguous match and returns null.
        var lookup = typeof(AccessTools).GetMethod(
            nameof(AccessTools.Method),
            BindingFlags.Public | BindingFlags.Static,
            null,
            [typeof(Type), typeof(string), typeof(Type[]), typeof(Type[])],
            null);
        if (lookup == null)
        {
            ModLog.Warn(
                "奥斯提皮肤兼容层无法定位 AccessTools.Method(Type, string, Type[], Type[])，" +
                "按旧名打补丁的皮肤包仍会被游戏自己停用。");
            return;
        }

        harmony.Patch(
            lookup,
            postfix: new HarmonyMethod(
                typeof(OstyDefectImpostorCompat),
                nameof(RedirectRenamedMember)));
    }

    /// <summary>
    /// Postfix for <c>AccessTools.Method(Type, string, Type[], Type[])</c>: when a mod asks for a
    /// member the game renamed, return the current member instead of null.
    /// </summary>
    private static void RedirectRenamedMember(Type type, string name, ref MethodInfo? __result)
    {
        if (__result != null || _resolving || _renamedMethod == null)
        {
            return;
        }

        if (type != typeof(NCreature)
            || !string.Equals(name, LegacyMethodName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            _resolving = true;
            __result = _renamedMethod;
        }
        finally
        {
            _resolving = false;
        }

        if (!_reported)
        {
            _reported = true;
            ModLog.Info(
                $"已把第三方模组请求的 NCreature.{LegacyMethodName} 重定向到 v0.113.0 的 " +
                $"{RenamedMethodName}，让仍按旧名打补丁的奥斯提皮肤包继续工作。");
        }
    }
}
