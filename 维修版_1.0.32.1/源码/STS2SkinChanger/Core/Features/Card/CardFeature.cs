using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Card;

internal sealed class CardFeature : ISkinFeature
{
    public string Id => "card";

    public bool? ShouldBeActive()
    {
        var catalog = SkinService.Catalog;
        return CardFeaturePolicy.ShouldBeActive(
            catalog?.CardGroups.SelectMany(group => group.Options)
                .Concat(catalog.PckCardOptions)
                .Select(option => (option.Id,
                    option.NormalPortraits.Count > 0 || option.AncientPortraits.Count > 0 ||
                    option.Assets.Count > 0 || option.CardPresentations.Count > 0 ||
                    option.CardSurfaces.Count > 0 || option.StatefulArt != null)),
            SkinCatalog.BaseOptionId);
    }

    public bool IsInUse()
    {
        try
        {
            return NodeGuard.IsAlive(NRun.Instance) ||
                   CardSkinControls.HasOpenLibrary || CardInspectSkinControls.HasOpenInspect;
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
