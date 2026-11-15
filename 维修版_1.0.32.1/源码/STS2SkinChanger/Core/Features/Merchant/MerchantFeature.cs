using Godot;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Merchant;

internal sealed class MerchantFeature : ISkinFeature
{
    public string Id => MerchantRuntimeAppearance.GroupId;

    public bool? ShouldBeActive() => MerchantFeaturePolicy.ShouldBeActive(
        SkinService.Catalog?.Groups.Select(group =>
            (group.Id, (IEnumerable<string>)group.Options.Select(option => option.Id))),
        SkinCatalog.BaseOptionId);

    public bool IsInUse()
    {
        if (NodeGuard.IsAliveInTree(NMerchantRoom.Instance))
        {
            return true;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || !NodeGuard.IsAlive(tree.Root))
        {
            return true;
        }

        var pending = new Stack<Node>();
        pending.Push(tree.Root);
        while (pending.TryPop(out var node))
        {
            if (!NodeGuard.IsAlive(node))
            {
                continue;
            }

            if (node is NMerchantRoom or NFakeMerchant ||
                MerchantRuntimeAppearance.IsMerchantPreviewRoot(node))
            {
                return true;
            }

            foreach (var child in node.GetChildren())
            {
                pending.Push(child);
            }
        }

        return false;
    }

    public void OnActivated()
    {
        if (NodeGuard.IsAliveInTree(NMerchantRoom.Instance) &&
            !MerchantRuntimeAppearance.IsMerchantPreviewRoot(NMerchantRoom.Instance))
        {
            if (!MerchantRuntimeAppearance.TryRefreshMerchant(out var error))
            {
                ModLog.Warn("商人模块启用后的外观刷新失败：" + error);
            }
        }
    }

    public void OnDeactivating() => MerchantRuntimeAppearance.PrepareMerchantSelectionChange();
}
