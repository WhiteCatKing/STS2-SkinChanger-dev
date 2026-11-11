namespace STS2SkinChanger.Ui;

internal static class CompendiumSidebarPolicy
{
	public static float X(float viewportWidth, float panelWidth, bool expanded)
	{
		return viewportWidth - panelWidth * (expanded ? 1f : 0.1f);
	}

	public static float IdleX(float viewportWidth, float panelWidth, bool outer)
	{
		return viewportWidth - panelWidth * (outer ? 0.1f : 0.05f);
	}

	public static bool ShouldExpand(bool pointerInside, bool popupOpen, bool keyboardFocus, bool allowed)
	{
		if (allowed)
		{
			return pointerInside || popupOpen || keyboardFocus;
		}
		return false;
	}
}
