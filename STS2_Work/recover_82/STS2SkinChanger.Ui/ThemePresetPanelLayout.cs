using System;
using Godot;

namespace STS2SkinChanger.Ui;

internal static class ThemePresetPanelLayout
{
	internal static (Vector2 ThemePosition, Rect2 Preset) Place(Vector2 viewport, Rect2 theme, Vector2 desiredSize)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Max(1f, Math.Min(desiredSize.X, viewport.X - 24f - 12f - ((Rect2)(ref theme)).Size.X));
		float num2 = Math.Max(1f, Math.Min(desiredSize.Y, viewport.Y - 24f));
		float num3 = Math.Clamp(((Rect2)(ref theme)).Position.X, 12f, Math.Max(12f, viewport.X - 12f - ((Rect2)(ref theme)).Size.X - 12f - num));
		float num4 = 12f + Math.Max(0f, num2 - ((Rect2)(ref theme)).Size.Y);
		float num5 = Math.Clamp(((Rect2)(ref theme)).Position.Y, num4, Math.Max(num4, viewport.Y - 12f - ((Rect2)(ref theme)).Size.Y));
		return (ThemePosition: new Vector2(num3, num5), Preset: new Rect2(num3 + ((Rect2)(ref theme)).Size.X + 12f, num5 + ((Rect2)(ref theme)).Size.Y - num2, num, num2));
	}
}
