using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Ancient;

/// <summary>
/// 先古模块：先古场景动画与背景适配、场景构建、地图/历史图标这几个补丁（<c>Ui/AncientCompendium.cs</c> 末尾的 8 个类）。
/// 没装任何先古皮肤时整块不装。图鉴里的先古入口和其它的浏览入口留在主干，不归这里。
/// </summary>
internal sealed class AncientFeature : ISkinFeature
{
    public string Id => "ancient";

    public bool? ShouldBeActive() => AncientFeaturePolicy.ShouldBeActive(
        SkinService.Catalog?.Groups.Select(group =>
            (group.Id, (IEnumerable<string>)group.Options.Select(option => option.Id))),
        SkinCatalog.KnownAncientIds,
        SkinCatalog.BaseOptionId);

    // 先古事件房间开着，或图鉴里的先古预览还开着（弱引用记录，不遍历树），就先不卸。
    public bool IsInUse()
    {
        try
        {
            return NodeGuard.IsAliveInTree(NEventRoom.Instance) ||
                   AncientCompendiumScreen.HasLivePreview;
        }
        catch
        {
            return true;
        }
    }

    // 模块只在目录就绪、先古包增减时变化；在用时注册表不会卸，所以这里的空实现是够的。
    // 已经打开的先古事件不重建：重新 SetEvent 会重置选项和奖励，和事件模块同一理由。
    public void OnActivated()
    {
    }

    // 补丁只改返回值、构建场景、调一次节点适配，没有需要额外还原的状态；
    // 离开先古场景时的范围收尾由 AncientRuntimeProviderScopeExitPatch 负责，而在用时模块不会被卸。
    public void OnDeactivating()
    {
    }
}
