namespace STS2SkinChanger.Core;

/// <summary>
/// 一次外观重建要激活哪些提供者、哪些留到菜单出来以后再初始化。
/// </summary>
internal sealed record ProviderActivationSchedule(
    IReadOnlyList<string> Immediate,
    IReadOnlyList<string> Deferred)
{
    public bool HasDeferred => Deferred.Count > 0;
}

/// <summary>
/// 皮肤提供者的 DLL 初始化（作者初始化器 + Harmony 补丁）是换肤流程里最贵的一段：本机 22 个
/// 提供者合计约 3.9 s，其中单个大包就要 1.4 s。这些工作以前全部压在启动的那一次重建上，
/// 结果是"游戏窗口出来了但主菜单很久不响应"。
///
/// 启动时还没有任何场景范围（<c>SkinService._runtimeProviderBehaviorScope == null</c>），
/// 屏幕上也没有角色或怪物，所以没有必要在那一刻初始化"只看战斗/事件/卡牌才用得到"的提供者：
/// 它们可以等主菜单出来以后在后台逐个初始化，而真正要用到它们的时候，场景范围（选角预览、
/// 对局角色、战斗房间、商人……）会先把可见的提供者同步激活，再实例化皮肤场景。
///
/// 因此这里只做一件事：把"本次要激活的提供者"拆成立刻同步和稍后后台两批，并且**只在还没有
/// 场景范围时**才拆。一旦有了范围，可见提供者必须同步激活——场景随时会用到它们的补丁。
/// </summary>
internal static class ProviderActivationSchedulePolicy
{
    /// <param name="activeProviderIds">
    /// 这次重建按口径要激活的提供者（已经有场景范围时就是范围筛选后的可见提供者）。
    /// </param>
    /// <param name="isFirstScreenProvider">
    /// 该提供者是否被选中在"玩家一进游戏就会看到的界面"上（角色、随从、商人外观）。
    /// </param>
    /// <param name="deferNonFirstScreen">
    /// 是否允许延后。只有启动/主菜单这种"没有任何场景范围"的重建才传 true。
    /// </param>
    public static ProviderActivationSchedule Plan(
        IEnumerable<string> activeProviderIds,
        Func<string, bool> isFirstScreenProvider,
        bool deferNonFirstScreen)
    {
        var immediate = new List<string>();
        var deferred = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var providerId in activeProviderIds)
        {
            if (string.IsNullOrWhiteSpace(providerId) || !seen.Add(providerId))
            {
                continue;
            }

            if (!deferNonFirstScreen || isFirstScreenProvider(providerId))
            {
                immediate.Add(providerId);
            }
            else
            {
                deferred.Add(providerId);
            }
        }

        return new ProviderActivationSchedule(immediate, deferred);
    }
}
