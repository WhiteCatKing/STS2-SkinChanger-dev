using System;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

// The EmperorCrabSkinFix project is authored and tested by 白猫 as a standalone Mod. It is
// bundled here verbatim (only the namespace changes) so SkinChanger does not depend on a second
// Workshop item for the kaiser crab's combat/bestiary/half-HP behaviour. Every Install method is
// internally guarded and tolerates a missing optional dependency.
internal static class EmperorCrabCompat
{
    private static bool _installed;

    internal static void Install(Harmony harmony)
    {
        if (_installed)
        {
            return;
        }

        _installed = true;
        try
        {
            KaiserCrabSkinRefreshPatch.EnsureInstalled(harmony);
        }
        catch (Exception exception)
        {
            GD.PrintErr("[SkinChanger] 安装帝皇蟹图鉴刷新补丁失败：" + exception);
        }

        try
        {
            CombatCrabSkinPatch.Install(harmony);
        }
        catch (Exception exception)
        {
            GD.PrintErr("[SkinChanger] 安装帝皇蟹局内背景补丁失败：" + exception);
        }

        try
        {
            HalfHpCrabSkinPatch.Install(harmony);
        }
        catch (Exception exception)
        {
            GD.PrintErr("[SkinChanger] 安装帝皇蟹半血爆衣补丁失败：" + exception);
        }

        try
        {
            RitsuLibCardGuardPatch.Install(harmony);
        }
        catch (Exception exception)
        {
            GD.PrintErr("[SkinChanger] 安装 RitsuLib 卡牌异步保护失败：" + exception);
        }
    }
}
