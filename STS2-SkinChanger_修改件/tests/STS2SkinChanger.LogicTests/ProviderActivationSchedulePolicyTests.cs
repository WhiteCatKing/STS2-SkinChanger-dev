using STS2SkinChanger.Core;

internal static class ProviderActivationSchedulePolicyTests
{
    // 启动阶段（还没有任何场景范围）把 22 个提供者的 DLL 全部初始化要 3.9 s，而那一刻屏幕上
    // 只有主菜单：怪物、先古、事件、卡牌的提供者要等玩家真的进入对应场景才有用。这里锁定拆分
    // 规则本身——真正需要的那一批（角色/随从外观）必须同步，其余才允许延后；一旦有了场景范围
    // 就不许再拆，否则可见的提供者可能缺席自己场景的第一次实例化。
    internal static void Run()
    {
        var firstScreen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OrchisNecrobinderSkinMod",
            "ironcladHpWear"
        };
        Func<string, bool> isFirstScreen = providerId => firstScreen.Contains(providerId);
        string[] providers =
        [
            "OrchisNecrobinderSkinMod",
            "1987的某一天 nsfw lgg",
            "ironcladHpWear",
            "AncientWaifus_Beta"
        ];

        var startup = ProviderActivationSchedulePolicy.Plan(
            providers,
            isFirstScreen,
            deferNonFirstScreen: true);
        Require(
            startup.Immediate.SequenceEqual(["OrchisNecrobinderSkinMod", "ironcladHpWear"]),
            "启动阶段必须同步初始化角色/随从外观的提供者（选人界面紧跟着就要画出来），" +
            "并且保持传入顺序。");
        Require(
            startup.Deferred.SequenceEqual(["1987的某一天 nsfw lgg", "AncientWaifus_Beta"]),
            "怪物、先古这类只在战斗/事件里用得到的提供者必须延后，别在启动时白白初始化。");
        Require(startup.HasDeferred, "有延后项时必须能被调用方看见，好安排后台队列。");

        var inScene = ProviderActivationSchedulePolicy.Plan(
            providers,
            isFirstScreen,
            deferNonFirstScreen: false);
        Require(
            inScene.Immediate.Count == providers.Length && !inScene.HasDeferred,
            "已经有了场景范围时不允许拆分：这次要激活的提供者全都同步就位。");

        var duplicated = ProviderActivationSchedulePolicy.Plan(
            ["IroncladHpWear", "", "  ", "ironcladhpwear", "AncientWaifus_Beta"],
            isFirstScreen,
            deferNonFirstScreen: true);
        Require(
            duplicated.Immediate.Count == 1 && duplicated.Immediate[0].Equals("IroncladHpWear"),
            "同一个提供者被多个分组选中时只同步初始化一次（忽略大小写）。");
        Require(
            duplicated.Deferred.SequenceEqual(["AncientWaifus_Beta"]),
            "空白项与重复项不能进队列，否则后台会重复跑同一个初始化器。");

        var merchant = ProviderActivationSchedulePolicy.Plan(
            ["ATA_Merchant", "AncientWaifus_Beta"], _ => false, true, "ata_merchant");
        Require(merchant.Immediate.SequenceEqual(["ATA_Merchant"]) &&
                merchant.Deferred.SequenceEqual(["AncientWaifus_Beta"]),
            "所选商人必须在建节点之前同步初始化，其他后台任务仍延后。");

        var nothing = ProviderActivationSchedulePolicy.Plan(
            [],
            isFirstScreen,
            deferNonFirstScreen: true);
        Require(
            nothing.Immediate.Count == 0 && nothing.Deferred.Count == 0 && !nothing.HasDeferred,
            "没有任何提供者时计划必须为空，不能凭空排一个空队列。");

        Console.WriteLine(
            "Provider activation schedule policy passed: character/companion providers initialize " +
            "with the startup rebuild, the rest wait for the menu, and a real scene scope never " +
            "defers a visible provider.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
