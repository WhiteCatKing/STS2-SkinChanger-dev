namespace STS2SkinChanger.Ui;

internal static class MerchantPreviewLayerPolicy
{
	internal const int NormalPreviewZIndex = 0;

	internal const int HighestCompendiumOverlayZIndex = 11;

	private const int OpenInventoryPreviewZIndex = 12;

	internal static MerchantPreviewLayerState Resolve(bool inventoryOpen, bool hasSkinOptions, bool actionSelectorRequested, bool compendiumVisible)
	{
		if (inventoryOpen)
		{
			return new MerchantPreviewLayerState(12, SkinSelectorVisible: false, ActionSelectorVisible: false, CompendiumBackEnabled: false, MerchantPreviewBackButtonHost.CompendiumOverlay);
		}
		return new MerchantPreviewLayerState(0, hasSkinOptions, actionSelectorRequested, compendiumVisible, MerchantPreviewBackButtonHost.InventorySubViewport);
	}
}
