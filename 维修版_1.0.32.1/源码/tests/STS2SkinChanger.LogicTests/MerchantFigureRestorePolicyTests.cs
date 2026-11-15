using STS2SkinChanger.Core;

internal static class MerchantFigureRestorePolicyTests
{
    // Merchant skins apply their figure's transform from the native button's _Ready prefix. Skin
    // Changer replays that prefix instead of keeping it in Harmony, so the transform lives as a
    // tracked mutation. Deactivating the provider while its merchant is still on screen used to hand
    // the vanilla transform back, which draws the skin's own skeleton at roughly 2.6x the intended
    // scale, far up and to the left ("the merchant grew and jumped into the top-left corner").
    internal static void Run()
    {
        Require(
            MerchantFigureRestorePolicy.KeepsTransform(
                "Merchant2CuteII",
                "Merchant2CuteII",
                providerStillSelected: true),
            "本次停用只是因为对局可见范围不含商店、而商人分组仍然选中该皮肤时，必须保它已经应用的骨骼变换；" +
            "否则换回原版变换后画面上的娘化骨骼会突然变大并跳到左上角。");

        Require(
            !MerchantFigureRestorePolicy.KeepsTransform(
                "Merchant2CuteII",
                "Merchant2CuteII",
                providerStillSelected: false),
            "商人分组已经换成别的皮肤或游戏原版时，必须照常还原原版骨骼变换。");

        Require(
            !MerchantFigureRestorePolicy.KeepsTransform(
                "Merchant2CuteII",
                "OtherMerchantPack",
                providerStillSelected: true),
            "被停用的是另一个提供者时，不能替它保住别的皮肤写下的变换。");

        Require(
            !MerchantFigureRestorePolicy.KeepsTransform(null, "Merchant2CuteII", true) &&
            !MerchantFigureRestorePolicy.KeepsTransform("Merchant2CuteII", "", true) &&
            !MerchantFigureRestorePolicy.KeepsTransform("", "", true),
            "没有记录归属的节点或没有提供者时一律按原样还原，不能凭猜测保留画面上任何变换。");

        Console.WriteLine(
            "Merchant figure restore policy passed: a still-selected skin keeps its own figure " +
            "transform through a scope-only deactivation, and every other case still restores.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
