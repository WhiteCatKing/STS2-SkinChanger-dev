using System;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal sealed record ModThemeSettings
{
	public string PanelColor { get; init; } = "#FFFFFF";

	public float PanelOpacity { get; init; } = 0.2f;

	public float PanelBlur { get; init; } = 3f;

	public string SelectionColor { get; init; } = "#FFFFFF";

	public float SelectionOpacity { get; init; } = 0.5f;

	public float SelectionBlur { get; init; } = 2f;

	public string SelectionHoverColor { get; init; } = "#FFFFFF";

	public float SelectionHoverOpacity { get; init; } = 0.5f;

	public float SelectionHoverBlur { get; init; } = 2f;

	public string ButtonColor { get; init; } = "#FFFFFF";

	public float ButtonOpacity { get; init; } = 0.21f;

	public float ButtonBlur { get; init; } = 2f;

	public string HoverColor { get; init; } = "#DFDFDF";

	public string DropdownColor { get; init; } = "#FFFFFF";

	public float DropdownOpacity { get; init; } = 0.19f;

	public float DropdownBlur { get; init; } = 3f;

	public string DropdownHoverColor { get; init; } = "#DFDFDF";

	public float DropdownHoverOpacity { get; init; } = 0.5f;

	public string DropdownSelectionColor { get; init; } = "#FFFFFF";

	public float DropdownSelectionOpacity { get; init; } = 0.3f;

	public string DropdownSelectionHoverColor { get; init; } = "#DFDFDF";

	public float DropdownSelectionHoverOpacity { get; init; } = 0.5f;

	public string DropdownBorderColor { get; init; } = "#D3D3D3";

	public int DropdownBorderWidth { get; init; }

	public int DropdownCornerRadius { get; init; } = 10;

	public string TextColor { get; init; } = "#FFF6E2";

	public string AccentColor { get; init; } = "#FFEAA9";

	public string BorderColor { get; init; } = "#D3D3D3";

	public int BorderWidth { get; init; }

	public int CornerRadius { get; init; } = 10;

	public float FontScale { get; init; } = 1f;

	public int TextOutline { get; init; } = 5;

	public bool TextShadowEnabled { get; init; } = true;

	public string TextShadowColor { get; init; } = "#000000";

	public float TextShadowOpacity { get; init; } = 0.5f;

	public int TextShadowOffsetX { get; init; } = 2;

	public int TextShadowOffsetY { get; init; } = 2;

	public int TextShadowSize { get; init; } = 3;

	public ModThemeSettings Normalize()
	{
		ModThemeSettings modThemeSettings = new ModThemeSettings();
		return this with
		{
			PanelColor = Hex(PanelColor, modThemeSettings.PanelColor),
			SelectionColor = Hex(SelectionColor, modThemeSettings.SelectionColor),
			SelectionHoverColor = Hex(SelectionHoverColor, modThemeSettings.SelectionHoverColor),
			SelectionHoverOpacity = Number(SelectionHoverOpacity, 0f, 1f, modThemeSettings.SelectionHoverOpacity),
			SelectionHoverBlur = Number(SelectionHoverBlur, 0f, 5f, modThemeSettings.SelectionHoverBlur),
			ButtonColor = Hex(ButtonColor, modThemeSettings.ButtonColor),
			HoverColor = Hex(HoverColor, modThemeSettings.HoverColor),
			DropdownColor = Hex(DropdownColor, modThemeSettings.DropdownColor),
			DropdownHoverColor = Hex(DropdownHoverColor, modThemeSettings.DropdownHoverColor),
			DropdownSelectionColor = Hex(DropdownSelectionColor, modThemeSettings.DropdownSelectionColor),
			DropdownSelectionHoverColor = Hex(DropdownSelectionHoverColor, modThemeSettings.DropdownSelectionHoverColor),
			DropdownSelectionHoverOpacity = Number(DropdownSelectionHoverOpacity, 0f, 1f, modThemeSettings.DropdownSelectionHoverOpacity),
			DropdownBorderColor = Hex(DropdownBorderColor, modThemeSettings.DropdownBorderColor),
			DropdownOpacity = Number(DropdownOpacity, 0f, 1f, modThemeSettings.DropdownOpacity),
			DropdownBlur = Number(DropdownBlur, 0f, 5f, modThemeSettings.DropdownBlur),
			DropdownHoverOpacity = Number(DropdownHoverOpacity, 0f, 1f, modThemeSettings.DropdownHoverOpacity),
			DropdownSelectionOpacity = Number(DropdownSelectionOpacity, 0f, 1f, modThemeSettings.DropdownSelectionOpacity),
			DropdownBorderWidth = Math.Clamp(DropdownBorderWidth, 0, 5),
			DropdownCornerRadius = Math.Clamp(DropdownCornerRadius, 0, 24),
			TextColor = Hex(TextColor, modThemeSettings.TextColor),
			AccentColor = Hex(AccentColor, modThemeSettings.AccentColor),
			BorderColor = Hex(BorderColor, modThemeSettings.BorderColor),
			TextShadowColor = Hex(TextShadowColor, modThemeSettings.TextShadowColor),
			PanelOpacity = Number(PanelOpacity, 0f, 1f, modThemeSettings.PanelOpacity),
			SelectionOpacity = Number(SelectionOpacity, 0f, 1f, modThemeSettings.SelectionOpacity),
			ButtonOpacity = Number(ButtonOpacity, 0f, 1f, modThemeSettings.ButtonOpacity),
			PanelBlur = Number(PanelBlur, 0f, 5f, modThemeSettings.PanelBlur),
			SelectionBlur = Number(SelectionBlur, 0f, 5f, modThemeSettings.SelectionBlur),
			ButtonBlur = Number(ButtonBlur, 0f, 5f, modThemeSettings.ButtonBlur),
			TextShadowOpacity = Number(TextShadowOpacity, 0f, 1f, modThemeSettings.TextShadowOpacity),
			TextShadowOffsetX = Math.Clamp(TextShadowOffsetX, -12, 12),
			TextShadowOffsetY = Math.Clamp(TextShadowOffsetY, -12, 12),
			TextShadowSize = Math.Clamp(TextShadowSize, 0, 8),
			BorderWidth = Math.Clamp(BorderWidth, 0, 5),
			CornerRadius = Math.Clamp(CornerRadius, 0, 24),
			FontScale = Number(FontScale, 0.75f, 1.5f, 1f),
			TextOutline = Math.Clamp(TextOutline, 0, 8)
		};
	}

	private static string Hex(string? value, string fallback)
	{
		if (value == null || !Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$"))
		{
			return fallback;
		}
		return value.ToUpperInvariant();
	}

	private static float Number(float value, float min, float max, float fallback)
	{
		if (!float.IsFinite(value))
		{
			return fallback;
		}
		return Math.Clamp(value, min, max);
	}
}
