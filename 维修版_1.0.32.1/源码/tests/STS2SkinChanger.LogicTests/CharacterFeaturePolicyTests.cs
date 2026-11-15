using STS2SkinChanger.Core.Features.Character;

internal static class CharacterFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        Require(CharacterFeaturePolicy.ShouldBeActive(null, baseId) == null,
            "目录未就绪时保留未知状态。");
        Require(CharacterFeaturePolicy.ShouldBeActive([], baseId) == false,
            "没有任何分组时不需要角色模块。");
        Require(CharacterFeaturePolicy.ShouldBeActive(
            [("ironclad", true, false, new[] { baseId, "" })], baseId) == false,
            "角色组只有原版或空选项时不激活。");
        Require(CharacterFeaturePolicy.ShouldBeActive(
            [("ironclad", true, false, new[] { baseId, "skin" })], baseId) == true,
            "角色组有皮肤就激活。");
        Require(CharacterFeaturePolicy.ShouldBeActive(
            [("osty", false, true, new[] { baseId, "midori_black" })], baseId) == true,
            "只有随从组有皮肤也算需要角色模块。");
        Require(CharacterFeaturePolicy.ShouldBeActive(
            [("merchant", false, false, new[] { "Merchant2CuteII" }),
             ("ancients", false, false, new[] { "custom" })], baseId) == false,
            "非角色、非随从的分组不触发角色模块。");
        Console.WriteLine("Character feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
