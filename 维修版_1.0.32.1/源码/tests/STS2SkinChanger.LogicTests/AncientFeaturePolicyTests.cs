using STS2SkinChanger.Core.Features.Ancient;

internal static class AncientFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        var ancientIds = new[] { "darv", "neow", "nonupeipe", "orobas", "pael", "tanx", "tezcatara", "vakuu" };
        Require(AncientFeaturePolicy.ShouldBeActive(null, ancientIds, baseId) == null,
            "目录未就绪时保留未知状态。");
        Require(AncientFeaturePolicy.ShouldBeActive([], ancientIds, baseId) == false,
            "没有任何分组时不需要先古模块。");
        Require(AncientFeaturePolicy.ShouldBeActive(
            [("neow", new[] { baseId, "" })], ancientIds, baseId) == false,
            "先古分组只有原版或空选项时不激活。");
        Require(AncientFeaturePolicy.ShouldBeActive(
            [("vakuu", new[] { baseId, "neow-pck" })], ancientIds, baseId) == true,
            "先古分组有皮肤就激活。");
        Require(AncientFeaturePolicy.ShouldBeActive(
            [("NEOW", new[] { "custom" })], ancientIds, baseId) == true,
            "先古分组 id 大小写不敏感。");
        Require(AncientFeaturePolicy.ShouldBeActive(
            [("ironclad", new[] { "skin" }), ("merchant", new[] { "pck" }), ("cultists", new[] { "pck" })],
            ancientIds, baseId) == false,
            "角色、商人和怪物分组不触发先古模块。");
        Console.WriteLine("Ancient feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
