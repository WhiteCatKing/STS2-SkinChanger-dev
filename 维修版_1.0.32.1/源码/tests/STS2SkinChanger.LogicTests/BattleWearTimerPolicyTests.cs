using STS2SkinChanger.Core;

internal static class BattleWearTimerPolicyTests
{
    internal static void Run()
    {
        const string run = "MegaCrit.Sts2.Core.Nodes.NRun";
        const string timerPatch = "BattleWear.BattleWearTimerPatch";
        Require(BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Postfix", true, [run]),
            "已验证的对局视觉 Timer 回调应允许补初始化。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", "OtherMod.RunReadyPatch", "Postfix", true, [run]),
            "其他模组的 NRun Ready 回调不能被重放。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            "MegaCrit.Sts2.Core.Nodes.Combat.NCreature", "_Ready", timerPatch, "Postfix", true, [run]),
            "只能补 NRun 的初始化。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_ExitTree", timerPatch, "Postfix", true, [run]),
            "退出回调不能被当成初始化。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Prefix", true, [run]),
            "只允许已验证的 Postfix 方法。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Postfix", false, [run]),
            "实例方法或非 void 回调不符合已验证签名。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Postfix", true, []),
            "缺少 NRun 实例参数时不能重放。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Postfix", true, [run, "System.Boolean"]),
            "增加其他参数的未知版本不能重放。");
        Require(!BattleWearTimerPolicy.IsVerifiedCallback(
            run, "_Ready", timerPatch, "Postfix", true, ["System.Object"]),
            "参数类型必须与已验证回调一致。");
        Console.WriteLine("Battle-wear timer policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
