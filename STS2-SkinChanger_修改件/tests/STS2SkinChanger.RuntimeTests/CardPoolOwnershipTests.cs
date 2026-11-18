using STS2SkinChanger.Core;

// #11：卡池未被识别（没有皮肤分类）的卡，保留原图，不参与其他卡图/框架角色卡面的顶替。
internal static class CardPoolOwnershipTests
{
    internal static void Run()
    {
        // 游戏自带卡：仍按原有规则让框架按角色皮肤覆盖（不受这条限制）。
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: true, hasKnownCardPool: false),
            "原版卡不能被这条规则挡住。");
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: true, hasKnownCardPool: true),
            "原版卡不能被这条规则挡住。");

        // 已被切换器识别卡池的 Mod 卡：仍可被选中的卡图皮肤接管。
        Require(!SkinService.KeepsIntrinsicModCardArt(isGameCard: false, hasKnownCardPool: true),
            "已识别卡池的 Mod 卡仍然要能被选中的卡图皮肤接管。");

        // 卡池未被识别的 Mod 卡（久留美）：保留自己的原图。
        Require(SkinService.KeepsIntrinsicModCardArt(isGameCard: false, hasKnownCardPool: false),
            "卡池未被识别的 Mod 卡必须保留原图，不能被别的卡图包或框架角色卡面顶替。");

        Console.WriteLine("Card pool ownership policy passed.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
