namespace STS2SkinChanger.Core;

internal sealed record OnlineSkinSource(string ProviderId, ulong WorkshopItemId, string SafeResourceFingerprint, string SafeResourceManifest, string SafeResourceBindings);
