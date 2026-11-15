using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Core.Features.StatefulCard;

/// <summary>
/// 带状态卡图模块（随卡牌状态换图、附带的出牌特效、悬停署名、消耗堆图标等）。
/// 这些补丁挂在很热的路径上：每次伤害结算（<c>AbstractModel.AfterDamageGiven</c>）、
/// 每次卡牌悬停（<c>CardModel.HoverTips</c>），每场战斗还会往消耗堆按钮上挂一个逐帧节点。
/// 没装带状态卡图的包时整块不装。
/// 提供者自己的 <c>NCardAwareCardsPatch.get_Node</c> 前缀是在建立它的运行时时才装（StatefulCardArtRuntime），本来就是按需的，不归这里管。
/// </summary>
internal sealed class StatefulCardFeature : ISkinFeature
{
    public string Id => "stateful-card";

    public bool? ShouldBeActive() => StatefulCardFeaturePolicy.ShouldBeActive(
        SkinService.Catalog?.PckCardOptions.Select(option => option.StatefulArt != null));

    // 保守判断：在局内就算在用（战斗特效、消耗堆图标、手牌状态图都在局内）。
    // 选角界面那条补丁只在开局时把提供者的手指位置复位，漏一次也不影响显示。
    // 模块只在带状态卡图的包增减时变化，实际上整局不会切换。
    public bool IsInUse() => NodeGuard.IsAliveInTree(NRun.Instance);

    public void OnActivated()
    {
    }

    public void OnDeactivating()
    {
    }
}
