namespace STS2SkinChanger.Core;

internal sealed record RuntimeProviderPriorityCandidate(string ProviderId, bool Enabled, bool IsRunWideMonsterProvider, bool AppliesToCurrentCombat = true);
