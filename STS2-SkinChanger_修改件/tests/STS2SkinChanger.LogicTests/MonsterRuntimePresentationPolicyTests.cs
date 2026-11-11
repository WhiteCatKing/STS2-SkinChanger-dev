using STS2SkinChanger.Core;

internal static class MonsterRuntimePresentationPolicyTests
{
    public static void Run()
    {
        static void Require(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }

        // A per-monster runtime provider draws the enemy itself; those callbacks must survive
        // the isolation pass or the whole enemy pack renders as the vanilla monsters.
        foreach (var callback in new[]
                 {
                     MonsterVisualCallback.MonsterVisualCreation,
                     MonsterVisualCallback.CreatureReady,
                     MonsterVisualCallback.BossMapPointReady
                 })
        {
            Require(
                MonsterRuntimePresentationPolicy.ProviderKeepsVisualCallback(true, callback),
                $"按怪物运行时配置绘制的提供者必须保留自己的 {callback} 回调。");
            Require(
                !MonsterRuntimePresentationPolicy.ProviderKeepsVisualCallback(false, callback),
                "普通资源皮肤仍由皮肤切换器接管外观创建入口。");
        }

        Require(
            !MonsterRuntimePresentationPolicy.ProviderKeepsVisualCallback(
                true,
                MonsterVisualCallback.None),
            "与怪物呈现无关的补丁不受此规则影响。");
        Require(
            MonsterRuntimePresentationPolicy.ProviderKeepsVisualCallback(
                true,
                MonsterVisualCallback.MonsterVisualCreation) &&
            !MonsterRuntimePresentationPolicy.ProviderKeepsVisualCallback(
                false,
                MonsterVisualCallback.MonsterVisualCreation),
            "只有声明了逐怪物运行时配置的提供者才拥有外观创建入口。");

        Require(
            MonsterRuntimePresentationPolicy.DefersVisualReplacementToProvider(true),
            "逐怪物运行时皮肤已由作者代码创建，不能再被规范场景替换。");
        Require(
            !MonsterRuntimePresentationPolicy.DefersVisualReplacementToProvider(false),
            "普通资源皮肤仍需按当前选择实例化规范场景。");

        Console.WriteLine(
            "Monster runtime presentation passed: per-monster providers keep their own visual " +
            "callbacks while resource skins stay managed.");
    }
}
