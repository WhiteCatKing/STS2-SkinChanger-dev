namespace STS2SkinChanger.Core;

/// <summary>
/// Whether a merchant skin's own figure transform survives the generic restore of a provider's
/// node-ready changes.
///
/// Merchant skins (Merchant2CuteII, ATA and similar) apply their figure's position and scale from a
/// prefix on the native button's <c>_Ready</c>. Skin Changer removes that callback from Harmony and
/// replays it, so the transform exists only as a tracked mutation of <c>%MerchantVisual</c>. The run
/// narrows provider behaviour down to the characters currently on screen — a shop is not one of them
/// — and deactivating a provider restores every mutation it made. Handing the vanilla transform back
/// while the skin's own skeleton is still on screen leaves the figure at the vanilla scale (roughly
/// two and a half times the skin's) and at the vanilla offset, which sits far up and to the left:
/// exactly the reported "merchant grew and jumped to the top-left corner".
///
/// The transform may only be kept while the very same provider is still what the merchant group
/// selects. A real deselection, another provider, or a rebuilt button keeps the ordinary restore.
/// Type and value only, so the rule can be verified without a game.
/// </summary>
internal static class MerchantFigureRestorePolicy
{
    internal static bool KeepsTransform(
        string? nodeOwnerProviderId,
        string providerBeingDeactivated,
        bool providerStillSelected)
    {
        if (string.IsNullOrEmpty(nodeOwnerProviderId) ||
            string.IsNullOrEmpty(providerBeingDeactivated))
        {
            return false;
        }

        return providerStillSelected &&
               nodeOwnerProviderId.Equals(
                   providerBeingDeactivated,
                   StringComparison.OrdinalIgnoreCase);
    }
}
