using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Event;

/// <summary>
/// 事件皮肤模块：事件立绘 / 特效 / 文字的接管补丁（<c>EventSkinRuntime.cs</c> 里的四个类）。
/// 没装任何事件皮肤时整块不装，尤其是 <c>LocTable.GetRawText</c> 这个每次取文字都会走的前缀。
/// 图鉴里的事件预览补丁（<c>EventPreviewLifecyclePatch</c>）是浏览入口，所有事件都能预览，留在主干。
/// </summary>
internal sealed class EventFeature : ISkinFeature
{
    public string Id => "event";

    public bool? ShouldBeActive() => EventFeaturePolicy.ShouldBeActive(
        SkinService.Catalog?.Groups.Select(group =>
            (group.Id, (IEnumerable<string>)group.Options.Select(option => option.Id))),
        SkinCatalog.BaseOptionId);

    // 事件房间开着，或图鉴里的事件预览开着，就不卸（它们的文字、立绘、特效都靠这些补丁）。
    public bool IsInUse() =>
        NodeGuard.IsAliveInTree(NEventRoom.Instance) || EventCompendiumPreview.HasLivePreview;

    // 模块只在目录就绪、事件包增减时变化，不在事件房间里切换（在用时不会卸）。
    // 已经打开的事件不重建：重新 SetEvent 会重置选项和奖励，见 EventSkinRuntime.RefreshCurrent 的注释。
    public void OnActivated()
    {
    }

    // 补丁只改返回值和节点元数据，没有需要额外还原的东西。
    public void OnDeactivating()
    {
    }
}
