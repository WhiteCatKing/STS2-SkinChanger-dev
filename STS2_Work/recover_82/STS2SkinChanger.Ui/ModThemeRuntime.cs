using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class ModThemeRuntime
{
	private static readonly Lazy<ModThemeSession> Editing = new Lazy<ModThemeSession>(() => new ModThemeSession(ModThemeStore.Load(Path)));

	public static string Path => SkinChangerPaths.Configuration("skin_changer_theme.json");

	public static ModThemeSession Session => Editing.Value;

	public static ModThemeSettings Current => Session.Current;

	public static Color Text => new Color(Current.TextColor);

	public static Color Accent => new Color(Current.AccentColor);

	public static Color Tint(string color, float alpha)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new Color(new Color(color), alpha);
	}

	public static void Bind(Node owner, string key, Action<ModThemeSettings> refresh)
	{
		string text = "SCTheme_" + key;
		ModThemeBinding modThemeBinding = owner.GetNodeOrNull<ModThemeBinding>(NodePath.op_Implicit(text));
		if (modThemeBinding == null)
		{
			ModThemeBinding modThemeBinding2 = new ModThemeBinding();
			((Node)modThemeBinding2).Name = StringName.op_Implicit(text);
			modThemeBinding2.Refresh = refresh;
			modThemeBinding = modThemeBinding2;
			owner.AddChild((Node)(object)modThemeBinding, false, (InternalMode)0);
		}
		else
		{
			modThemeBinding.Refresh = refresh;
		}
		modThemeBinding.Apply();
	}

	public static void ApplyStyle(StyleBoxFlat style, ModThemeSurface surface, ModThemeSettings theme)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxFlat val = style;
		val.BgColor = (Color)(surface switch
		{
			ModThemeSurface.Panel => Tint(theme.PanelColor, theme.PanelOpacity), 
			ModThemeSurface.Hover => Tint(theme.HoverColor, theme.ButtonOpacity), 
			ModThemeSurface.Selected => Tint(theme.SelectionColor, theme.SelectionOpacity), 
			ModThemeSurface.Disabled => Tint(theme.ButtonColor, theme.ButtonOpacity * 0.4f), 
			ModThemeSurface.Focus => Colors.Transparent, 
			_ => Tint(theme.ButtonColor, theme.ButtonOpacity), 
		});
		val = style;
		bool flag = ((surface == ModThemeSurface.Selected || surface == ModThemeSurface.Focus) ? true : false);
		val.BorderColor = new Color(flag ? theme.AccentColor : theme.BorderColor);
		int borderWidth = theme.BorderWidth;
		int num2 = (style.BorderWidthBottom = borderWidth);
		int num4 = (style.BorderWidthTop = num2);
		int borderWidthLeft = (style.BorderWidthRight = num4);
		style.BorderWidthLeft = borderWidthLeft;
		num2 = (style.CornerRadiusBottomRight = theme.CornerRadius);
		num4 = (style.CornerRadiusBottomLeft = num2);
		borderWidthLeft = (style.CornerRadiusTopRight = num4);
		style.CornerRadiusTopLeft = borderWidthLeft;
		float contentMarginLeft = (((StyleBox)style).ContentMarginRight = 12f);
		((StyleBox)style).ContentMarginLeft = contentMarginLeft;
	}

	public static void Panel(PanelContainer panel)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxFlat style = new StyleBoxFlat();
		ModThemeBackdrop background = ModThemeBackdrop.For((Control)(object)panel);
		((Control)panel).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)style);
		Bind((Node)(object)panel, "panel", delegate(ModThemeSettings theme)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			ApplyStyle(style, ModThemeSurface.Panel, theme);
			style.DrawCenter = false;
			background.Update(Tint(theme.PanelColor, theme.PanelOpacity), theme.PanelBlur, theme.CornerRadius);
			BindPanelText((Node)(object)panel);
		});
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)panel) && !((GodotObject)panel).IsQueuedForDeletion())
			{
				BindPanelText((Node)(object)panel);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void BindPanelText(Node owner)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in owner.GetChildren(false))
		{
			if (((object)child.Name).ToString().StartsWith("SCTheme", StringComparison.Ordinal))
			{
				continue;
			}
			LineEdit val = (LineEdit)(object)((child is LineEdit) ? child : null);
			if (val != null && ((Node)val).GetNodeOrNull<ModThemeBinding>(NodePath.op_Implicit("SCTheme_input")) == null)
			{
				int fontSize;
				if (!((GodotObject)val).HasMeta(StringName.op_Implicit("sc_theme_base_font_size")))
				{
					fontSize = ((Control)val).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null);
				}
				else
				{
					StringName obj = StringName.op_Implicit("sc_theme_base_font_size");
					Variant val2 = default(Variant);
					val2 = ((GodotObject)val).GetMeta(obj, val2);
					fontSize = ((Variant)(ref val2)).AsInt32();
				}
				Input(val, fontSize, ((Control)val).GetThemeColor(StringName.op_Implicit("font_color"), (StringName)null) == Accent);
			}
			if ((child is Label || child is Button) ? true : false)
			{
				Control val3 = (Control)(object)((child is Control) ? child : null);
				if (val3 != null && ((Node)val3).GetNodeOrNull<ModThemeBinding>(NodePath.op_Implicit("SCTheme_text")) == null)
				{
					Color themeColor = val3.GetThemeColor(StringName.op_Implicit("font_color"), (StringName)null);
					bool flag = themeColor == new Color("fff6e2") || themeColor == Colors.White;
					bool flag2 = themeColor == new Color("efc850");
					TextControl(val3, val3.GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null), flag2, !flag && !flag2);
				}
			}
			if (!(child is ModThemeBinding))
			{
				BindPanelText(child);
			}
		}
	}

	public static void Button(Button button, int fontSize)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		(string Name, ModThemeSurface Surface, StyleBoxFlat Style)[] styles = new(string, ModThemeSurface, StyleBoxFlat)[6]
		{
			("normal", ModThemeSurface.Button, new StyleBoxFlat()),
			("hover", ModThemeSurface.Hover, new StyleBoxFlat()),
			("pressed", ModThemeSurface.Selected, new StyleBoxFlat()),
			("hover_pressed", ModThemeSurface.Hover, new StyleBoxFlat()),
			("focus", ModThemeSurface.Focus, new StyleBoxFlat()),
			("disabled", ModThemeSurface.Disabled, new StyleBoxFlat())
		};
		(string, ModThemeSurface, StyleBoxFlat)[] array = styles;
		for (int i = 0; i < array.Length; i++)
		{
			var (text, _, val) = array[i];
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)val);
		}
		ModThemeBackdrop background = ModThemeBackdrop.For((Control)(object)button);
		if (!((GodotObject)button).HasMeta(StringName.op_Implicit("sc_theme_button_draw")))
		{
			((GodotObject)button).SetMeta(StringName.op_Implicit("sc_theme_button_draw"), Variant.op_Implicit(true));
			((CanvasItem)button).Draw += RefreshBackground;
		}
		Bind((Node)(object)button, "button", delegate(ModThemeSettings theme)
		{
			(string, ModThemeSurface, StyleBoxFlat)[] array2 = styles;
			for (int j = 0; j < array2.Length; j++)
			{
				(string, ModThemeSurface, StyleBoxFlat) tuple2 = array2[j];
				ModThemeSurface item = tuple2.Item2;
				StyleBoxFlat item2 = tuple2.Item3;
				ApplyStyle(item2, item, theme);
				item2.DrawCenter = false;
			}
			RefreshBackground();
		});
		TextControl((Control)(object)button, fontSize);
		void RefreshBackground()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Invalid comparison between Unknown and I8
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Invalid comparison between Unknown and I8
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)button) && !((GodotObject)button).IsQueuedForDeletion())
			{
				ModThemeSettings current = Current;
				DrawMode drawMode = ((BaseButton)button).GetDrawMode();
				bool flag = (((long)drawMode == 1 || (long)drawMode == 4) ? true : false);
				bool pressed = flag;
				background.Update(ButtonTint(current, ((BaseButton)button).IsHovered(), pressed, ((BaseButton)button).Disabled), current.ButtonBlur, current.CornerRadius);
			}
		}
	}

	internal static Color ButtonTint(ModThemeSettings theme, bool hovered, bool pressed, bool disabled)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		return Tint((!pressed) ? ((hovered && !disabled) ? theme.HoverColor : theme.ButtonColor) : ((hovered && !disabled) ? theme.SelectionHoverColor : theme.SelectionColor), ((!pressed) ? theme.ButtonOpacity : ((hovered && !disabled) ? theme.SelectionHoverOpacity : theme.SelectionOpacity)) * (disabled ? 0.4f : 1f));
	}

	public static void Input(LineEdit input, int fontSize, bool accent = false)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		ModThemeBackdrop background = ModThemeBackdrop.For((Control)(object)input);
		(string Name, ModThemeSurface Surface, StyleBoxFlat Style)[] styles = new(string, ModThemeSurface, StyleBoxFlat)[3]
		{
			("normal", ModThemeSurface.Button, new StyleBoxFlat()),
			("read_only", ModThemeSurface.Disabled, new StyleBoxFlat()),
			("focus", ModThemeSurface.Focus, new StyleBoxFlat())
		};
		(string, ModThemeSurface, StyleBoxFlat)[] array = styles;
		for (int i = 0; i < array.Length; i++)
		{
			var (text, _, val) = array[i];
			((Control)input).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)val);
		}
		if (!((GodotObject)input).HasMeta(StringName.op_Implicit("sc_theme_input_draw")))
		{
			((GodotObject)input).SetMeta(StringName.op_Implicit("sc_theme_input_draw"), Variant.op_Implicit(true));
			((CanvasItem)input).Draw += RefreshBackground;
		}
		Bind((Node)(object)input, "input", delegate(ModThemeSettings theme)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			(string, ModThemeSurface, StyleBoxFlat)[] array2 = styles;
			for (int j = 0; j < array2.Length; j++)
			{
				(string, ModThemeSurface, StyleBoxFlat) tuple2 = array2[j];
				ModThemeSurface item = tuple2.Item2;
				StyleBoxFlat item2 = tuple2.Item3;
				ApplyStyle(item2, item, theme);
				item2.DrawCenter = false;
			}
			((Control)input).AddThemeColorOverride(StringName.op_Implicit("font_placeholder_color"), new Color(theme.TextColor));
			((Control)input).AddThemeColorOverride(StringName.op_Implicit("selection_color"), Tint(theme.SelectionColor, theme.SelectionOpacity));
			((Control)input).AddThemeColorOverride(StringName.op_Implicit("font_selected_color"), new Color(accent ? theme.AccentColor : theme.TextColor));
			RefreshBackground();
		});
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)input).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		TextControl((Control)(object)input, fontSize, accent);
		void RefreshBackground()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)input) && !((GodotObject)input).IsQueuedForDeletion())
			{
				ModThemeSettings current = Current;
				background.Update(ButtonTint(current, hovered: false, pressed: false, !input.Editable), current.ButtonBlur, current.CornerRadius);
			}
		}
	}

	public static void TextControl(Control control, int fontSize, bool accent = false, bool preserveTextColor = false)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)control).SetMeta(StringName.op_Implicit("sc_theme_base_font_size"), Variant.op_Implicit(fontSize));
		Dictionary<string, Color> lastColors = new Dictionary<string, Color>();
		int lastSize = -1;
		Bind((Node)(object)control, "text", delegate(ModThemeSettings theme)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			if (!preserveTextColor)
			{
				Color("font_color", new Color(accent ? theme.AccentColor : theme.TextColor));
			}
			Color("font_hover_color", new Color(accent ? theme.AccentColor : theme.TextColor));
			Color("font_focus_color", new Color(accent ? theme.AccentColor : theme.TextColor));
			Color("font_pressed_color", new Color(theme.AccentColor));
			Color("font_hover_pressed_color", new Color(theme.AccentColor));
			if (control is LineEdit)
			{
				Color("font_uneditable_color", new Color(accent ? theme.AccentColor : theme.TextColor));
				Color("caret_color", new Color(theme.AccentColor));
			}
			if (control.GetThemeColor(StringName.op_Implicit("font_outline_color"), (StringName)null) != new Color("332f27"))
			{
				control.AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
			}
			if (control.GetThemeConstant(StringName.op_Implicit("outline_size"), (StringName)null) != theme.TextOutline)
			{
				control.AddThemeConstantOverride(StringName.op_Implicit("outline_size"), theme.TextOutline);
			}
			if (lastSize >= 0 && control.GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null) != lastSize)
			{
				fontSize = control.GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null);
			}
			lastSize = Math.Max(10, (int)MathF.Round((float)fontSize * theme.FontScale));
			if (control.GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null) != lastSize)
			{
				control.AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), lastSize);
			}
			ApplyTextShadow(control, theme);
		});
		void Color(string key, Color value)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			Color themeColor = control.GetThemeColor(StringName.op_Implicit(key), (StringName)null);
			if (!lastColors.TryGetValue(key, out var value2) || !(themeColor != value2))
			{
				if (themeColor != value || !control.HasThemeColorOverride(StringName.op_Implicit(key)))
				{
					control.AddThemeColorOverride(StringName.op_Implicit(key), value);
				}
				lastColors[key] = value;
			}
		}
	}

	public static void AccentText(Control control)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		int fontSize;
		if (!((GodotObject)control).HasMeta(StringName.op_Implicit("sc_theme_base_font_size")))
		{
			fontSize = control.GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null);
		}
		else
		{
			StringName obj = StringName.op_Implicit("sc_theme_base_font_size");
			Variant val = default(Variant);
			val = ((GodotObject)control).GetMeta(obj, val);
			fontSize = ((Variant)(ref val)).AsInt32();
		}
		TextControl(control, fontSize, accent: true);
	}

	internal static void ApplyTextShadow(Control control, ModThemeSettings theme)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		control.AddThemeColorOverride(StringName.op_Implicit("font_shadow_color"), Tint(theme.TextShadowColor, theme.TextShadowEnabled ? theme.TextShadowOpacity : 0f));
		control.AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_x"), theme.TextShadowOffsetX);
		control.AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_y"), theme.TextShadowOffsetY);
		control.AddThemeConstantOverride(StringName.op_Implicit("shadow_outline_size"), theme.TextShadowSize);
		Button val = (Button)(object)((control is Button) ? control : null);
		if (val != null)
		{
			ModThemeButtonShadow.Attach(val);
		}
	}

	public static void ListButton(Button button, bool selected, int fontSize)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		StyleBoxEmpty val = new StyleBoxEmpty();
		StyleBoxFlat focus = new StyleBoxFlat();
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover_pressed"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)focus);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)focus);
		Bind((Node)(object)button, "row", delegate(ModThemeSettings theme)
		{
			ApplyStyle(focus, ModThemeSurface.Focus, theme);
			focus.DrawCenter = false;
		});
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)button).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		TextControl((Control)(object)button, fontSize, selected);
	}

	public static void Popup(PopupMenu popup)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		ModThemeDropdownBackdrop.Attach(popup);
		StyleBoxFlat panel = new StyleBoxFlat();
		StyleBoxFlat hover = new StyleBoxFlat();
		((Window)popup).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)panel);
		((Window)popup).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)hover);
		Bind((Node)(object)popup, "popup", delegate(ModThemeSettings theme)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			ApplyDropdownStyle(panel, theme);
			ApplyDropdownStyle(hover, theme, hovered: true);
			((Window)popup).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color(theme.TextColor));
			((Window)popup).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), new Color(theme.TextColor));
			((Window)popup).AddThemeColorOverride(StringName.op_Implicit("font_separator_color"), new Color(theme.AccentColor));
			((Window)popup).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
			((Window)popup).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), theme.TextOutline);
			((Window)popup).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), (int)MathF.Round(22f * theme.FontScale));
		});
	}

	public static void ItemList(ItemList list)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		StyleBoxFlat[] styles = (StyleBoxFlat[])(object)new StyleBoxFlat[4]
		{
			new StyleBoxFlat(),
			new StyleBoxFlat(),
			new StyleBoxFlat(),
			new StyleBoxFlat()
		};
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)styles[0]);
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("hovered"), (StyleBox)(object)styles[1]);
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("selected"), (StyleBox)(object)styles[2]);
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("selected_focus"), (StyleBox)(object)styles[2]);
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("hovered_selected"), (StyleBox)(object)styles[3]);
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("hovered_selected_focus"), (StyleBox)(object)styles[3]);
		StyleBoxFlat focus = new StyleBoxFlat();
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)focus);
		Bind((Node)(object)list, "list", delegate(ModThemeSettings theme)
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			ApplyDropdownStyle(styles[0], theme);
			ApplyDropdownStyle(styles[1], theme, hovered: true);
			ApplyDropdownStyle(styles[2], theme, hovered: false, selected: true);
			ApplyDropdownStyle(styles[3], theme, hovered: true, selected: true);
			ApplyDropdownStyle(focus, theme);
			focus.DrawCenter = false;
			((Control)list).AddThemeColorOverride(StringName.op_Implicit("font_hovered_color"), new Color(theme.TextColor));
			((Control)list).AddThemeColorOverride(StringName.op_Implicit("font_selected_color"), new Color(theme.TextColor));
			((Control)list).AddThemeColorOverride(StringName.op_Implicit("font_hovered_selected_color"), new Color(theme.TextColor));
		});
		AttachDropdownListHost(list);
		TextControl((Control)(object)list, 19);
	}

	internal static Color DropdownTint(ModThemeSettings theme, bool hovered, bool selected)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!selected)
		{
			if (!hovered)
			{
				return Tint(theme.DropdownColor, theme.DropdownOpacity);
			}
			return Tint(theme.DropdownHoverColor, theme.DropdownHoverOpacity);
		}
		if (!hovered)
		{
			return Tint(theme.DropdownSelectionColor, theme.DropdownSelectionOpacity);
		}
		return Tint(theme.DropdownSelectionHoverColor, theme.DropdownSelectionHoverOpacity);
	}

	private static void ApplyDropdownStyle(StyleBoxFlat style, ModThemeSettings theme, bool hovered = false, bool selected = false)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		style.BgColor = DropdownTint(theme, hovered, selected);
		style.BorderColor = new Color(theme.DropdownBorderColor);
		int num = (style.BorderWidthBottom = theme.DropdownBorderWidth);
		int num3 = (style.BorderWidthTop = num);
		int borderWidthLeft = (style.BorderWidthRight = num3);
		style.BorderWidthLeft = borderWidthLeft;
		num = (style.CornerRadiusBottomRight = theme.DropdownCornerRadius);
		num3 = (style.CornerRadiusBottomLeft = num);
		borderWidthLeft = (style.CornerRadiusTopRight = num3);
		style.CornerRadiusTopLeft = borderWidthLeft;
		float contentMarginLeft = (((StyleBox)style).ContentMarginRight = 12f);
		((StyleBox)style).ContentMarginLeft = contentMarginLeft;
	}

	private static void AttachDropdownListHost(ItemList list)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		PanelContainer nativePanel;
		Color original;
		if (!((GodotObject)list).HasMeta(StringName.op_Implicit("sc_theme_list_host")))
		{
			((GodotObject)list).SetMeta(StringName.op_Implicit("sc_theme_list_host"), Variant.op_Implicit(true));
			nativePanel = null;
			original = Colors.White;
			((Node)list).TreeEntered += Refresh;
			((CanvasItem)list).VisibilityChanged += Refresh;
			((Node)list).TreeExiting += Restore;
			Refresh();
		}
		void Refresh()
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			Node parent = ((Node)list).GetParent();
			PopupMenu val = (PopupMenu)(object)((parent is PopupMenu) ? parent : null);
			if (val != null)
			{
				if (nativePanel == null)
				{
					nativePanel = ((IEnumerable)((Node)val).GetChildren(true)).OfType<PanelContainer>().FirstOrDefault();
					if (nativePanel == null)
					{
						return;
					}
					original = ((CanvasItem)nativePanel).Modulate;
				}
				if (GodotObject.IsInstanceValid((GodotObject)(object)nativePanel))
				{
					((CanvasItem)nativePanel).Modulate = (Color)(((CanvasItem)list).Visible ? new Color(original, 0f) : original);
				}
			}
		}
		void Restore()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)nativePanel))
			{
				((CanvasItem)nativePanel).Modulate = original;
			}
			nativePanel = null;
		}
	}
}
