using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Monster;

/// <summary>
/// 怪物模块：真正换怪物外观的只有 <c>ContextualSkinControls.cs</c> 里的 <c>MonsterVisualResultPatch</c>
/// （<c>MonsterModel.CreateVisuals</c>）。怪物图鉴的入口、下拉、范围补丁都是选择入口，留在主干；
/// 和商人/假商人相关的分组归商人模块。
/// </summary>
internal sealed class MonsterFeature : ISkinFeature
{
    private const string MonsterAssetPrefix = "res://animations/monsters/";

    public string Id => "monster";

    public bool? ShouldBeActive()
    {
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return null;
        }

        // 普通 PCK 怪物组：目录选项里带 res://animations/monsters/<id>/ 资源。它们没有运行时元数据，
        // 只靠 Config.MonsterSkinCategoryGroups 快照会漏（那个表要等分类登记才有内容）。
        var registered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ids in SkinService.Config.MonsterSkinCategoryGroups.Values)
        {
            registered.UnionWith(ids);
        }

        return MonsterFeaturePolicy.ShouldBeActive(
            catalog.Groups.Select(group =>
                (group.Id,
                    (IEnumerable<string>)group.Options.Select(option => option.Id),
                    group.Options.Any(option =>
                        option.ManagedMonsterScene != null ||
                        option.RuntimeMonsterVisualMode != null ||
                        option.Assets.Keys.Any(path =>
                            path.StartsWith(MonsterAssetPrefix, StringComparison.OrdinalIgnoreCase))))),
            registered,
            SkinCatalog.BaseOptionId);
    }

    // 保守判断：在局内（战斗里随时会创建怪物）或怪物图鉴开着，就算在用。
    // 模块只在怪物包增减时变化，实际上一局里不会切换。
    public bool IsInUse()
    {
        try
        {
            return NodeGuard.IsAliveInTree(NRun.Instance) ||
                   ContextualSkinControls.HasOpenBestiary;
        }
        catch
        {
            return true;
        }
    }

    public void OnActivated()
    {
    }

    public void OnDeactivating()
    {
    }
}
