using STS2SkinChanger.Core;

internal static class BackgroundSkinPolicyTests
{
    public static void Run()
    {
        static void Require(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }

        var encoded = BackgroundSkinPolicy.ProviderOptionId("CznEnemySkin", "b_laino_outdoor2_2");
        Require(
            BackgroundSkinPolicy.TryParseProviderOptionId(encoded, out var provider, out var battlefield) &&
            provider == "CznEnemySkin" &&
            battlefield == "b_laino_outdoor2_2",
            "背景选项 ID 必须能无损还原提供者与战场，重启后仍能定位同一背景。");

        Require(
            !BackgroundSkinPolicy.TryParseProviderOptionId(
                BackgroundSkinPolicy.FollowCategoryId,
                out _,
                out _) &&
            !BackgroundSkinPolicy.TryParseProviderOptionId("__base__", out _, out _),
            "“跟随分类”和“游戏默认”不能被解析成提供者背景。");

        Require(
            BackgroundSkinPolicy.IsFollowCategory(null) &&
            BackgroundSkinPolicy.IsFollowCategory(string.Empty) &&
            BackgroundSkinPolicy.IsFollowCategory(BackgroundSkinPolicy.FollowCategoryId),
            "未选择、空值与显式“跟随分类”都必须按跟随分类处理。");
        Require(
            !BackgroundSkinPolicy.IsFollowCategory("__base__") &&
            !BackgroundSkinPolicy.IsFollowCategory(encoded),
            "明确选了“游戏默认”或某个 mod 背景时不能再回退成跟随分类。");
        Require(
            !BackgroundSkinPolicy.IsFollowCategory(BackgroundSkinPolicy.ModDefaultId) &&
            !BackgroundSkinPolicy.TryParseProviderOptionId(
                BackgroundSkinPolicy.ModDefaultId,
                out _,
                out _),
            "“模组默认”是独立选项，不能与跟随分类或提供者背景混淆。");

        Require(
            BackgroundSkinPolicy.DisplayName("b_laino_outdoor2_2") == "Laino Outdoor2 2",
            "战场名必须去掉 b_ 前缀并把分隔符转成可读文字。");
        Require(
            BackgroundSkinPolicy.DisplayName("ellinad_weak_standard_combat_pool") ==
            "Ellinad Weak Standard Combat Pool",
            "池型战场也必须给出稳定可读名称。");

        Console.WriteLine(
            "Background skin policy passed: stable option ids, follow-category default and readable names.");
    }
}
