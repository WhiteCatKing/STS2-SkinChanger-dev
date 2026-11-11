namespace STS2SkinChanger.Core;

internal sealed record SlotVisibilitySelection(string GroupId, string ProviderId, string ToggleId, bool Hidden, string[] SourceSlots);
