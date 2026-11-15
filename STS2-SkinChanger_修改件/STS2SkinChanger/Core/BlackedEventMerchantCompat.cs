using System;
using HarmonyLib;

namespace STS2SkinChanger.Core;

/// <summary>
/// 「暗黑事件」(STS2BlackedEvevt) 的商人补丁在 v0.113.1 上会炸：
/// 它在 <c>MerchantInventory.CreateForNormalMerchant</c> 的 postfix 里调用
/// <c>RelicFactory.RollRarity(Player)</c>，而 v0.113.1 已经删掉了这个方法，
/// 于是抛 <see cref="MissingMethodException"/>，整条 <c>MerchantRoom.EnterInternal</c> 任务中断，
/// 玩家看到的就是「进商店房间一直黑屏」。
///
/// 这里只做一件事：当游戏里确实没有 <c>RollRarity</c> 时，跳过该 mod 的 postfix，
/// 让商人房间能正常进入（代价是它的遗物数量改动不生效）。游戏还留有该 API 时什么都不做。
/// </summary>
internal static class BlackedEventMerchantCompat
{
    private const string PatchTypeName = "STS2BlackedEvevt.Code.MerchantRelicCountPatch";

    internal static void Install(Harmony harmony)
    {
        try
        {
            var factory = AccessTools.TypeByName("MegaCrit.Sts2.Core.Factories.RelicFactory");
            if (factory == null || AccessTools.Method(factory, "RollRarity") != null)
            {
                // 没有这个 mod 的宿主类型，或者游戏仍然提供 RollRarity —— 不需要兼容。
                return;
            }

            var patchType = AccessTools.TypeByName(PatchTypeName);
            var postfix = patchType == null ? null : AccessTools.Method(patchType, "Postfix");
            if (postfix == null)
            {
                ModLog.Warn("[兼容] 发现游戏已删除 RelicFactory.RollRarity，但没有找到「暗黑事件」的商人补丁，跳过兼容处理。");
                return;
            }

            harmony.Patch(postfix, prefix: new HarmonyMethod(typeof(BlackedEventMerchantCompat), nameof(SkipPostfix)));
            ModLog.Warn(
                "[兼容] 游戏已删除 RelicFactory.RollRarity，「暗黑事件」的商人遗物补丁会抛 MissingMethodException 导致进商店黑屏，" +
                "已跳过该补丁（它的遗物数量改动不生效，商人房间可正常进入）。");
        }
        catch (Exception exception)
        {
            ModLog.Warn("[兼容] 暗黑事件商人补丁兼容处理失败：" + exception.GetBaseException().Message);
        }
    }

    private static bool SkipPostfix() => false;
}
