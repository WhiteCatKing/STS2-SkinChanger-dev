namespace STS2SkinChanger.Ui;

internal static class PauseMenuAppearanceEntryPolicy
{
	internal static PauseMenuAppearanceEntryDecision Resolve(bool showEntry, bool buttonExists)
	{
		return new PauseMenuAppearanceEntryDecision(showEntry && !buttonExists, showEntry);
	}
}
