using CznCore.Presentation;

namespace CznCore.Settings;

internal sealed record CharacterSettingsUiProfile(string Key, ILocalizedText Header, ILocalizedText Target, string AvatarPath, string GlobalIconPath, bool SupportsMeleeApproachMovement, bool SupportsOfficialAttackTiming)
{
	public string AccentColor { get; init; } = "";
}
