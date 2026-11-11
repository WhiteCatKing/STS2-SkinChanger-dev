using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace PengoTarot.BalatroEffect;

public static class LocaleFontUtil
{
	public static Font? GetLocaleFont(FontType type)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (LocManager.Instance != null && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
		{
			return FontManager.GetSubstituteFont(LocManager.Instance.Language, type);
		}
		return null;
	}
}
