using STS2SkinChanger.Core.Features.StatefulCard;

internal static class StatefulCardFeaturePolicyTests
{
    internal static void Run()
    {
        Require(StatefulCardFeaturePolicy.ShouldBeActive(null) == null, "目录未就绪时保留未知状态。");
        Require(StatefulCardFeaturePolicy.ShouldBeActive([]) == false, "没有卡图选项时不激活。");
        Require(StatefulCardFeaturePolicy.ShouldBeActive([false, false]) == false, "只有普通卡图包时不激活。");
        Require(StatefulCardFeaturePolicy.ShouldBeActive([false, true]) == true, "装了任何带状态卡图包就激活。");
        Console.WriteLine("Stateful card feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
