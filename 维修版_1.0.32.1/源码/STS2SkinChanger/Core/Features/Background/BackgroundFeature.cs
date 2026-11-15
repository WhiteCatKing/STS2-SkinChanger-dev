using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Background;

internal sealed class BackgroundFeature : ISkinFeature
{
    public string Id => "background";

    public bool? ShouldBeActive() => BackgroundFeaturePolicy.ShouldBeActive(
        SkinService.Catalog == null ? null : BackgroundSkinService.GetAvailableOptionIdsSnapshot(),
        SkinCatalog.BaseOptionId);

    public bool IsInUse()
    {
        try
        {
            if (BackgroundAppearanceScreen.IsOpen)
            {
                return true;
            }

            var room = NCombatRoom.Instance;
            return NodeGuard.IsAliveInTree(room) && BackgroundSkinService.IsBackgroundInUse(room);
        }
        catch
        {
            return true;
        }
    }

    public void OnActivated() => BackgroundSkinService.RefreshCurrentBackground();

    public void OnDeactivating()
    {
    }
}
