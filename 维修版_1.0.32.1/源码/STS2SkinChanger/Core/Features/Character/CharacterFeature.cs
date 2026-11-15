using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Core.Features.Character;

/// <summary>
/// 角色模块：角色战斗外观、拖尾、能量颜色/图层、入场动画、动画触发桥，以及角色选择图标/地图标记这些按需补丁
/// （见 <c>character-migration.md</c> 的 16 个类）。启动时先装上；目录就绪后，如果没有任何角色或随从皮肤，就在局外卸掉。
/// 选择、存读档、共享作用域和手动安装桥留在主干，不归这里。
/// </summary>
internal sealed class CharacterFeature : ISkinFeature
{
    public string Id => "character";

    public bool? ShouldBeActive()
    {
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return null;
        }

        return CharacterFeaturePolicy.ShouldBeActive(
            catalog.Groups.Select(group =>
                (group.Id,
                    catalog.IsCharacterAppearanceGroup(group.Id),
                    catalog.IsCompanionGroup(group.Id),
                    (IEnumerable<string>)group.Options.Select(option => option.Id))),
            SkinCatalog.BaseOptionId);
    }

    // 局内（NRun 活着）算在用：角色、随从外观随时重建，缺头 VFX 守卫也要跟着活节点。
    // 选人界面不单独判断：只有目录里已经没有任何角色 / 随从皮肤时才会走到卸载，
    // 这时这些补丁本来就不改任何东西，在选人界面卸掉也安全。
    // （Claude 11-15 改：原来恒为 true，注册表在启动时先装好补丁，这样会一直卸不掉。）
    public bool IsInUse()
    {
        try
        {
            return NodeGuard.IsAliveInTree(NRun.Instance);
        }
        catch
        {
            return true;
        }
    }

    // 模块只在目录就绪、角色/随从包增减时变化；在用时注册表不会卸，所以空实现够用。
    public void OnActivated()
    {
    }

    // 补丁只改返回值、重建外观、调动画触发器，没有需要额外还原的状态。
    public void OnDeactivating()
    {
    }
}
