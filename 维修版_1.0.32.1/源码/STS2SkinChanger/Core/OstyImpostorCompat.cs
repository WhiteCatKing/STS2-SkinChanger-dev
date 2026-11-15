using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// 「独立随从 Mod」兼容的对外门面（历史上只有「让鸡煲撑在地上假装自己是奥斯提」，
/// 所以名字仍保留 Osty，但实现已经完全通用：见 <see cref="SummonModCompat"/>）。
///
/// 两种随从皮肤来源的完整说明、自动发现规则与"下一个同类 Mod 怎么零改动接入"都写在
/// <see cref="SummonModCompat"/> 的文档注释里。这里只转发，保证既有调用点语义不变。
/// </summary>
internal static class OstyImpostorCompat
{
    private const string GroupId = "osty";

    /// <summary>是否有"自己接管随从身体"的第三方 Mod 装着。</summary>
    internal static bool IsInstalled => SummonModCompat.HasBodyOwner(GroupId);

    /// <summary>
    /// 玩家是否已经把随从外观当成独立选项（在「奥斯提下拉」里选过）。
    /// 为 false 时保留旧行为：这类 Mod 装了就把 osty 让给它；
    /// 为 true 时 osty 完全由本 Mod 的选择决定，它只在玩家主动选它的皮肤时接管。
    /// </summary>
    internal static bool Decoupled
    {
        get => SummonModCompat.IsDecoupled(GroupId);
        set => SummonModCompat.SetDecoupled(GroupId, value);
    }

    /// <summary>
    /// 随从的最终外观是否由第三方 Mod 产生：玩家选了它的皮肤，或分组停在"游戏默认"而它装着。
    /// 它的补丁替换整条身体分支，我们的重建会把它装好的 sprite 丢掉，两条路径都必须让开。
    /// </summary>
    internal static bool OwnsSummonBody(string groupId) =>
        groupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase) &&
        SummonModCompat.OwnsBody(groupId);

    /// <summary>本 Mod 是否必须把这个随从分组整个让出去。</summary>
    internal static bool MustDefer(string groupId) =>
        groupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase) &&
        SummonModCompat.MustDefer(groupId);
}
